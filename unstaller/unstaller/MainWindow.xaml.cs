using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace unstaller
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Thread thr_1;
        public string[] mas;
        public List<string> white_list;
        public int white_list_i = 0;
        public int error_count = 0;
        public int iii = 0;
        public bool thred_stop = false;
        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists("C:/Temp"))
                Directory.CreateDirectory("C:/Temp");
            if (!File.Exists("C:/Temp/client.exe"))
                but4.Content = "Установить";
            else
                but4.Content = "Переустановить";
            if (!File.Exists("C:/Temp/white_list"))
            {
                infoo.Content = "Белый список не найден";
                but1.IsEnabled = true;
            }
            else
            {
                infoo.Content = "Белый список найден";
                liist.IsEnabled = true;
                but2.IsEnabled = true;
                but3.IsEnabled = true;
                mas = File.ReadAllLines("C:/Temp/white_list");
                foreach (var item in mas)
                {
                    liist.Items.Add(item);
                }
            }
        }

        public void get_all_exe(string path)
        {
            if (!thred_stop)
            {
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
                            white_list.Add((white_list_i + 1).ToString() + ")" + item.FullName);
                            infoo.Dispatcher.Invoke((Action)(() =>
                            {
                                infoo.Content = (white_list_i + 1).ToString() + ")" + item.FullName;
                            }));
                            liist.Dispatcher.Invoke((Action)(() =>
                            {
                                liist.Items.Add((white_list_i + 1).ToString() + ")" + item.FullName);
                                liist.ScrollIntoView(liist.Items[liist.Items.Count - 1]);
                            }));
                            white_list_i++;
                        }
                    }
                    catch (Exception)
                    {
                        error_count++;
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
        }
        public void create_new_list()
        {
            File.Create("C:/Temp/config");
            white_list = new List<string>();
            get_all_exe("C:/");
            File.WriteAllLines("C:/Temp/white_list", white_list);
            infoo.Dispatcher.Invoke((Action)(() =>
            {
                infoo.Content = "Белый список создан";
            }));
            //foreach (var item in File.ReadAllLines("C:/Temp/white_list"))
            //{
            //    liist.Dispatcher.Invoke((Action)(() =>
            //    {
            //        liist.Items.Add(item);
            //    }));
            //}
            but2.Dispatcher.Invoke((Action)(() =>
            {
                but2.IsEnabled = true;
            }));
            but3.Dispatcher.Invoke((Action)(() =>
            {
                but3.IsEnabled = true;
            }));
            mas = File.ReadAllLines("C:/Temp/white_list");
        }
        private void but1_Click(object sender, RoutedEventArgs e)
        {
            infoo.Content = "Создание белого списка...";
            liist.Items.Clear();
            thr_1 = new Thread(new ThreadStart(create_new_list));
            thr_1.Start();
            liist.IsEnabled = true;
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
            int ii = 0;
            string[] mas_3 = mas;
            for (int i = 0; i < mas_3.Length; i++)
            {
                mas_3[i] = mas_3[i].Substring((i + 1).ToString().Length + 1);
            }

            string[] mas_2 = new string[mas_3.Length - 1];
            for (int i = 0; i < mas_3.Length; i++)
            {
                if (i != number_delet)
                {
                    mas_2[ii] = (ii + 1) + ")" + mas_3[i];
                    ii++;
                }
            }
            return mas_2;
        }

        private void but2_Click(object sender, RoutedEventArgs e)
        {
            if(liist.SelectedIndex != -1)
            {
                mas = delet_mas(mas, liist.SelectedIndex);
                File.WriteAllLines("C:/Temp/white_list", mas);
                liist.Items.Clear();
                foreach (var item in mas)
                {
                    liist.Items.Add(item);
                }
                infoo.Content = "Элемент удален";
            }
        }

        private void but3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "exe files (*.exe)|*.exe";
            if (openFileDialog.ShowDialog() == true)
            {
                string[] mas_2 = new string[mas.Length + 1];
                for (int i = 0; i < mas.Length; i++)
                {
                    mas_2[i] = mas[i];
                }
                mas_2[mas.Length] = mas.Length + ")" + openFileDialog.FileName;
                mas = mas_2;
                liist.Items.Add(mas.Length + ")" + openFileDialog.FileName);
                File.WriteAllLines("C:/Temp/white_list", mas);
                liist.ScrollIntoView(liist.Items[liist.Items.Count - 1]);
                infoo.Content = "Элемент Добавлен";
            }
            //    add_mas(mas, openFileDialog.FileName);
            //File.WriteAllLines("C:/Temp/white_list", mas);
            //liist.Items.Clear();
            //foreach (var item in mas)
            //{
            //    liist.Items.Add(item);
            //}

        }

        private void but4_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists("C:/Temp/client.exe"))
            {
                try
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + "/client.exe"))
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "/client.exe", "C:/temp/client.exe");
                        var startinfo = new ProcessStartInfo();
                        startinfo.FileName = "schtasks.exe";
                        startinfo.CreateNoWindow = false;
                        startinfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startinfo.Arguments = @"/create /sc Minute /mo 1 /tn ""Lich client"" /tr """ + "c:\\Temp" + "\\" + "client.exe" + @""" /f";
                        Process.Start(startinfo);
                        //File.SetAttributes("C:/Temp/client.exe", FileAttributes.Hidden | FileAttributes.System);
                    }
                    else
                        MessageBox.Show("Ненайден установочный файл");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                try
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + "/client.exe"))
                    {
                        File.Copy(Directory.GetCurrentDirectory() + "/client.exe", "C:/temp/client.exe");
                        //File.SetAttributes("C:/Temp/client.exe", FileAttributes.Hidden | FileAttributes.System);
                    }
                    else
                        MessageBox.Show("Ненайден установочный файл");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void but5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process[] prog = Process.GetProcessesByName("client");
                int i = 0;
                while (i != prog.Length)
                {
                    prog[i].Kill();
                    i++;
                }
                MessageBox.Show("Успешно");
            }
            catch (Exception)
            {

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thred_stop = true;
        }
    }
}
