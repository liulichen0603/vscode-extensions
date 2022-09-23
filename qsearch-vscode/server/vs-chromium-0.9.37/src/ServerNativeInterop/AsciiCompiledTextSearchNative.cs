﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using VsChromium.Core.Ipc;
using VsChromium.Core.Win32.Memory;
using VsChromium.Core.Win32.Strings;

namespace VsChromium.Server.NativeInterop {
  public class AsciiCompiledTextSearchNative : CompiledTextSearchBase {
    private readonly SafeSearchHandle _handle;
    private readonly SafeHGlobalHandle _patternHandle;
    private readonly int _searchBufferSize;

    public AsciiCompiledTextSearchNative(
        NativeMethods.SearchAlgorithmKind kind,
        string pattern,
        NativeMethods.SearchOptions searchOptions) {
      _patternHandle = new SafeHGlobalHandle(Marshal.StringToHGlobalAnsi(pattern));
      var patternLength = pattern.Length;

      _handle = CreateSearchHandle(kind, _patternHandle, patternLength, searchOptions);
      _searchBufferSize = NativeMethods.AsciiSearchAlgorithm_GetSearchBufferSize(_handle);
    }

    private static unsafe SafeSearchHandle CreateSearchHandle(
        NativeMethods.SearchAlgorithmKind kind,
        SafeHGlobalHandle patternHandle,
        int patternLength,
        NativeMethods.SearchOptions searchOptions) {
      NativeMethods.SearchCreateResult createResult;
      var result = NativeMethods.AsciiSearchAlgorithm_Create(
          kind,
          patternHandle.Pointer,
          patternLength,
          searchOptions,
          out createResult);

      if (createResult.HResult < 0) {
        // The error is recoverable, since we are dealing with an invalid pattern
        // or something along the lines.
        var message = Marshal.PtrToStringAnsi(new IntPtr(createResult.ErrorMessage));
        throw new RecoverableErrorException(message);
      }

      return result;
    }

    protected override int SearchBufferSize {
      get { return _searchBufferSize; }
    }

    protected override void Search(ref NativeMethods.SearchParams searchParams) {
      NativeMethods.AsciiSearchAlgorithm_Search(_handle, ref searchParams);
    }

    protected override void CancelSearch(ref NativeMethods.SearchParams searchParams) {
      NativeMethods.AsciiSearchAlgorithm_CancelSearch(_handle, ref searchParams);
    }

    public override void Dispose() {
      _handle.Dispose();
      _patternHandle.Dispose();
    }
  }
}
