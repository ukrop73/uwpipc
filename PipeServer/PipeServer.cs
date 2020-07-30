using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;

namespace PipeServer
{
    //
    // s-c: PhraseHello
    // c-s: Command Info with Data = AppName
    // s-c: Command Info with Data = "TPCCompanion v1.0.0"
    // c-s: Command "some command" with task
    // s-c: Command "some command" with result
    // c-s: PhraseBye
    class PipeServer
    {
        public static int MaxPipes = 4;
        private const string PipeName = "TPCCompanion";

        // Declare the delegate (if using non-generic pattern).
        public delegate void StateEventHandler(object sender);
        // Declare the event.
        public static event StateEventHandler stateEvent;

        private string client = null;

        public static string objectToJson(string commandName, object o)
        {
            return JsonConvert.SerializeObject(new { command = commandName, result = o });
        }

        public void start(CancellationToken cancellationToken)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(String.Format("PipeServer starting[{0}]...", threadId));

            start:

            NamedPipeServerStream pipeServer = null;
            try
            {
                pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, MaxPipes, PipeTransmissionMode.Byte, PipeOptions.None);
                /*PipeSecurity ps = pipeServer.GetAccessControl();
                Log.Information("ps={0}", ps.AccessRuleType.FullName);
                //ps.SetAccessRuleProtection(false, false);
                foreach (PipeAccessRule r in ps.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    Log.Information("PipeAccessRule={0}/{1}/{2}", r.AccessControlType, r.PipeAccessRights, r.IdentityReference);
                }
                //pipeServer.SetAccessControl(ps);
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                Log.Information("identity/principal={0}/{1}", identity.User, principal);
                /*if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    // Allow the Administrators group full access to the pipe.
                    ps.AddAccessRule(new PipeAccessRule("LAPTOP-4GA1U2Q9\\oleks",
                        PipeAccessRights.FullControl, AccessControlType.Allow));
                    //new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount)),
                    //    PipeAccessRights.FullControl, AccessControlType.Allow));
                }
                else
                {
                    // Allow current user read and write access to the pipe.
                    ps.AddAccessRule(new PipeAccessRule(
                        WindowsIdentity.GetCurrent().User,
                        PipeAccessRights.ReadWrite, AccessControlType.Allow));
                }

                pipeServer.SetAccessControl(ps);
                //PipesAclExtensions.SetAccessControl(pipeServer, ps);*/
            }
            catch (Exception exception)
            {
                Console.WriteLine(String.Format("PipeServer[{0}] Error: {1}", threadId, exception.Message));
                return;
            }

            cancellationToken.Register(() => pipeServer.Close());

            // Wait for a client to connect
            try
            {
                pipeServer.WaitForConnection();
            }
            catch (Exception exception)
            {
                Console.WriteLine(String.Format("PipeServer[{0}] Error: {1}", threadId, exception.Message));
                return;
            }

            Console.WriteLine(String.Format("PipeServer[{0}] Client connected.", threadId));
            stateEvent?.Invoke(this);

            try
            {
                StreamString ss = new StreamString(pipeServer);
                ss.WriteString(Message.PhraseHello, false);
                Console.WriteLine(String.Format("PipeServer[{0}] Hello Phrase sent.", threadId));

                Message messageIn;
                string result;

                while (!cancellationToken.IsCancellationRequested)
                {
                    messageIn = ss.ReadString();
                    Console.WriteLine(String.Format("PipeServer[{0}] messageIn={1}", threadId, messageIn));

                    if (messageIn.isBye() || messageIn.isEnd()) break;

                    if (messageIn.isError())
                    {
                        ss.WriteString(messageIn.getError());
                        break;
                    }

                    result = objectToJson("testcommand", "Ok.");
                    if (ss.WriteString(result) == 0)
                    {
                        result = objectToJson("testcommand", "Too long result. Please try to use filter.");
                        ss.WriteString(result);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(String.Format("PipeServer[{0}] Error: {1}", threadId, exception.Message));
            }

            try
            {
                pipeServer.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(String.Format("PipeServer[{0}] Error: {1}", threadId, exception.Message));
            }

            Console.WriteLine(String.Format("PipeServer[{0}] done.", threadId));
            goto start;
        }
    }
}
