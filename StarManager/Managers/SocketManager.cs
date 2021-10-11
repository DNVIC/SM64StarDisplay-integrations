using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace StarDisplay
{
    
    public class SocketManager : CachedManager
    {
        public static int stars;
        public bool isClosed = false;
        public byte[] AcquiredData;
        public SocketManager(string ip, int port, string user)
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipaddr = IPAddress.Parse(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipaddr, port);
                Console.WriteLine(ipaddr.ToString());
                Socket sender = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
                try
                {
                    sender.Connect(localEndPoint);
                    byte[] ByteBuffer = new byte[1024];

                    Console.WriteLine("Socket connected to -> {0} ", sender.RemoteEndPoint.ToString());

                    string test = SendAndReceiveCommand(sender, user + " connected with " + stars + " stars", ByteBuffer);
                    Console.WriteLine(test);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected Exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
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

        }
    }
}
