// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using FHL;
using System;
using System.IO;
using VsChromium.Core.Ipc;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Core.Logging;

namespace VsChromium.Threads
{
    public class DispatchThreadServerRequestExecutor
    {
        public readonly TypedRequestProcessProxy _typedRequestProcessProxy = new TypedRequestProcessProxy();
        public readonly DelayedOperationExecutor _delayedOperationExecutor = new DelayedOperationExecutor();
        private readonly SynchronizationContextProvider _synchronizationContextProvider = new SynchronizationContextProvider(new DispatchThread());

        public DispatchThreadServerRequestExecutor()
        {
            _typedRequestProcessProxy.ProcessStarted += TypedRequestProcessProxyOnProcessStarted;
            _typedRequestProcessProxy.ProcessFatalError += TypedRequestProcessProxyOnProcessFatalError;
        }

        public void Post(DispatchThreadServerRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.Id == null)
                throw new ArgumentException(@"Request must have an Id.", "request");
            if (request.Request == null)
                throw new ArgumentException(@"Request must have a typed request.", "request");

            var operation = new DelayedOperation
            {
                Id = request.Id,
                Delay = request.Delay,
                // Action executed on a background thread when delay has expired.
                Action = () => {
                    if (request.OnThreadPoolSend != null)
                        Logger.WrapActionInvocation(request.OnThreadPoolSend);

                    _typedRequestProcessProxy.RunAsync(request.Request,
                      GetRunAsycOptions(request),
                      response => OnRequestSuccess(request, response),
                      errorResponse => OnRequestError(request, errorResponse));
                },
            };

            _delayedOperationExecutor.Post(operation);
        }

        public event EventHandler ProcessStarted;
        public event EventHandler<ErrorEventArgs> ProcessFatalError;

        public bool IsServerRunning => _typedRequestProcessProxy.IsServerRunning;

        private void TypedRequestProcessProxyOnProcessStarted(object sender, EventArgs args)
        {
            OnProcessStarted();
        }

        private void TypedRequestProcessProxyOnProcessFatalError(object sender, ErrorEventArgs args)
        {
            OnProcessFatalError(args);
        }

        private void OnRequestSuccess(DispatchThreadServerRequest request, TypedResponse response)
        {
            if (request.OnThreadPoolReceive != null)
                Logger.WrapActionInvocation(request.OnThreadPoolReceive);

            if (request.OnDispatchThreadSuccess != null)
            {
                request.OnDispatchThreadSuccess(response);
            }
        }

        private void OnRequestError(DispatchThreadServerRequest request, ErrorResponse errorResponse)
        {
            if (request.OnThreadPoolReceive != null)
                Logger.WrapActionInvocation(request.OnThreadPoolReceive);

            if (request.OnDispatchThreadError != null)
            {
                _synchronizationContextProvider.DispatchThreadContext.Post(() => {
                    if (errorResponse.IsOperationCanceled())
                    {
                        // UIRequest are cancelable at any point.
                    }
                    else
                    {
                        request.OnDispatchThreadError(errorResponse);
                    }
                });
            }
        }

        protected virtual void OnProcessStarted()
        {
            ProcessStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnProcessFatalError(ErrorEventArgs e)
        {
            ProcessFatalError?.Invoke(this, e);
        }

        private RunAsyncOptions GetRunAsycOptions(DispatchThreadServerRequest request)
        {
            RunAsyncOptions options = default(RunAsyncOptions);
            if (request.RunOnSequentialQueue)
            {
                options |= RunAsyncOptions.RunOnSequentialQueue;
            }
            return options;
        }
    }
}


