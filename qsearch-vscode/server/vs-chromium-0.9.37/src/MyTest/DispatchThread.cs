// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.


namespace FHL
{
    public class DispatchThread
    {
        public DispatchThread()
        {
            ManagedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public int ManagedThreadId { get; private set; }
    }
}

