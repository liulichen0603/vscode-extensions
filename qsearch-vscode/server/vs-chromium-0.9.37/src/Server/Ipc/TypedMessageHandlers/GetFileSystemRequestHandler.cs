﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.ComponentModel.Composition;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Server.FileSystem;

namespace VsChromium.Server.Ipc.TypedMessageHandlers {
  [Export(typeof(ITypedMessageRequestHandler))]
  public class GetFileSystemRequestHandler : TypedMessageRequestHandler {
    private readonly IFileSystemSnapshotManager _snapshotManager;

    [ImportingConstructor]
    public GetFileSystemRequestHandler(IFileSystemSnapshotManager snapshotManager) {
      _snapshotManager = snapshotManager;
    }

    public override TypedResponse Process(TypedRequest typedRequest) {
      return new GetFileSystemResponse {
        Tree = _snapshotManager.CurrentSnapshot.ToIpcFileSystemTree()
      };
    }
  }
}
