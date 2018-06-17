using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace PR2_Client
{
    class BaseSocket : Socket
    {
        protected bool isConnected;
        private int sendnum = 1;
        private string m_EncryptionKey = "QHE0NSNwKWZZQVEhU19xMA==";
        public delegate void PR2Event(PR2Packet m_Pkt);
        public event PR2Event m_Event;
        private Timer timer; 
        public BaseSocket(string ip, int port, string user, string pass, int server_id) : base(createIP(port, ip).AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            timer = new Timer(handleTimerElapsed);
            timer.Change(0, 10000);
            IPAddress m_iAddr = createIP(port, ip);
            IPEndPoint m_endPoint = new IPEndPoint(m_iAddr, port);
            try
            {
                //BeginConnect(m_endPoint, new AsyncCallback(ConnectCallback), this);
                Connect(m_endPoint);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
            if (Connected)
            {
                isConnected = true;
                Receive();
                m_Event += new PR2Event(handleRecvChat);
            }
        }

        private void handleTimerElapsed(object StateInfo)
        {
            if(Connected)
                Send("ping`", null);
        }
        private static IPAddress createIP(int port, string ip)
        {
            IPAddress m_ipAddress = IPAddress.Parse(ip);
            return m_ipAddress;
        }
        public void chat(string m_Chat)
        {
            Send("chat`", new String[]{m_Chat});
        }
        public void Send(string m_Send, string[] m_Data)
        {
            StringBuilder m_Builder = new StringBuilder();
            m_Builder.Append(m_Send);
            if (m_Data != null)
            {
                int count = 0;
                foreach (String i in m_Data)
                {
                    m_Builder.Append(i);
                    count++;
                    if(count < m_Data.Length)
                        m_Builder.Append("`");
                }
            }
            byte[] m_Byte = genHash(m_Builder.ToString());
            sendnum++;
            BeginSend(m_Byte, 0, m_Byte.Length, 0, null, this);
        }

        private void Receive()
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = this;

                // Begin receiving the data from the remote device.  
                BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            StateObject m_Result = (StateObject)result.AsyncState;
            int byteToRead = EndReceive(result);
            if (byteToRead > 0)
            {
                using (MemoryStream stream = new MemoryStream(m_Result.buffer))
                {
                    using (BinaryReader read = new BinaryReader(stream))
                    {
                        while (true)
                        {
                            byte[] buf = new byte[4096];
                            int i = 0;
                            while (read.PeekChar() != '\u0004' && i < buf.Length)
                            {
                                buf[i++] = read.ReadByte();
                            }
                            String m_Pkt = Encoding.UTF8.GetString(buf, 0, i);
                            String[] m_Vars = m_Pkt.Split('`');
                            String m_Hash = m_Vars[0];
                            int m_Write = Int32.Parse(m_Vars[1]);
                            PR2Packet pack = new PR2Packet();
                            pack.opCode = m_Vars[2];
                            pack.data = new String[m_Vars.Length - 2];
                            for (int k = 3; k < m_Vars.Length; k++)
                            {
                                pack.data[k - 3] = m_Vars[k];
                            }

                            handlePacket(pack);

                            read.ReadByte();
                            var a = Int32.Parse(read.PeekChar().ToString());
                            if (a == 0 || i >= buf.Length)
                                break;
                        }
                    }
                }
                m_Result.buffer = new byte[StateObject.BufferSize];
            }
            BeginReceive(m_Result.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), m_Result);
        }

        private void handleRecvChat(PR2Packet m_Pkt)
        {
            if (m_Pkt.opCode.Equals("chat"))
            {
                String m_User = m_Pkt.data[0];
                String m_Chat = m_Pkt.data[2];
                Console.WriteLine("Chat message from " + m_User + ":" + m_Chat);
            }
        }
        private void handlePacket(PR2Packet m_Pkt)
        {
            m_Event(m_Pkt);
             /*  Console.Write("Packet opCode: " + m_Pkt.opCode + " Data:");
               for(int i = 0; i < m_Pkt.data.Length; i++)
                   Console.Write(m_Pkt.data[i] + ",");
                Console.Write("\n");*/
        }
        private byte[] genHash(string name)
        {
            string m_Var = m_EncryptionKey + sendnum + "`" + name; // Lol
            string md5 = createMD5(m_Var).Substring(0, 3);
            string res = md5 + "`" + sendnum + "`" + name + Char.ToString('\u0004');
            return Encoding.UTF8.GetBytes(res);
        }
        private static string createMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().Substring(0, 3).ToLower();
            }
        }
        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 4096;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
        }
    }
}
