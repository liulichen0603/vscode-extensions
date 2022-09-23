// Copyright 2018 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Collections.Generic;
using ProtoBuf;

namespace VsChromium.Core.Ipc.TypedMessages {
  [ProtoContract]
  public class FileSystemTree {
    public FileSystemTree() {
      // To avoid getting "null" property if empty collection deserialized using protobuf.
      Projects = new List<ProjectEntry>();
    }

    /// <summary>
    /// The version of this snapshot of the tree.
    /// </summary>
    [ProtoMember(1)]
    public int Version { get; set; }

    /// <summary>
    /// The Root entry has a "null" name, and contains one directory entry per
    /// project known to the server. Each project entry contains the hierarchy
    /// of directory and file entries as they appear in the file system under
    /// that project path.
    /// </summary>
    [ProtoMember(2)]
    public List<ProjectEntry> Projects { get; set; }
  }
}