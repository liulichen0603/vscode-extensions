﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using VsChromium.Core.Win32.Processes;

namespace VsChromium.Core.Utility {
  public static class ChromeUtility {
    public static bool IsChromeProcess(string imagePath) {
      string file = Path.GetFileName(imagePath);
      return file.Equals("chrome.exe", StringComparison.CurrentCultureIgnoreCase);
    }

    public static string[] SplitArgs(string unsplitArgumentLine) {
      if (unsplitArgumentLine == null)
        return new string[0];

      int numberOfArgs;
      IntPtr ptrToSplitArgs;
      string[] splitArgs;

      ptrToSplitArgs = NativeMethods.CommandLineToArgvW(unsplitArgumentLine, out numberOfArgs);

      // CommandLineToArgvW returns NULL upon failure.
      if (ptrToSplitArgs == IntPtr.Zero)
        throw new ArgumentException("Unable to split argument.", new Win32Exception());

      // Make sure the memory ptrToSplitArgs to is freed, even upon failure.
      try {
        splitArgs = new string[numberOfArgs];

        // ptrToSplitArgs is an array of pointers to null terminated Unicode strings.
        // Copy each of these strings into our split argument array.
        for (int i = 0; i < numberOfArgs; i++)
          splitArgs[i] = Marshal.PtrToStringUni(
            Marshal.ReadIntPtr(ptrToSplitArgs, i * IntPtr.Size));

        return splitArgs;
      } finally {
        // Free memory obtained by CommandLineToArgW.
        Win32.Memory.NativeMethods.LocalFree(ptrToSplitArgs);
      }
    }
  }
}
