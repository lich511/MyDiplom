using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string IP_ADRES = "192.168.0.106";
        static Socket SOCKET_FIRST = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static Socket[] SOCET_LIST = new Socket[10];
        static IPEndPoint IP_POINT = new IPEndPoint(IPAddress.Parse(IP_ADRES), 901);
        static bool MENAGER_SOCKET_CLOSE = true;
        static bool CHEK_SOCKET_CLOSE = true;
        static Thread MENAGER_SOCKET;
        static Thread CHEK_SOCKET;
        static int ID = 0;
        static int ID_ACCEPT = 0;
        static string[] LIST_NAME = new string[10]
        { "null", "null", "null", "null", "null", "null", "null", "null", "null", "null" };
        public MainWindow()
        {
            InitializeComponent();

            MENAGER_SOCKET = new Thread(new ThreadStart(Manager_socket));
            CHEK_SOCKET = new Thread(new ThreadStart(Chek_socket));

            MENAGER_SOCKET.Start();
            CHEK_SOCKET.Start();

            Write_list();
        }
        public void Manager_socket()
        {
            SOCKET_FIRST.Bind(IP_POINT);
            SOCKET_FIRST.Listen(10);
            do
            {
                try
                {
                    ID_ACCEPT = ID;
                    SOCET_LIST[ID_ACCEPT] = SOCKET_FIRST.Accept();
                    LIST_NAME[ID_ACCEPT] = Recive_messege_string(SOCET_LIST[ID_ACCEPT]);
                    List_handler();
                    Write_list();
                }
                catch (Exception) {}
            } while (MENAGER_SOCKET_CLOSE);
        }
        public void Chek_socket()
        {
            do
            {
                Thread.Sleep(2000);
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        if (LIST_NAME[i] != "null")
                        {
                            Send_Messege_string(SOCET_LIST[i], "chek");
                        }
                    }
                    catch (Exception)
                    {
                        SOCET_LIST[i].Disconnect(true);
                        LIST_NAME[i] = "null";
                        List_handler();
                        Write_list();
                    }
                }
            } while (CHEK_SOCKET_CLOSE);
        }
        public void Write_list()
        {
            Client_list.Items.Clear();
            foreach (var item in LIST_NAME)
            {
                Client_list.Items.Add(item);
            }
        }
        public void List_handler()
        {
            for (int i = 0; i < 10; i++)
            {
                if (LIST_NAME[i] == "null") { ID = i; break; }
            }
        }
        public string Recive_messege_string(Socket socket)
        {
            byte[] by = new byte[3];
            socket.Receive(by);
            int a = Convert.ToInt32(Encoding.UTF8.GetString(by));
            by = new byte[a];
            socket.Receive(by);
            return Encoding.UTF8.GetString(by);
        }
        public void Send_Messege_string(Socket socket, string messege)
        {
            socket.Send(Encoding.UTF8.GetBytes(messege.Length.ToString()));
            socket.Send(Encoding.UTF8.GetBytes(messege));
        }
        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MENAGER_SOCKET_CLOSE = false;
            CHEK_SOCKET_CLOSE = false;
        }
    }
}
