// Copyright 2015 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FHL;
using VsChromium.Core.Collections;
using VsChromium.Core.Files;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Threads;

namespace FHL
{
    public class FileRegistrationRequestService
    {
        private readonly DispatchThreadServerRequestExecutor _dispatchThreadServerRequestExecutor;

        private class TextDocumentEventHandlers
        {
        }

        public FileRegistrationRequestService(
          DispatchThreadServerRequestExecutor dispatchThreadServerRequestExecutor)
        {
            _dispatchThreadServerRequestExecutor = dispatchThreadServerRequestExecutor;
        }

        public void RegisterTextDocument(string filePath)
        {
            SendRegisterFileRequest(filePath);
        }

        public void UnregisterTextDocument(string filePath)
        {
            SendUnregisterFileRequest(filePath);
        }

        public void RegisterFile(string path)
        {
            SendRegisterFileRequest(path);
        }

        public void UnregisterFile(string path)
        {
            SendUnregisterFileRequest(path);
        }

        private void SendRegisterFileRequest(string path)
        {
            var request = new DispatchThreadServerRequest
            {
                Id = "RegisterFileRequest-" + path,
                RunOnSequentialQueue = true,
                Request = new RegisterFileRequest
                {
                    FileName = path
                }
            };

            _dispatchThreadServerRequestExecutor.Post(request);
        }

        private void SendUnregisterFileRequest(string path)
        {
            if (!IsValidPath(path))
                return;

            var request = new DispatchThreadServerRequest
            {
                Id = "UnregisterFileRequest-" + path,
                RunOnSequentialQueue = true,
                Request = new UnregisterFileRequest
                {
                    FileName = path
                }
            };

            _dispatchThreadServerRequestExecutor.Post(request);
        }

        private bool IsValidPath(string path)
        {
            // This can happen with "Find in files" for example, as it uses a fake filename.
            if (!PathHelpers.IsAbsolutePath(path))
                return false;

            if (!PathHelpers.IsValidBclPath(path))
                return false;

            return true;
        }

    }
}