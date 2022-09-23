// Copyright 2020 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

namespace VsChromium.Server.ProgressTracking {
  public class EnvironmentTickCountProvider : ITickCountProvider {
    public long TickCount {
      get {
        // TODO(rpaquay): Implement some logic to increment an internal counter
        // if the value rounds up.
        return System.Environment.TickCount;
      }
    }
  }
}