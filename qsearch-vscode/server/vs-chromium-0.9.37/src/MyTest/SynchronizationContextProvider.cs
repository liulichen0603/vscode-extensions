// Copyright 2014 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Threading;

namespace FHL
{
    public class SynchronizationContextProvider
    {
        private readonly ISynchronizationContext _context;
        private readonly int _threadId;

        public SynchronizationContextProvider(DispatchThread dispatchThread)
        {
            _context = new SynchronizationContextDelegate(SynchronizationContext.Current);
            _threadId = dispatchThread.ManagedThreadId;
        }

        public ISynchronizationContext DispatchThreadContext => _context;
    }
}

