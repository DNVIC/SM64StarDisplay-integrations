using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace StarDisplay
{
    public struct MessageContents
    {
        public string Password;
        public string Username;
        public string Info;
        public MessageContents(string Password, string Username, string Info)
        {
            this.Password = Password;
            this.Username = Username;
            this.Info = Info;
        }
    }
    public class SocketManager : CachedManager
    {
        public static int stars;
        public bool isClosed = false;
        public static bool isInitialized = false;
        public byte[] AcquiredData;

        private static IPAddress _ip;
        private static int _port;
        private static string _user;
        private static string _password;
        public SocketManager(string ip, int port, string user, string password)
        {
            IPAddress ipaddr = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipaddr, port);
            Console.WriteLine(ipaddr.ToString());
            Socket sender = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
            sender.Connect(localEndPoint);
            byte[] ByteBuffer = new byte[1024];

            Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint.ToString());

            MessageContents message = new MessageContents(user, password, stars.ToString());
            string ReceivedString = SendAndReceiveCommand(sender, JsonConvert.SerializeObject(message), ByteBuffer);
            Console.WriteLine(ReceivedString);
            sender.Close();
            if(ReceivedString.StartsWith("PASSERROR"))  // maybe switch case is better here but cba
            {
                throw new Exception("Inserted incorrect password, please try again");
            }
            if(ReceivedString.StartsWith("USERERROR"))
            {
                throw new Exception("Inserted username not in the list of possible participants on the restream, contact DNVIC#2075 on discord if you think this is a mistake.");
            }
            if(ReceivedString.StartsWith("ERROR"))
            {
                throw new Exception("Unexpected Error: " + ReceivedString.Substring(5));
            }
            isInitialized = true;
            _ip = ipaddr;
            _port = port;
            _user = user;
            _password = password;
        }
        static string SendAndReceiveCommand(Socket s, string Command, byte[] Buffer)
        {
            byte[] spinx = Encoding.ASCII.GetBytes(Command + "<EOF>");
            s.Send(spinx);
            int recv = s.Receive(Buffer);
            return Encoding.ASCII.GetString(Buffer, 0, recv);
        }


        public static void SetStars(int starNumber)
        {
            Console.WriteLine(starNumber + "stars" );
            stars = starNumber;
            if(isInitialized)
            {
                IPAddress ipaddr = _ip;
                IPEndPoint localEndPoint = new IPEndPoint(ipaddr, _port);
                Console.WriteLine(ipaddr.ToString());
                Socket sender = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                sender.Connect(localEndPoint);
                byte[] ByteBuffer = new byte[1024];

                Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint.ToString());

                MessageContents message = new MessageContents(_user, _password, stars.ToString());
                string ReceivedString = SendAndReceiveCommand(sender, JsonConvert.SerializeObject(message), ByteBuffer);
                Console.WriteLine(ReceivedString);
                sender.Close();
                if (ReceivedString.StartsWith("PASSERROR"))  // maybe switch case is better here but cba
                {
                    throw new Exception("Inserted incorrect password, please try again");
                }
                if (ReceivedString.StartsWith("USERERROR"))
                {
                    throw new Exception("Inserted username not in the list of possible participants on the restream, contact DNVIC#2075 on discord if you think this is a mistake.");
                }
                if (ReceivedString.StartsWith("ERROR"))
                {
                    throw new Exception("Unexpected Error: " + ReceivedString.Substring(5));
                }
            }
        }
    }
}
