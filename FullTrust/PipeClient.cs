using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;

namespace FullTrust
{
    class PipeClient
    {
        private const string PipeName = "TPCCompanion";
        private const string PhraseHello = "TPCCompanionServiceHello!";
        private const string PhraseBye = "Bye!";
        private const string PhraseError = "Error!";

        public void start(string str)
        {
            Console.WriteLine("TPCCompanionClient:");
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

            Console.WriteLine("Connecting to server...\n");
            pipeClient.Connect();
            Console.WriteLine("Connected...\n");

            StreamString ss = new StreamString(pipeClient);
            // Validate the server's signature string
            Message messageIn = ss.ReadString(PhraseHello);
            Console.WriteLine("messageIn={0}", messageIn);
            if (!messageIn.isError())
            {
                JObject jObject = JObject.FromObject(new
                {
                    command = "info",
                    data = "Clent test v1.0.0"
                });
                string s = JsonConvert.SerializeObject(jObject);
                Console.WriteLine(s);
                ss.WriteString(s);

                messageIn = ss.ReadString();
                Console.WriteLine("messageIn={0}", messageIn);

                jObject = JObject.FromObject(new
                {
                    command = str,
                    type = "get"
                });

                

                    s = JsonConvert.SerializeObject(jObject);
                    Console.WriteLine(">sOut={0}.", s);
                    ss.WriteString(s);
                    Console.Write("Read:{0}", ss.ReadString());
                ss.WriteString(Message.PhraseBye);
            }
            else
            {
                Console.WriteLine("Server could not be verified.");
            }
            //pipeClient.Close();
            //pipeClient = null;
        }
    }
}
