﻿// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.Breakpoints;

namespace VsChromium.DkmIntegration {
  interface IFunctionTracer {
    void OnEntryBreakpointHit(DkmRuntimeBreakpoint bp, DkmThread thread, bool hasException);
    void OnExitBreakpointHit(DkmRuntimeBreakpoint bp, DkmThread thread, bool hasException);
  }
}
