﻿// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

namespace VsChromium.Core.Win32.Files {
  /// <summary>
  /// File or directory entry information returned by <see
  /// cref="NativeFile.GetDirectoryEntries"/>, including the short <see
  /// cref="Name"/> and file attributes.
  /// </summary>
  public struct DirectoryEntry {
    private readonly string _name;
    private readonly FILE_ATTRIBUTE _attributes;

    public DirectoryEntry(string name, FILE_ATTRIBUTE attributes) {
      _name = name;
      _attributes = attributes;
    }

    public string Name {
      get { return _name; }
    }

    public FILE_ATTRIBUTE Attributes {
      get { return _attributes; }
    }

    public bool IsFile {
      get {
        return (_attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY) == 0;
      }
    }

    public bool IsDirectory {
      get {
        return (_attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY) != 0;
      }
    }

    public bool IsSymLink {
      get {
        return (_attributes & FILE_ATTRIBUTE.FILE_ATTRIBUTE_REPARSE_POINT) != 0;
      }
    }
  }
}