﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using VsChromium.Core.Ipc.TypedMessages;

namespace VsChromium.Server.Ipc.TypedEvents {
  /// <summary>
  /// Allows sending events from any thread asynchronously.
  /// </summary>
  public interface ITypedEventSender {
    void SendEventAsync(TypedEvent typedEvent);
  }
}
