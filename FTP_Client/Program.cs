using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace FTP_Client
{
    class Program
    {
       static string HOST = "127.0.0.1:58214/";
       static FtpWebRequest ftpRequest;
       static  FtpWebResponse ftpResponse;

        static void Main(string[] args)
        {
            Console.WriteLine("Введите логин (если у вас его нет - ничего не вводите):");

            //получение логина пользователя
            string login = Console.ReadLine();

            //флаг анонимного пользователя (по умолчанию false)
            bool anonimous = false; 
            
            //проверка на анонимного пользователя: если пользователь не ввел логин, то считается, что он анонимный
            if (login.Equals("")) {
                anonimous = true; 
            }

            Console.WriteLine("Введите пароль (если у вас его нет - ничего не вводите):");

            //получение пароля пользователя
            string password = Console.ReadLine();

            //-----------------------------------------------------просмотр файлов-----------------------------------------------------------------
            try
            {
                // объект запроса
                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + HOST);
                //логин и пароль (если есть)
                if (anonimous == false)
                {
                    ftpRequest.Credentials = new NetworkCredential(login, password);
                }
                //команда ftp LIST
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                //Получаем входящий поток
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                //переменная для хранения всей полученной информации
                string content = "";

                StreamReader sr = new StreamReader(ftpResponse.GetResponseStream(), System.Text.Encoding.ASCII);
                content = sr.ReadToEnd();
                sr.Close();
                ftpResponse.Close();
                content = content.Replace("drwxr-xr-x", "папка");
                content = content.Replace("-rw-r--r--", "файл");
                Console.WriteLine(content);

                Console.WriteLine();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
 //--------------------------------------------------- выбор папки-------------------------------------------------------------------
            bool choose_folder=false;
            string folder_name = "";
            Console.WriteLine("Введите название папки, в которую хотите зайти:");
            while (choose_folder != true)
            {
                folder_name = Console.ReadLine();
                string HOST_1 = "127.0.0.1:58214/"+folder_name;
                try
                {
                    ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + HOST_1);
                    if (anonimous == false)
                    {
                        ftpRequest.Credentials = new NetworkCredential(login, password);
                    }
                    ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                    string content = "";
                    StreamReader sr = new StreamReader(ftpResponse.GetResponseStream(), System.Text.Encoding.ASCII);
                    content = sr.ReadToEnd();
                    sr.Close();
                    ftpResponse.Close();
                    content = content.Replace("drwxr-xr-x", "папка");
                    content = content.Replace("-r--r--r--", "файл");
                    Console.WriteLine(content);
                    choose_folder = true;

                }
                catch (Exception e)
                {
                    Console.WriteLine("Такой папки не существует, потовторите попытку:");
                }
            }
            Console.WriteLine();
 //-------------------------------------------скачивание файла---------------------------------------------------------
            Console.WriteLine("Напишите, какой файл хотите скачать:");
            string file_name = Console.ReadLine();
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + HOST + folder_name + "/" + file_name);
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream fs = new FileStream("C:\\Users\\user\\source\\repos\\FTP_Client\\newText.txt", FileMode.Create);
                byte[] buffer = new byte[64];
                int size = 0;

                while ((size = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fs.Write(buffer, 0, size);

                }
                fs.Close();
                response.Close();
                Console.WriteLine("Файл скачан!");

            }
            catch (Exception e) {
                if (e.ToString().Contains("File unavailable"))
                {
                    Console.WriteLine("Неправильное имя файла");
                }
                else
                {
                    Console.WriteLine(e);
                }
            }
            Console.WriteLine();
 //------------------------------------------------загрузка файла на сервер ---------------------------------------------------------
            Console.WriteLine("Загрузка файла на сервер...");
            string upload_file = "newText.txt";
            string path_file = "C:\\Users\\user\\source\\repos\\FTP_Client\\newText.txt";
            try
            {
                FileStream uploadedFile = new FileStream(path_file, FileMode.Open, FileAccess.Read);
                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://127.0.0.1:58214/"+ upload_file);
                if (anonimous == false)
                {
                    ftpRequest.Credentials = new NetworkCredential(login, password);
                }
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                byte[] file_to_bytes = new byte[uploadedFile.Length];
                //Считываем данные в буфер
                uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);
                uploadedFile.Close();

                //Поток для загрузки файла 
                Stream writer = ftpRequest.GetRequestStream();

                writer.Write(file_to_bytes, 0, file_to_bytes.Length);
                writer.Close();
                Console.WriteLine("Загрузка завершена!");
            }
            catch (Exception e) {
                Console.WriteLine("У вас недостаточно прав на загрузку файлов");
                Console.WriteLine(e);
            }

            Console.WriteLine();
 //------------------------------------------------создание директории ---------------------------------------------------------
            Console.WriteLine("Введите название директории, которую хотите создать:");
            string folderName = Console.ReadLine();
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + HOST + "/" + folderName);
                if (anonimous == false)
                {
                    ftpRequest.Credentials = new NetworkCredential(login, password);
                }
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpResponse.Close();
                Console.WriteLine("Директория успено создана!");
            }
            catch (Exception e) {
                Console.WriteLine("У вас недостаточно прав на создание директории");
                Console.WriteLine(e);
            }



        }
    }
}
