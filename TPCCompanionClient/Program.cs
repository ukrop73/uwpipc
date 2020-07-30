using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;

namespace TPCCompanionClient
{
    class Program
    {
        private const string PipeName = "TPCCompanion";
        private const string PhraseHello = "TPCCompanionServiceHello!";
        private const string PhraseBye = "Bye!";
        private const string PhraseError = "Error!";

        static void Main(string[] args)
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

                string sCommand;
                string sData = null;

                while (true)
                {
                    sCommand = Console.ReadLine();
                    Console.WriteLine(">command={0}.", sCommand);
                    if (sCommand == "bye")
                    {
                        ss.WriteString(Message.PhraseBye);
                        break;
                    }

                    if (sCommand == "log")
                    {
                        sData = Console.ReadLine();
                        if (sData.Length > 0)
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                filter = "appear>'2019-10-27 15:00' and appear<'2019-10-27 16:00'"
                            });
                        }
                        else
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand
                            });
                        }
                    }
                    else if (sCommand == "device")
                    {
                        Console.Write(", get/set/del>");
                        sData = Console.ReadLine();

                        if (sData == "set")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { name = "d1", model = "m1", description = "ds1", protocol = "tcp", address = "192.168.1.5", port = 52408, keepconnection = 0 },
                                new { name = "d2", model = "m2", description = "ds2", protocol = "udp", address = "192.168.1.5", port = 52409, keepconnection = 1 }
                            }
                            });
                        }
                        else if (sData == "del")
                        {
                            Console.Write("Input device id>");
                            string id = Console.ReadLine();
                            int intId = 0;
                            try
                            {
                                intId = Int32.Parse(id);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Unable to parse '{id}'");
                            }
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { id = intId }
                            }
                            });
                        }
                        else if (sData == "id")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "set",
                                data = new List<object>() {
                                //new { id=3, name = "d3", model = "m31", description = "ds31", protocol = "tcp", address = "   ", port = 52408, keepconnection = 0 },
                                new { id=1, name = "d4", model = "m42", description = "ds42", protocol = "udp", address = "", port = 52409, keepconnection = 1 }
                            }
                            });
                        }
                        else
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "get"
                            });
                            
                        }
                    }
                    else if (sCommand == "command")
                    {
                        Console.Write(", get/set/set1/del>");
                        sData = Console.ReadLine();
                        if (sData == "set")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { deviceid = 1, name = "c11", content = "Test \\xAF \\x8B \\xCC\\xeD 0", description = "description11" },
                                new { deviceid = 1, name = "c21", content = "content21", description = "description21", disabled=1 }
                            }
                            });
                        }
                        else if (sData == "set1")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "set",
                                data = new List<object>() {
                                new { deviceid = 7, name = "c71", content = "con71" } 
                            }
                            });
                        }
                        else if (sData == "set2")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "set",
                                data = new List<object>() {
                                new { deviceid = 7, name = "c71", content = "con712" }
                            }
                            });
                        }
                        else if (sData == "set3")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "set",
                                data = new List<object>() {
                                new { id = 2, deviceid=7, name = "c71", content = "con713" }
                            }
                            });
                        }
                        else if (sData == "del")
                        {
                            Console.Write("Input command id>");
                            string id = Console.ReadLine();
                            int intId = 0;
                            try
                            {
                                intId = Int32.Parse(id);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Unable to parse '{id}'");
                            }
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { id = intId }
                            }
                            });
                        }
                        else
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "get"
                            });
                        }
                    }
                    else if (sCommand == "app")
                    {
                        Console.Write(", get/set/del>");
                        sData = Console.ReadLine();

                        if (sData == "set")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { name = "app1", content = "m1", description = "ds1", arguments = "tcp" }
                            }
                            });
                        }
                        else if (sData == "del")
                        {
                            Console.Write("Input app id>");
                            string id = Console.ReadLine();
                            int intId = 0;
                            try
                            {
                                intId = Int32.Parse(id);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Unable to parse '{id}'");
                            }
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { id = intId }
                            }
                            });
                        }
                        else
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "get"
                            });

                        }
                    }
                    else if (sCommand == "hotkey")
                    {
                        Console.Write(", get/set/del>");
                        sData = Console.ReadLine();

                        if (sData == "set")
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { name = "screenshot", content = "SHIFT-LWIN+VK_S", description = "ds1", disabled = 0 }
                            }
                            });
                        }
                        else if (sData == "del")
                        {
                            Console.Write("Input hotkey id>");
                            string id = Console.ReadLine();
                            int intId = 0;
                            try
                            {
                                intId = Int32.Parse(id);
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Unable to parse '{id}'");
                            }
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = sData,
                                data = new List<object>() {
                                new { id = intId }
                            }
                            });
                        }
                        else
                        {
                            jObject = JObject.FromObject(new
                            {
                                command = sCommand,
                                type = "get"
                            });

                        }
                    }
                    else if (sCommand == "exe")
                    {
                        Console.Write(", c/a/h/h2/c2>");
                        sData = Console.ReadLine();
                        switch (sData)
                        {
                            case "a":
                                jObject = JObject.FromObject(new
                                {
                                    command = sCommand,
                                    data = "app:notepad"
                                });
                                break;
                            case "h":
                                jObject = JObject.FromObject(new
                                {
                                    command = sCommand,
                                    data = "hotkey:Run"
                                });
                                break;
                            case "h2":
                                jObject = JObject.FromObject(new
                                {
                                    command = sCommand,
                                    data = "hotkey:screenshot"
                                });
                                break;
                            case "c2":
                                jObject = JObject.FromObject(new
                                {
                                    command = sCommand,
                                    data = "command:d2:c21"
                                });
                                break;
                            default:
                                jObject = JObject.FromObject(new
                                {
                                    command = sCommand,
                                    data = "command:d1:c11"
                                });
                                break;
                        }
                    }
                    else
                    {
                        jObject = JObject.FromObject(new
                        {
                            command = sCommand,
                            type = "get"
                        });
                    }

                    s = JsonConvert.SerializeObject(jObject);
                    Console.WriteLine(">sOut={0}.", s);
                    ss.WriteString(s);
                    Console.Write("Read:{0}", ss.ReadString());
                }  
            }
            else
            {
                Console.WriteLine("Server could not be verified.");
            }
            pipeClient.Close();
        }
    }
}
