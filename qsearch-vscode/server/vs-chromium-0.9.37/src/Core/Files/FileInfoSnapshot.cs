// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using VsChromium.Core.Win32.Files;

namespace VsChromium.Core.Files {
  public class FileInfoSnapshot : IFileInfoSnapshot {
    private readonly SlimFileInfo _fileInfo;

    public FileInfoSnapshot(SlimFileInfo fileInfo) {
      _fileInfo = fileInfo;
    }

    public SlimFileInfo SlimFileInfo { get { return _fileInfo; } }

    public bool IsFile { get { return _fileInfo.IsFile; } }
    public bool IsDirectory { get { return _fileInfo.IsDirectory; } }
    public bool IsSymLink { get { return _fileInfo.IsSymLink; } }
    public bool Exists { get { return _fileInfo.Exists; } }
    public FullPath Path { get { return _fileInfo.FullPath; } }
    public DateTime LastWriteTimeUtc { get { return _fileInfo.LastWriteTimeUtc; } }
    public long Length { get { return _fileInfo.Length; } }
  }
}