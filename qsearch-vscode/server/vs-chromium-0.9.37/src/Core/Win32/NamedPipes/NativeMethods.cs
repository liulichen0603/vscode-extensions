﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;
using VsChromium.Core.Win32.Interop;

namespace VsChromium.Core.Win32.NamedPipes {
  static class NativeMethods {
    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool CreatePipe(
      out SafeFileHandle hReadPipe,
      out SafeFileHandle hWritePipe,
      SecurityAttributes lpPipeAttributes,
      int nSize);
  }
}
