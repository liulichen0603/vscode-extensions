// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using VsChromium.Server.NativeInterop;

namespace VsChromium.Server.Search {
  public class GetLineExtentCache {
    private readonly GetLineRangeFunction _getLineRange;
    private TextRange? _previousSpan;

    public GetLineExtentCache(GetLineRangeFunction getLineRange) {
      _getLineRange = getLineRange;
    }

    public TextRange GetLineExtent(int position) {
      if (_previousSpan.HasValue) {
        if (position >= _previousSpan.Value.Position &&
            position < _previousSpan.Value.Position + _previousSpan.Value.Length) {
          return _previousSpan.Value;
        }
      }

      _previousSpan = _getLineRange(position);
      return _previousSpan.Value;
    }
  }
}