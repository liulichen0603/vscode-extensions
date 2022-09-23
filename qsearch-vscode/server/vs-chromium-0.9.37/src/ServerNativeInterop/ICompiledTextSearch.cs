﻿// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Collections.Generic;
using VsChromium.Core.Utility;

namespace VsChromium.Server.NativeInterop {
  /// <summary>
  /// Abstraction over text search algorithm pre-compiled with the search pattern.
  /// </summary>
  public interface ICompiledTextSearch : IDisposable {
    /// <summary>
    /// Find all occurrences of the stored prepared search pattern in the given
    /// text fragment.
    /// </summary>
    IList<TextRange> FindAll(
      TextFragment textFragment,
      Func<TextRange, TextRange?> postProcess,
      IOperationProgressTracker progressTracker);

    /// <summary>
    /// Find the first occurrence of the stored search pattern in the given text
    /// fragment.
    /// </summary>
    TextRange? FindFirst(
      TextFragment textFragment,
      IOperationProgressTracker progressTracker);
  }
}