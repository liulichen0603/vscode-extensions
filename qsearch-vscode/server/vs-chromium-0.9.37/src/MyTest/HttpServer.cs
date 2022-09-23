using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VsChromium.Core.Ipc.TypedMessages;

namespace FHL
{
    internal class HttpServer
    {
        private HttpListener _listener;
        private string _url = "http://127.0.0.1:";
        private int _port = 63303;
        private int _requestCount = 0;
        private CodeSearchController _codeSearcherController;

        public HttpServer(CodeSearchController codeSearcherController, int port) { _codeSearcherController = codeSearcherController; _port = port; }

        private string GetRequestData(HttpListenerRequest request) {
            if (!request.HasEntityBody) {
                return "";
            }
            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body);
            string result = reader.ReadToEnd();
            body.Close();
            reader.Close();
            return result;
        }

        
        private FHLSearchRequest GetFHLSearchRequestFromString(string request_str) {
            FHLSearchRequest result = null;
            try
            {
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<FHLSearchRequest>(request_str);
            }
            catch { 
                
            }
            return result;
        }

        private string GenResultJson(DirectoryEntry entry) {
            if (entry == null) {
                return "";
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(entry);
        }

        public void Start() { 
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url + _port + "/");
            _listener.Start();

            Task listen_task = HandleIncomingConnections();
            listen_task.GetAwaiter().GetResult();

            _listener.Close();
        }


        public async Task HandleIncomingConnections() {
            bool run_server = true;

            while (run_server) { 
                HttpListenerContext ctx = await _listener.GetContextAsync();
            
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                Console.WriteLine("Request #: {0}", ++_requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                if (req.HttpMethod == "GET") { 
                    byte[] data = Encoding.UTF8.GetBytes(req.Url.AbsoluteUri);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.Length;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown")) {
                    run_server = false;
                    break;
                }

                FHLSearchRequest fhl_request = null;
                if (req.HttpMethod == "POST") {
                    string request_str = GetRequestData(req);
                    fhl_request = GetFHLSearchRequestFromString(request_str);
                }

                if (fhl_request != null) {
                    _codeSearcherController.PerformSearch(fhl_request.SearchString, fhl_request.SearchPath);
                    await GlobalClass.WaitStart();
                    Console.WriteLine("Should wait end");
                    string result = GenResultJson(GlobalClass.OneResponse);


                    Console.WriteLine(result);
                    byte[] data = Encoding.UTF8.GetBytes(result);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.Length;

                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }
                GlobalClass.OneResponse = new DirectoryEntry();
                resp.Close();
            }
        }
    }
}
