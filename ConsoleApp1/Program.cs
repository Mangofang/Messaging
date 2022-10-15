using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        public static List<Socket> ClientProxSocketList = new List<Socket>();
        public static string str = "";
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"),int.Parse("25565")));
            socket.Listen(10);
            Console.WriteLine("IP端口配置完毕");
            ThreadPool.QueueUserWorkItem(ClientConnect =>
            {
                while (true)
                {
                    Socket proxSocket = socket.Accept();
                    ClientProxSocketList.Add(proxSocket);
                    Console.WriteLine("已与 " + proxSocket.RemoteEndPoint + "建立连接");
                    ThreadPool.QueueUserWorkItem(ReceiveData =>
                    {
                        byte[] data = new byte[1024 * 1024];
                        while (true)
                        {
                            int len = 0;
                            try
                            {
                                len = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                                if (len == 0) break;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("客户端:" + proxSocket.RemoteEndPoint.ToString() + "被迫断开连接");
                                ClientProxSocketList.Remove(proxSocket);
                                return;
                            }

                            if (len <= 0)
                            {
                                Console.WriteLine("客户端:" + proxSocket.RemoteEndPoint.ToString() + "断开连接");
                                ClientProxSocketList.Remove(proxSocket);
                                return;
                            }

                            str = Encoding.Default.GetString(data, 0, len);

                            ThreadPool.QueueUserWorkItem(SendMsg =>
                            {
                                try
                                {
                                    foreach (Socket proxSocket_ in ClientProxSocketList)
                                    {
                                        if (proxSocket_.Connected)
                                        {
                                            byte[] data_ = Encoding.Default.GetBytes(str + "\n");
                                            proxSocket_.Send(data_, 0, data_.Length, SocketFlags.None);
                                        }
                                    }
                                }
                                catch { }
                            });
                        }
                    });
                }
            });
            Console.ReadLine();
        }
    }
}
