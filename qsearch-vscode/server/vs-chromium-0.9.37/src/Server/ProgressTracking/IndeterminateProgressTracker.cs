// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using VsChromium.Server.Ipc.TypedEvents;

namespace VsChromium.Server.ProgressTracking {
  public class IndeterminateProgressTracker : ProgressTrackerBase {
    public IndeterminateProgressTracker(ITypedEventSender typedEventSender)
      : base(typedEventSender) {
    }

    public override int TotalStepCount { get { return int.MaxValue; } }
  }
}
