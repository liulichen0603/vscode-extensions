// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

namespace VsChromium.Core.Win32.Memory {
  public interface IHeapAlloc {
    SafeHeapBlockHandle Alloc(int size);
  }
}
