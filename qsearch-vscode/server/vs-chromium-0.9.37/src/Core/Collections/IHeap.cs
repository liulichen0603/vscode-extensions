// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

namespace VsChromium.Core.Collections {
  /// <summary>
  /// Common interface for a Max or Min heap implementation.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IHeap<T> {
    int Count { get; }
    T Root { get; }

    void Clear();

    void Add(T item);
    T Remove();
  }
}
