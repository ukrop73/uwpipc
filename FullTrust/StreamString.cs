using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FullTrust
{
    class StreamString
    {
        private const string BaseString = "K_A!a3q$VH49rGdm";
        private const int MinLength = 64;

        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public Message ReadString(string phrase = null)
        {
            Message message;

            int byte1 = ioStream.ReadByte();
            Console.WriteLine("ReadString: b1={0}<", byte1);
            if (byte1 < 0)
            {
                message = new Message("End of stream.", -1);
                return message;
            }

            int len = byte1 * 256;
            byte1 = ioStream.ReadByte();
            Console.WriteLine("ReadString: b2={0}<", byte1);
            if (byte1 < 0)
            {
                message = new Message("End of stream, but error format.", -1);
                return message;
            }

            len += byte1;

            Console.WriteLine("ReadString: len={0}<", len);
            if (len >= MinLength)
            {
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                string s = streamEncoding.GetString(inBuffer);
                Console.WriteLine("ReadString:sAll={0}<", s);

                string checksum = s.Substring(0, MinLength);
                Console.WriteLine("ReadString:checksum={0}<", checksum);
                s = len > MinLength ? s.Substring(MinLength) : "";

                if (checksum != ComputeSha256Hash(phrase == null ? s : phrase))
                {
                    message = new Message("Checksum not valid.", 2);
                }
                else
                {
                    message = new Message(s);
                }
                Console.WriteLine("ReadString:sOut={0}<", s);
            }
            else
            {
                message = new Message("To short message.", 1);
                return message;
            }

            return message;
        }

        //
        // Summary:
        //      Write string to stream in format: 2 bytes + SHA256ofMessage + Message
        //      2 bytes = length of Message
        //      If Message is Hello Phrase then Message don't push to stream.
        //
        // Parameters:
        //   outString:
        //     A string for sending.
        //
        //   isMessage:
        //     If false - to stream pushed SHA256 only.
        public int WriteString(string outString, bool isMessage = true)
        {
            outString = isMessage ? createMessage(outString) : ComputeSha256Hash(outString);
            Console.WriteLine("<{0}", outString);
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue) // TODO: maybe need solution for long message
            {
                len = (int)UInt16.MaxValue;
            }

            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return len + 2;
        }

        private static string ComputeSha256Hash(string s)
        {
            s += BaseString;
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(s));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }
                return builder.ToString();
            }
        }

        // Add prefix with SHA256
        private string createMessage(string s)
        {
            if (s == null) s = "";
            return ComputeSha256Hash(s) + s;
        }
    }
}
