﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;

namespace VsChromium.Core.Files {
  /// <summary>
  /// The comparer instance to use for file s system paths comparisons.
  /// </summary>
  public static class SystemPathComparer {
    public static IPathComparer Instance => PathComparerRegistry.Default;

    public static int GetHashCode(string name) {
      return Instance.StringComparer.GetHashCode(name);
    }

    public static bool Equals(string name1, string name2) {
      return Instance.StringComparer.Equals(name1, name2);
    }

    public static int Compare(string name1, string name2) {
      return Instance.StringComparer.Compare(name1, name2);
    }

    public static bool EqualsNames(string name1, string name2) {
      return StringComparer.OrdinalIgnoreCase.Equals(name1, name2);
    }

    public static int CompareNames(string name1, string name2) {
      return StringComparer.OrdinalIgnoreCase.Compare(name1, name2);
    }
  }
}
