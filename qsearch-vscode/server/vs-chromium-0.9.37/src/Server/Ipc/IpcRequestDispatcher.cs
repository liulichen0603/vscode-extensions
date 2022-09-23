﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using VsChromium.Core.Ipc;
using VsChromium.Core.Logging;
using VsChromium.Server.Ipc.ProtocolHandlers;
using VsChromium.Server.Threads;

namespace VsChromium.Server.Ipc {
  [Export(typeof(IIpcRequestDispatcher))]
  public class IpcRequestDispatcher : IIpcRequestDispatcher {
    private readonly ICustomThreadPool _customThreadPool;
    private readonly IIpcResponseQueue _ipcResponseQueue;
    private readonly IEnumerable<IProtocolHandler> _protocolHandlers;
    private readonly ITaskQueue _sequentialTaskQueue;

    [ImportingConstructor]
    public IpcRequestDispatcher(
      ICustomThreadPool customThreadPool,
      ITaskQueueFactory taskQueueFactory,
      IIpcResponseQueue ipcResponseQueue,
      [ImportMany] IEnumerable<IProtocolHandler> protocolHandlers) {
      _customThreadPool = customThreadPool;
      _ipcResponseQueue = ipcResponseQueue;
      _protocolHandlers = protocolHandlers;
      _sequentialTaskQueue = taskQueueFactory.CreateQueue("IpcRequestDispatcher sequential requests queue");
    }

    public void ProcessRequestAsync(IpcRequest request) {
      if (request.RunOnSequentialQueue) {
        // Run on queue, with a unique ID since each request is unique
        _sequentialTaskQueue.Enqueue(new TaskId(String.Format("RequestId={0}", request.RequestId)), t => ProcessRequestWorker(request));
      } else {
        _customThreadPool.RunAsync(() => ProcessRequestWorker(request));
      }
    }

    /// <summary>
    /// Process one request synchronously (on a background thread) and sends the response back
    /// to the response queue (i.e. communication pipe)
    /// </summary>
    private void ProcessRequestWorker(IpcRequest request) {
      var sw = Stopwatch.StartNew();
      var response = ProcessOneRequest(request);
      sw.Stop();
      _ipcResponseQueue.Enqueue(response);

      Logger.LogInfo("Request {0} of type \"{1}\" took {2:n0} msec to execute.",
                 request.RequestId, request.Data.GetType().Name, sw.ElapsedMilliseconds);
    }

    private IpcResponse ProcessOneRequest(IpcRequest request) {
      try {
        var processor = _protocolHandlers.FirstOrDefault(x => x.CanProcess(request));
        if (processor == null) {
          throw new Exception(string.Format("Request protocol {0} is not recognized by any request processor!",
                                            request.Protocol));
        }

        return processor.Process(request);
      }
      catch (OperationCanceledException e) {
        Logger.LogInfo("Request {0} of type \"{1}\" has been canceled.",
                   request.RequestId, request.Data.GetType().Name);
        return ErrorResponseHelper.CreateIpcErrorResponse(request.RequestId, e);
      }
      catch (RecoverableErrorException e) {
        Logger.LogInfo("Request {0} of type \"{1}\" generated a recoverable error: {2}.",
                   request.RequestId, request.Data.GetType().Name, e.Message);
        return ErrorResponseHelper.CreateIpcErrorResponse(request.RequestId, e);
      }
      catch (Exception e) {
        var message = string.Format("Error executing request {0} of type \"{1}\".",
                            request.RequestId, request.Data.GetType().Name);
        Logger.LogError(e, "{0}", message);
        var outer = new Exception(message, e);
        return ErrorResponseHelper.CreateIpcErrorResponse(request.RequestId, outer);
      }
    }
  }
}
