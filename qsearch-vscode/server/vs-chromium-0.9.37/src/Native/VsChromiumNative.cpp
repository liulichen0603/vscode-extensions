// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

// VsChromiumNative.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include <assert.h>
#include <stdlib.h>

#include <algorithm>
#include <locale>

#include "search_bndm32.h"
#include "search_bndm64.h"
#include "search_boyer_moore.h"
#include "search_strstr.h"
#include "search_regex.h"
#include "search_re2.h"

#define EXPORT __declspec(dllexport)

namespace {

template<class CharType>
bool GetLineExtentFromPosition(
    const CharType* text,
    int textLen,
    int position,
    int maxOffset,
    int* lineStartPosition,
    int* lineLen) {
  const CharType nl = '\n';
  const CharType* low = max(text, text + position - maxOffset);
  const CharType* high = min(text + textLen, text + position + maxOffset);
  const CharType* current = text + position;

  // Search backward up to "low" included
  const CharType* start = current;
  if (start > low) {
    start--;
    for (; start >= low; start--) {
      if (*start == nl) {
        break;
      }
    }
    start++;
  }

  // Search forward up to "high" excluded
  const CharType* end = current;
  for (; end < high; end++) {
    if (*end == nl) {
      end++;
      break;
    }
  }

  assert(low <= start);
  assert(start <= high);
  assert(low <= end);
  assert(end <= high);

  // TODO(rpaquay): We are limited to 2GB for now.
  *lineStartPosition = static_cast<int>(start - text);
  *lineLen = static_cast<int>(end - start);
  return true;
}

bool char_equal_icase(wchar_t x , wchar_t y) {
  static const std::locale& loc(std::locale::classic());
  return std::tolower(x, loc) == std::tolower(y, loc);
}

bool Text_HasUtf8Bom(const char *text, int textLen) {
  return textLen >= 3 &&
    static_cast<uint8_t>(text[0]) == static_cast<uint8_t>(0xEF) &&
    static_cast<uint8_t>(text[1]) == static_cast<uint8_t>(0xBB) &&
    static_cast<uint8_t>(text[2]) == static_cast<uint8_t>(0xBF);
}

enum ContentKindResult {
  ResultAscii,
  ResultUtf8,
  ResultBinary
};

const uint8_t maskSeq2 = 0xE0; // 111x-xxxx
const uint8_t maskSeq3 = 0xF0; // 1111-xxxx
const uint8_t maskSeq4 = 0xF8; // 1111-1xxx
const uint8_t maskRest = 0xC0; // 11xx-xxxx

const uint8_t valueSeq2 = 0xC0; // 110x-xxxx
const uint8_t valueSeq3 = 0xE0; // 1110-xxxx
const uint8_t valueSeq4 = 0xF0; // 1111-0xxx
const uint8_t valueRest = 0x80; // 10xx-xxxx

bool isSeq(const uint8_t* ch, uint8_t mask, uint8_t value) {
  return (*ch & mask) == value;
}

bool isRest(const uint8_t* ch) {
  return (*ch & maskRest) == valueRest;
}

bool IsUtf8Rune(const uint8_t** pch, int* len) {
  const uint8_t* text = *pch;
  uint8_t ch = *text;
  if (isSeq(text, maskSeq4, valueSeq4) && (*len >= 4)) {
    if (isRest(text + 1) && isRest(text + 2) && isRest(text + 3)) {
      (*pch) = text + 4;
      (*len) -= 4;
      return true;
    }
  }
  else if (isSeq(text, maskSeq3, valueSeq3) && (*len >= 3)) {
    if (isRest(text + 1) && isRest(text + 2)) {
      (*pch) = text + 3;
      (*len) -= 3;
      return true;
    }
  }
  else if (isSeq(text, maskSeq2, valueSeq2) && (*len >= 2)) {
    if (isRest(text + 1)) {
      (*pch) = text + 2;
      (*len) -= 2;
      return true;
    }
  }
  return false;
}

struct ContentResult {
  ContentResult() {
    asciiCount = 0;
    utf8Count = 0;
    otherCount = 0;
  }
  // Count simple (i.e. readable) ascii characters
  int asciiCount;
  // Count extended UTF8 characters
  int utf8Count;
  // Count others (i.e. not in previous 2 categories)
  int otherCount;
};

void Text_ContentKindFull(const char* text, int textLen, ContentResult& result) {
  const uint8_t* textPtr = (const uint8_t *)text;
  while (textLen > 0) {
    uint8_t ch = *textPtr;

    // See http://www.asciitable.com/
    if ((ch >= 32 && ch <= 126) || ch == '\t' || ch == '\r' || ch == '\n') {
      result.asciiCount++;
      textPtr++;
      textLen--;
    }
    else if (IsUtf8Rune(&textPtr, &textLen)) {
      result.utf8Count++;
    }
    else {
      result.otherCount++;
      textPtr++;
      textLen--;
    }
  }
}

void Text_ContentKindSlices(const char* text, int textLen, ContentResult& result) {
  // Instead of going through the whole file contents (which can be big), we
  // limit ourselves to 50 slices of 4K each. This means we examine a maximum of
  // 200KB per file, whatever the size. For files smaller than 200KB, we still
  // examine the whole content.
  const int sliceCount = 50;
  const int sliceSize = 4 * 1024;
  int betweenSlicesCount = (textLen - sliceSize * sliceCount) / sliceCount;
  if (betweenSlicesCount <= 0) {
    Text_ContentKindFull(text, textLen, result);
    return;
  }

  const char* textLimit = text + textLen;
  while (textLen > 0) {
    int sliceLen = min(sliceSize, textLen);
    Text_ContentKindFull(text, sliceLen, result);

    text += (sliceLen + betweenSlicesCount);
    textLen -= (sliceLen + betweenSlicesCount);
  }
}

ContentKindResult ContentResultToContentKindResult(ContentResult contentResult) {
  int total = contentResult.asciiCount + contentResult.utf8Count + contentResult.otherCount;
  double otherRatio = (double)contentResult.otherCount / (double)total;

  // Content is considered "binary" if more than 10% of characters are non human readable
  if (otherRatio >= 0.1) {
    return ResultBinary;
  } else {
    // Content is considered "Text", choose between pure ascii or mix of ascii/utf8
    if (contentResult.utf8Count == 0.0)
      return ResultAscii;
    else
      return ResultUtf8;
  }
}

ContentKindResult Text_ContentKind(const char* text, int textLen) {
#if 1
  ContentResult contentResult;
  Text_ContentKindSlices(text, textLen, contentResult);
  return ContentResultToContentKindResult(contentResult);
#else
  ContentResult contentResult1;
  Text_ContentKindFull(text, textLen, contentResult1);
  ContentResult contentResult2;
  Text_ContentKindSlices(text, textLen, contentResult2);

  ContentKindResult result1 = ContentResultToContentKindResult(contentResult1);
  ContentKindResult result2 = ContentResultToContentKindResult(contentResult2);
  if (result1 != result2) {
    int x = contentResult1.asciiCount;
  }
  return result1;
#endif
}

}  // namespace

