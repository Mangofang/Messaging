using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Socket Clientsocket;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(ServerConnect =>
            {
                Clientsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                richTextBox1.Text = "正在连接服务器...";
                try
                {
                    Clientsocket.Connect(IPAddress.Parse("127.0.0.1"), int.Parse("25565"));
                    richTextBox1.Text += "\n你已连接到服务器\n";
                }
                catch (Exception ex)
                {
                    richTextBox1.Text += "\n连接失败，正在重试...";
                    Thread.Sleep(5000);
                    Form1_Load(this, e);
                    return;
                }
                ThreadPool.QueueUserWorkItem(ReceiveData =>
                {
                    byte[] data = new byte[1024 * 1024];
                    while (true)
                    {
                        int len;
                        try
                        {
                            len = Clientsocket.Receive(data, 0, data.Length, SocketFlags.None);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("服务器:" + Clientsocket.RemoteEndPoint.ToString() + "被迫断开连接");
                            return;
                        }

                        if (len <= 0)
                        {
                            Console.WriteLine("服务器:" + Clientsocket.RemoteEndPoint.ToString() + "断开连接");
                            
                            return;
                        }
                        string str = Encoding.Default.GetString(data,0,len);
                        if (len != 0)
                        {
                            
                            richTextBox1.AppendText(str);
                        }
                    }
                });
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Clientsocket.Connected)
            {
                string ip = GetIP_();
                string send_msg = ip + ":" + textBox1.Text;
                byte[] data = Encoding.Default.GetBytes(send_msg);
                Clientsocket.Send(data, 0, data.Length, SocketFlags.None);
                textBox1.Text = "";
            }
        }
        public static string GetIP_()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);
                var addressV = iPHostEntry.AddressList.FirstOrDefault(q => q.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (addressV != null)
                    return addressV.ToString();
                return "127.0.0.1";
            }
            catch (Exception ex)
            {
                return "127.0.0.1";
            }

        }
    }
}
