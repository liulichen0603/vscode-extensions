// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using VsChromium.Core.Ipc;

namespace VsChromium.Server.Ipc {
  public interface IIpcResponseQueue {
    void Enqueue(IpcResponse response);
    IpcResponse Dequeue();
  }
}
