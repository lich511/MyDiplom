using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace client
{
    class Program
    {
        static string[] WHITE_LIST;
        static List<string> WHITE_LIST_NEW;
        static Thread[] threads = new Thread[10];
        static bool[] IS_ACTIVE = new bool[10];
        static string[] CHEKED_P = new string[0];
        static int WHITE_LIST_I = 0;
        static int error_count = 0;
        static int iii = 0;

        static Thread TCP_MANAGER;
        static bool TCP_MANAGER_CLOSE = true;
        static Socket SOCKET = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static IPEndPoint IP;
        static void Main(string[] args)
        {
            if(!File.Exists("C:/Temp/white_list"))
            {
                //WHITE_LIST_NEW = new List<string>();
                //Console.WriteLine("сканирование");
                ////Console.WriteLine(ii + "/" + iii);
                //get_all_exe("C:/");
                //Console.Clear();
                //Console.WriteLine("первичное сканирование завершено");
                //Console.WriteLine("Количество найденных exe файлов: " + WHITE_LIST_NEW.Count());
                //Console.WriteLine("Количество сломаных сканеров: " + error_count);
                //File.WriteAllLines("C:/Temp/white_list", WHITE_LIST_NEW);
                //Console.WriteLine("белый список не найден");
                //Console.ReadKey();
            }
            if (File.Exists("C:/Temp/white_list"))
            {
                Console.WriteLine("white list найден");
                WHITE_LIST = File.ReadAllLines("C:/Temp/white_list");
                TCP_MANAGER = new Thread(new ThreadStart(Tcp_manager));
                TCP_MANAGER.Start();
                error_count = 0;

                for (int i = 0; i < WHITE_LIST.Length; i++)
                {
                    WHITE_LIST[i] = WHITE_LIST[i].Substring((i + 1).ToString().Length + 1);
                }

                Process[] a = Process.GetProcesses();
                foreach (var item in a)
                {
                    CHEKED_P = add_mas(CHEKED_P, item.Id.ToString());
                }

                chekk();
            }
        }
        static void chekk()
        {
            do
            {
                Thread.Sleep(1000);
                Process[] proccess = Process.GetProcesses();
                foreach (var item in proccess)
                {
                    First_chek(item, chek_mass());
                }
                int i = option_chek(proccess);
                Console.WriteLine("----цикл завершон----");
                Console.WriteLine(DateTime.Now + "/n");
                Console.WriteLine("Количество отключенных процессов: " + i);
            } while (true);
        }
        static void get_all_exe(string path)
        {
            //Thread.Sleep(10);
            DirectoryInfo dir = new DirectoryInfo(path);

            iii++;
            Console.WriteLine(iii + ")" + dir.FullName);

            FileInfo[] files = dir.GetFiles();
            foreach (var item in files)
            {
                try
                {
                    if (item.Extension == ".exe")
                    {
                        WHITE_LIST_NEW.Add((WHITE_LIST_I + 1).ToString() + ")" + item.FullName);
                        WHITE_LIST_I++;
                    }
                }
                catch (Exception)
                {

                }
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (var item in dirs)
            {
                try
                {
                    get_all_exe(item.FullName);
                }
                catch (Exception)
                {
                    error_count++;
                }
            }
        }
        static void First_chek(Process proc, int i_thred)
        {
            bool is_find = false;
            try
            {
                foreach (var item in CHEKED_P)
                {
                    if (item == proc.Id.ToString())
                    {
                        IS_ACTIVE[i_thred] = false;
                        is_find = true;
                        break;
                    }
                }
                if(is_find == false)
                    Secons_chek(proc, i_thred);
            }
            catch (Exception)
            {
                IS_ACTIVE[i_thred] = false;
            }
        }
        static void Secons_chek(Process proc, int i_thred)
        {
            bool is_find = false;
            try
            {
                string path_proc = proc.MainModule.FileName;
                for (int i = 0; i < WHITE_LIST.Length; i++)
                {
                    if(WHITE_LIST[i] == path_proc)
                    {
                        add_mas(CHEKED_P, proc.Id.ToString());
                        IS_ACTIVE[i_thred] = false;
                        is_find = true;
                        break;
                    }    
                }
                if(is_find == false)
                {
                    Console.WriteLine("----Обнаруженн шпион-----");
                    Console.WriteLine(proc.Id + "|" + path_proc);
                    Console.WriteLine(DateTime.Now + "/n");
                    IS_ACTIVE[i_thred] = false;
                }
            }
            catch (Exception)
            {
                IS_ACTIVE[i_thred] = false;
            }
        }
        static int option_chek(Process[] procs)
        {
            bool is_find = false;
            int ii = 0;
            for (int i = 0; i < CHEKED_P.Length; i++)
            {
                foreach (var item in procs)
                {
                    if(CHEKED_P[i] == item.Id.ToString())
                    {
                        is_find = true;
                        break;
                    }
                }
                if(!is_find)
                {
                    CHEKED_P = delet_mas(CHEKED_P, i);
                    i++;
                }
            }
            return ii;
        }
        static string[] add_mas(string[] mas, string new_str)
        {
            string[] mas_2 = new string[mas.Length + 1];
            for (int i = 0; i < mas.Length; i++)
            {
                mas_2[i] = mas[i];
            }
            mas_2[mas.Length] = new_str;
            return mas_2;
        }
        static string[] delet_mas(string[] mas, int number_delet)
        {
            string[] mas_2 = new string[mas.Length - 1];
            for (int i = 0; i < mas.Length; i++)
            {
                if (i != number_delet)
                    mas_2[i] = mas[i];
            }
            return mas_2;
        }
        static int chek_mass()
        {
            do
            {
                for (int i = 0; i < IS_ACTIVE.Length; i++)
                {
                    if (IS_ACTIVE[i] == false)
                        return i;
                }
                Thread.Sleep(500);
            } while (true);
        }

        static void Tcp_manager()
        {
            do
            {
                IP = new IPEndPoint(IPAddress.Parse(File.ReadAllLines("C:/Temp/config")[0]), 901);
                do
                {
                    try
                    {

                        SOCKET.Connect(IP);
                        Send_Messege_string(SOCKET, File.ReadAllLines("C:/Temp/config")[1]);
                        break;
                    }
                    catch { }
                } while (TCP_MANAGER_CLOSE);//цикл подключения
                Console.WriteLine("Подкючение успешно");
                do
                {
                    try
                    {
                        string COMAND = Recive_messege_string(SOCKET);
                        switch (COMAND)
                        {
                            case "chek": break;//проверка что клиент вообще жив
                        }
                    }
                    catch 
                    {
                        SOCKET.Close();
                        break;
                    }
                } while (TCP_MANAGER_CLOSE);
            } while (TCP_MANAGER_CLOSE);
        }
        private void OnApplicationExit(object sender, EventArgs e)
        {
            TCP_MANAGER_CLOSE = false;
        }
        static string Recive_messege_string(Socket socket)
        {
            byte[] by = new byte[3];
            socket.Receive(by);
            by = new byte[Convert.ToInt32(Encoding.UTF8.GetString(by))];
            socket.Receive(by);
            return Encoding.UTF8.GetString(by);
        }
        static void Send_Messege_string(Socket socket, string messege)
        {
            byte[] a = Encoding.UTF8.GetBytes(messege.Length.ToString());
            socket.Send(Encoding.UTF8.GetBytes(messege.Length.ToString()));
            socket.Send(Encoding.UTF8.GetBytes(messege));
        }
    }
}
