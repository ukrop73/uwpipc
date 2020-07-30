using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeServer
{
    class Program
    {

        private static bool keepRunning = true;
        static void Main(string[] args)
        {
            System.Threading.CancellationTokenSource cancellationTokenSource = new System.Threading.CancellationTokenSource();

            Task t = new Task(() => {
                PipeServer pipeServer = new PipeServer();
                pipeServer.start(cancellationTokenSource.Token);
                pipeServer = null;
            });
            t.Start();

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                Program.keepRunning = false;
            };

            while (Program.keepRunning)
            {
                // Do your work in here, in small chunks.
                // If you literally just want to wait until ctrl-c,
                // not doing anything, see the answer using set-reset events.
            }
        }
    }
}
