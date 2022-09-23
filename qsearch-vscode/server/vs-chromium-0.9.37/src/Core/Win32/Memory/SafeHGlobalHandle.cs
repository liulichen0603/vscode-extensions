﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace VsChromium.Core.Win32.Memory {
  public class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid {
    public SafeHGlobalHandle(IntPtr handle)
      : base(true) {
      this.handle = handle;
    }

    public IntPtr Pointer { get { return handle; } }

    protected override bool ReleaseHandle() {
      Marshal.FreeHGlobal(handle);
      return true;
    }
  }
}
