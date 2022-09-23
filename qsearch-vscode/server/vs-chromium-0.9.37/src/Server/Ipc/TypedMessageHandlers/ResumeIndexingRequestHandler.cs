﻿// Copyright 2018 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.ComponentModel.Composition;
using VsChromium.Core.Ipc.TypedMessages;

namespace VsChromium.Server.Ipc.TypedMessageHandlers {
  [Export(typeof(ITypedMessageRequestHandler))]
  public class ResumeIndexingRequestHandler : TypedMessageRequestHandler {
    private readonly IIndexingServer _indexingServer;

    [ImportingConstructor]
    public ResumeIndexingRequestHandler(IIndexingServer indexingServer) {
      _indexingServer = indexingServer;
    }

    public override TypedResponse Process(TypedRequest typedRequest) {
      _indexingServer.Resume();
      return new PauseResumeIndexingResponse();
    }
  }
}