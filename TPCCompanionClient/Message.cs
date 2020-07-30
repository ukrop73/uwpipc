using System;
using System.Collections.Generic;
using System.Text;

namespace TPCCompanionClient
{
    class Message
    {
        private string message;
        private int resultCode;

        public const string PhraseHello = "TPCCompanionServiceHello!";
        public const string PhraseBye = "Bye!";
        public const string PhraseError = "Error!";

        public Message(string s = null, int code = 0)
        {
            this.message = s;
            this.resultCode = code;
        }

        public bool isEnd()
        {
            return (resultCode == -1);
        }

        public bool isBye()
        {
            return (message == PhraseBye);
        }

        public bool isError()
        {
            return (resultCode > 0);
        }

        public string getError()
        {
            return (resultCode > 0 ? message : null);
        }

        public string getMessage()
        {
            return (resultCode == 0 ? message : null);
        }

        public override string ToString()
        {
            return "Message([" + resultCode + "]:" + message + ")";
        }
    }
}
