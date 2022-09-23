// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Collections.Generic;
using System.Threading;
using VsChromium.Server.FileSystemNames;
using VsChromium.Server.Projects;

namespace VsChromium.Server.FileSystem {
  public interface IFileSystemSnapshotBuilder {
    FileSystemSnapshot Compute(
      IFileSystemNameFactory fileNameFactory, 
      FileSystemSnapshot oldSnapshot,
      FullPathChanges pathChanges /* may be null */,
      IList<IProject> projects, 
      int version,
      CancellationToken cancellationToken);
  }
}