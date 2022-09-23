﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using VsChromium.Server.Ipc.TypedEvents;

namespace VsChromium.Server.ProgressTracking {
  public class ProgressTracker : ProgressTrackerBase {
    private readonly int _totalStepCount;

    public ProgressTracker(ITypedEventSender typedEventSender, int totalStepCount)
      : base(typedEventSender) {
      _totalStepCount = totalStepCount;
    }

    public override int TotalStepCount { get { return _totalStepCount; } }
  }
}
