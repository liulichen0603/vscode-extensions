﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using VsChromium.Core.Configuration;
using VsChromium.Core.Files;

namespace VsChromium.Server.Projects {
  public class FileFilter : IFileFilter {
    private readonly FilePatternsPathMatcherProvider _ignoreMatcherProvider;

    public FileFilter(IConfigurationSectionContents contents) {
      _ignoreMatcherProvider = new FilePatternsPathMatcherProvider(contents);
    }

    public bool Include(RelativePath relativePath) {
      var ignore = _ignoreMatcherProvider.AnyPathMatcher.MatchFileName(relativePath, SystemPathComparer.Instance);
      return !ignore;
    }
  }
}