extern "C" {

enum SearchAlgorithmKind {
  kStrStr = 1,
  kBndm32 = 2,
  kBndm64 =3,
  kBoyerMoore = 4,
  kRegex = 5,
  kRe2 = 6,
};

EXPORT AsciiSearchBase* __stdcall AsciiSearchAlgorithm_Create(
    SearchAlgorithmKind kind,
    const char* pattern,
    int patternLen,
    AsciiSearchBase::SearchOptions options, 
    AsciiSearchBase::SearchCreateResult* searchCreateResult) {
  (*searchCreateResult) = AsciiSearchBase::SearchCreateResult();
  AsciiSearchBase* result = NULL;

  switch(kind) {
    case kBndm32:
      if (options & AsciiSearchBase::kMatchCase)
        result = new Bndm32Search<CaseSensitive>();
      else
        result = new Bndm32Search<CaseInsensitive>();
      break;
    case kBndm64:
      if (options & AsciiSearchBase::kMatchCase)
        result = new Bndm64Search<CaseSensitive>();
      else
        result = new Bndm64Search<CaseInsensitive>();
      break;
    case kBoyerMoore:
      result = new BoyerMooreSearch();
      break;
    case kStrStr:
      result = new StrStrSearch();
      break;
    case kRegex:
      result = new RegexSearch();
      break;
    case kRe2:
      result = new RE2Search();
      break;
  }

  if (!result) {
    searchCreateResult->SetError(E_OUTOFMEMORY, "Out of memory");
    return result;
  }

  result->StartSearch(pattern, patternLen, options, *searchCreateResult);
  if (FAILED(searchCreateResult->HResult)) {
    delete result;
    return NULL;
  }

  return result;
}

EXPORT int __stdcall AsciiSearchAlgorithm_GetSearchBufferSize(AsciiSearchBase* search) {
  return search->GetSearchBufferSize();
}

EXPORT void __stdcall AsciiSearchAlgorithm_Search(
    AsciiSearchBase* search,
    AsciiSearchBase::SearchParams* searchParams) {
  search->FindNext(searchParams);
}

EXPORT void __stdcall AsciiSearchAlgorithm_CancelSearch(
    AsciiSearchBase* search,
    AsciiSearchBase::SearchParams* searchParams) {
  search->CancelSearch(searchParams);
}

EXPORT void __stdcall AsciiSearchAlgorithm_Delete(AsciiSearchBase* search) {
  delete search;
}

enum TextKind {
  TextKind_Ascii,
  TextKind_AsciiWithUtf8Bom,
  TextKind_Utf8,
  TextKind_Utf8WithBom,
  TextKind_ProbablyBinary,
};

EXPORT TextKind __stdcall Text_GetKind(const char* text, int textLen) {
  bool utf8 = Text_HasUtf8Bom(text, textLen);
  if (utf8) {
    ContentKindResult kind = Text_ContentKind(text + 3, textLen -3);
    if (kind == ResultAscii)
      return TextKind_AsciiWithUtf8Bom;
    else if (kind == ResultUtf8)
      return TextKind_Utf8WithBom;
    else 
      return TextKind_ProbablyBinary;
  } else {
    ContentKindResult kind = Text_ContentKind(text, textLen);
    if (kind == ResultAscii)
      return TextKind_Ascii;
    else if (kind == ResultUtf8)
      return TextKind_Utf8;
    else 
      return TextKind_ProbablyBinary;
  }
}

EXPORT bool __stdcall Ascii_Compare(
    const char *text1,
    size_t text1Length,
    const char* text2,
    size_t text2Length) {
  if (text1Length != text2Length)
    return false;

  return memcmp(text1, text2, text1Length) == 0;
}

EXPORT bool __stdcall Ascii_GetLineExtentFromPosition(
    const char* text,
    int textLen,
    int position,
    int maxOffset,
    int* lineStartPosition,
    int* lineLen) {
  return GetLineExtentFromPosition(
      text, textLen, position, maxOffset, lineStartPosition, lineLen);
}

EXPORT const wchar_t* __stdcall Utf16_Search(
    const wchar_t *text,
    size_t textLength,
    const wchar_t* pattern,
    size_t patternLength,
    AsciiSearchBase::SearchOptions options) {
  const wchar_t* textEnd = text + textLength;
  const wchar_t* patternEnd = pattern + patternLength;
  auto result = (options & AsciiSearchBase::kMatchCase)
    ? std::search(text, textEnd, pattern, patternEnd)
    : std::search(text, textEnd, pattern, patternEnd, char_equal_icase);
  if (result == textEnd)
    return nullptr;
  return result;
}

EXPORT bool __stdcall Utf16_GetLineExtentFromPosition(
    const wchar_t* text,
    int textLen,
    int position,
    int maxOffset,
    int* lineStartPosition,
    int* lineLen) {
  return GetLineExtentFromPosition(
      text, textLen, position, maxOffset, lineStartPosition, lineLen);
}

}  // extern "C"
