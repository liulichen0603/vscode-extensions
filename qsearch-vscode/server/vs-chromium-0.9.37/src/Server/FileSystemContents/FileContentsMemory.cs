﻿// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.IO;
using System.Runtime.InteropServices;
using VsChromium.Core.Win32;
using VsChromium.Core.Win32.Memory;

namespace VsChromium.Server.FileSystemContents {
  /// <summary>
  /// Encapsulation over file contents stored in memory, with support for
  /// skipping a fixed numbers of bytes in prefix and suffix.
  /// </summary>
  public struct FileContentsMemory {
    private readonly SafeHandle _block;
    private readonly int _contentsOffset;
    private readonly int _contentsLength;

    public FileContentsMemory(SafeHeapBlockHandle block, int contentsOffset, int contentsLength) :
      this(block, block.ByteLength, contentsOffset, contentsLength) {
    }

    public FileContentsMemory(SafeHandle block, int size, int contentsOffset, int contentsLength) {
      if (contentsOffset < 0)
        throw new ArgumentException("Contents offset must be positive", "contentsOffset");
      if (contentsOffset < 0)
        throw new ArgumentException("Contents length must be positive.", "contentsLength");
      if (checked(contentsOffset + contentsLength) > size)
        throw new ArgumentException("Contents range must be within the bounds of the memory block.", "contentsOffset");
      _block = block;
      _contentsOffset = contentsOffset;
      _contentsLength = contentsLength;
    }

    /// <summary>
    /// Returns the pointer to the usable memory of this block, i.e. the block
    /// offset added to the start pointer.
    /// </summary>
    public IntPtr Pointer { get { return Pointers.AddPtr(_block.DangerousGetHandle(), _contentsOffset); } }
    /// <summary>
    /// Return the number of bytes of the usable memory of this block.
    /// </summary>
    public int ByteLength { get { return _contentsLength; } }

    /// <summary>
    /// Create a stream over the underlying memory content.
    /// </summary>
    public unsafe Stream CreateSteam() {
      return new UnmanagedMemoryStream(
        Pointers.Add(_block.DangerousGetHandle(), _contentsOffset),
        _contentsLength);
    }
  }
}