// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;

namespace VsChromium.Features.SourceExplorerHierarchy {
  public interface IIncrementalHierarchyBuilder {
    Func<int, ApplyChangesResult> ComputeChangeApplier();
  }

  public enum ApplyChangesResult {
    Bail,
    Retry,
    Done
  };
}