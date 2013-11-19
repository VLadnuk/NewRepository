using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Lab2_CS
{
    public class Data
    {
        public string Password { get; set; }
        public int Access { get; set; }
    }

    public class DictionaryWriter
    {
        private Dictionary<string, Data> _dictionary = new Dictionary<string, Data>();

        public Dictionary<string, Data> GetDictionary(string filePath)
        {
            var reader = new StreamReader(filePath);
            while (reader.Peek() != -1)
            {
                string[] str = reader.ReadLine().Split(" ".ToCharArray());
                var data = new Data();
                if (str.Length == 3)
                {
                    data.Password = str[1];
                    data.Access = Convert.ToInt32(str[2]);
                }
                else data.Access = Convert.ToInt32(str[1]);
                _dictionary.Add(str[0], data);
            }
            return _dictionary;
        }
    }

    public static class AuthorizationService
    {
        public static Dictionary<string, Data> Authorization { get; private set; }
        public static string SignedUser { get; private set; }
        public static int UserAccess { get; private set; }

        public static void WriteDictionaryFromFile(string filePath)
        {
            var dictionaryWriter = new DictionaryWriter();
            Authorization = dictionaryWriter.GetDictionary(filePath);
        }

        public static bool Login(string login, string password)
        {
            var data = new Data();
            Authorization.TryGetValue(login, out data);
            if (data != null && data.Password == password)
            {
                SignedUser = login;
                UserAccess = data.Access;
                return true;
            }

            return false;
        }

        public static void Logoff()
        {
            SignedUser = null;
            UserAccess = 0;
        }
    }

    public class AccessService
    {
        public Dictionary<string, Data> Access { get; private set; }

        public void WriteDictionaryFromFile(string filePath)
        {
            var dictionaryWriter = new DictionaryWriter();
            Access = dictionaryWriter.GetDictionary(filePath);
        }

        public bool? FileIsAccess(string fileName)
        {
            var data = new Data();
            Access.TryGetValue(fileName, out data);
            if (data == null) return null;
            if (data.Access <= AuthorizationService.UserAccess) return true;

            return false;
        }

        public void OpenFile(string directory, string fileName)
        {
            if (this.FileIsAccess(fileName) == true)
            {
                Process.Start("notepad", string.Format("{0}\\{1}", directory, fileName));
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var accessService = new AccessService();

            Console.WriteLine("Enter file directory name");
            string directory = Console.ReadLine();

            AuthorizationService.WriteDictionaryFromFile(directory + "/auth.txt");
            accessService.WriteDictionaryFromFile(directory + "/access.txt");

            while (true)
            {
                Console.Clear();
                string login;
                string password;
                Console.WriteLine("Enter your login");
                login = Console.ReadLine();
                Console.WriteLine("And password");
                password = Console.ReadLine();

                if (AuthorizationService.Login(login, password))
                {
                    while (true)
                    {
                        Console.Clear();
                        string fileName;
                        var files = Directory.GetFiles(directory);
                        Console.WriteLine("Files in directory:");
                        foreach (var f in files)
                        {
                            var file = f.Replace(directory + "\\", string.Empty);
                            if (file == "auth.txt" || file == "access.txt") continue;
                            Console.WriteLine(file);
                        }
                        Console.WriteLine(string.Empty);

                        Console.WriteLine("Enter file name");
                        fileName = Console.ReadLine().Replace(".txt", string.Empty);
                        var access = accessService.FileIsAccess(fileName);
                        if(access == true) 
                            accessService.OpenFile(directory, fileName);
                        else if(access == false) Console.WriteLine("Access error!");
                        else Console.WriteLine("File not found!");
                        Console.WriteLine(string.Empty);

                        Console.WriteLine("Press \"Esc\" to exit, any other key to continue");
                        var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.Escape)
                        {
                            AuthorizationService.Logoff();
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Login failure! Please try again");
                    Console.ReadLine();
                }
            }
        }
    }
}
