using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHL
{
    class Program
    {
        static void Main(string[] args) {
            string workspace = "";
            int port = 0;
            if (args.Length == 2)
            {
                workspace = args[0];
                port = Int32.Parse(args[1]);
            }
            else {
                Console.WriteLine("Args error");
                return;
            }
            CodeSearchController searcher = new CodeSearchController(workspace);
            HttpServer server = new HttpServer(searcher, port);
            server.Start();
            Thread.Sleep(-1);
        }
    }
}

