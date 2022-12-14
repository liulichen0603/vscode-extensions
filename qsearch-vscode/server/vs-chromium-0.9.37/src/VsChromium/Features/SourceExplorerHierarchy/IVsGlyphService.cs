// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using Microsoft.VisualStudio.Language.Intellisense;

namespace VsChromium.Features.SourceExplorerHierarchy {
  public interface IVsGlyphService {
    IntPtr ImageListPtr { get; }
    int GetImageIndex(StandardGlyphGroup standardGlyphGroup, StandardGlyphItem standardGlyphItem);
  }
}