﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;

namespace VsChromium.Core.Win32.Files {
  [Flags]
  public enum NativeAccessFlags : uint {
    FILE_LIST_DIRECTORY = 1,
    GenericWrite = 0x40000000,
    GenericRead = 0x80000000
  }
}
