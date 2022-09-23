﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using VsChromium.Server.NativeInterop;
using VsChromium.Server.Search;

namespace VsChromium.Server.FileSystemContents {
  public class Utf16FileContents : FileContents {

    public Utf16FileContents(FileContentsMemory contents, DateTime utcLastModified)
      : base(contents, utcLastModified) {
    }

    protected override ITextLineOffsets GetFileOffsets() {
      return new Utf16TextLineOffsets(Contents);
    }

    public override byte CharacterSize {
      get { return sizeof(char); }
    }

    protected override ICompiledTextSearch GetCompiledTextSearch(ICompiledTextSearchContainer container) {
      return container.GetUtf16Search();
    }

    protected override TextRange GetLineTextRangeFromPosition(int position, int maxRangeLength) {
      int lineStart;
      int lineLength;
      NativeMethods.Utf16_GetLineExtentFromPosition(
          this.Contents.Pointer,
          this.CharacterCount,
          position,
          MaxLineExtentOffset,
          out lineStart,
          out lineLength);
      return new TextRange(lineStart, lineLength);
    }

    public static CompiledTextSearchBase CreateSearchAlgo(
        string pattern,
        SearchProviderOptions searchOptions) {
      var options = NativeMethods.SearchOptions.kNone;
      if (searchOptions.MatchCase) {
        options |= NativeMethods.SearchOptions.kMatchCase;
      }
      if (searchOptions.MatchWholeWord) {
        options |= NativeMethods.SearchOptions.kMatchWholeWord;
      }
      return new Utf16CompiledTextSearchStdSearch(pattern, options);
    }
  }
}
