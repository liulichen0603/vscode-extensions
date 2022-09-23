﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Collections.Generic;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Server.FileSystemNames;

namespace VsChromium.Server.Search {
  public class FileSearchResult {
    public FileSearchResult() {
      Spans = new List<FilePositionSpan>();
    }

    public FileName FileName { get; set; }
    public List<FilePositionSpan> Spans { get; set; }
  }
}
