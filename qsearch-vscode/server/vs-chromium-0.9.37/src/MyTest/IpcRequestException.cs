using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsChromium.Core.Ipc;

namespace MyTest
{
    internal class IpcRequestException : Exception {
        private readonly IpcRequest request_;

        public IpcRequestException(IpcRequest request, Exception inner)
          : base(string.Format("Error sending request {0} of type {1} to server", request.RequestId, request.Data.GetType().FullName), inner)
        {
            request_ = request;
        }

        public IpcRequest Request { get { return request_; } }
    }
}
