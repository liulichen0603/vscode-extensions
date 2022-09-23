// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

#include "stdafx.h"

#include <stdint.h>
#include <stdlib.h>
#include <assert.h>

#include "search_base.h"

namespace {

bool IsWordCharacter(char ch) {
  return 
    (ch >= 'a' && ch <= 'z') ||
    (ch >= 'A' && ch <= 'Z') ||
    (ch >= '0' && ch <= '9') ||
    (ch == '_');
}

bool IsWholeWordMatch(AsciiSearchBase::SearchParams* searchParams) {
  const char* start = searchParams->MatchStart - 1;
  char startCh = 0;
  if (start >= searchParams->TextStart) {
    startCh = *(searchParams->MatchStart -1);
  }
  if (IsWordCharacter(startCh))
    return false;

  const char* end = searchParams->MatchStart + searchParams->MatchLength;
  char endCh = 0;
  if (end < searchParams->TextStart + searchParams->TextLength) {
    endCh = *end;
  }
  if (IsWordCharacter(endCh))
    return false;

  return true;
}

}

AsciiSearchBase::AsciiSearchBase() {}
AsciiSearchBase::~AsciiSearchBase() {}

void AsciiSearchBase::StartSearch(
  const char *pattern,
  int patternLen,
  SearchOptions options,
  SearchCreateResult& result) {
  this->StartSearchWorker(pattern, patternLen, options, result);
  if (options & kMatchWholeWord) {
    findNext_ = &AsciiSearchBase::FindNextWholeWord;
  } else {
    findNext_ = &AsciiSearchBase::FindNextWorker;
  }
}

void AsciiSearchBase::FindNext(SearchParams* searchParams) {
  (this->*findNext_)(searchParams);
}

void AsciiSearchBase::FindNextWholeWord(SearchParams* searchParams) {
  while (true) {
    this->FindNextWorker(searchParams);
    if (searchParams->MatchStart == nullptr)
      break;

    if (IsWholeWordMatch(searchParams))
      break;
  }
}

