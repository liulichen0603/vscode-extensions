using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsChromium.Core.Ipc;
using VsChromium.Core.Ipc.TypedMessages;

namespace FHL
{
    internal class GlobalClass
    {
        static private readonly ManualResetEvent _mre = new ManualResetEvent(false);


        static private TaskCompletionSource<int> task = task = new TaskCompletionSource<int>();
        public static Task WaitStart() {
            task = new TaskCompletionSource<int>();
            Console.WriteLine("WaitStart");
            return task.Task; 
        }
        public static void WaitEnd() { Console.WriteLine("WaitEnd"); task.SetResult(0); }

        public static DirectoryEntry OneResponse = new DirectoryEntry();


    }
}
