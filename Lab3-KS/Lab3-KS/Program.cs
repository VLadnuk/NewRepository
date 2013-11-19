using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Lab3_KS
{
    public delegate void FunctionHandler(string[] parameters);

    public class Data
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }
        public string Permission { get; set; }
        public bool Directory { get; set; }
        public List<Data> Children { get; set; }
    }

    public class Functions
    {
        private string[] _enum = { "---", "--x", "-w-", "-wx", "r--", "r-x", "rw-", "rwx" };
        private string _userPermission = "rwxrwxr-x";
        private string _userGroup = "root";
        private string _user = "root";
        private string _path = "/";
        private List<string> path = new List<string>() { "/" };
        private Data _currentData;
        private List<Data> _previosData = new List<Data>();

        private Data currentData
        {
            get
            {
                if (_currentData == null) _currentData = Data;
                return _currentData;
            }
            set { _currentData = value; }
        }

        public HashSet<string> usersList = new HashSet<string>() { "root" };
        public HashSet<string> groupsList = new HashSet<string>() { "root" };
        private Dictionary<string, string> _userGroups = new Dictionary<string, string>() { { "root", "root" } };

        private Data _data;
        private Dictionary<string, FunctionHandler> _dictionary;
        public Dictionary<string, FunctionHandler> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new Dictionary<string, FunctionHandler>
                    {
                        {"useradd", UserAdd},
                        {"groupadd ", GroupAdd},
                        {"su", Su},
                        {"cd", Cd},
                        {"mkdir", MkDir},
                        {"touch", Touch},
                        {"chmod", ChMod},
                        {"chgrp", ChGrp},
                        {"chown", ChOwn},
                        {"--clear", Clear},
                        {"command-list", CommandList}
                    };
                }
                return _dictionary;
            }
        }

        public string CurrentUser
        {
            get { return _user; }
            private set { _user = value; }
        }

        public string CurrentPath
        {
            get { return _path; }
            private set { _path = value; }
        }

        public Data Data
        {
            get
            {
                return _data ?? (_data = new Data()
                {
                    Name = "/",
                    Group = _userGroup,
                    Owner = _user,
                    Directory = true,
                    Permission = _userPermission
                });
            }
        }

        private void UserAdd(string[] parameters)
        {
            usersList.Add(parameters[1]);
            if (parameters.Count() == 2)
            {
                _userGroups.Add(parameters[1], parameters[1]);
                groupsList.Add(parameters[1]);
                _user = parameters[1];
                _userGroup = parameters[1];
                return;
            }

            if (parameters[2] == "-g")
            {
                _user = parameters[1];
                _userGroup = parameters[3];
                _userGroups.Add(parameters[1], parameters[3]);
                groupsList.Add(parameters[3]);
            }
        }
        private void GroupAdd(string[] parameters)
        {
            groupsList.Add(parameters[1]);
        }
        private void Su(string[] parameters)
        {
            if (_userGroups.ContainsKey(parameters[1]))
            {
                _user = parameters[1];
                _userGroups.TryGetValue(_user, out _userGroup);
            }
        }
        private void Cd(string[] parameters)
        {
            if (parameters[1] == "..")
            {
                if (path.Count > 1)
                    //if (_previosData[_previosData.Count - 1].Name == path[path.Count - 2])
                {
                    _path = "";
                    path.Remove(path[path.Count - 1]);
                    foreach (var p in path)
                    {
                        _path = p == "/" ? p : _path + p + "/";
                    }
                    currentData = _previosData[_previosData.Count - 1];
                    _previosData.Remove(_previosData[_previosData.Count - 1]);
                }
            }
            else
            {
                foreach (var child in currentData.Children.Where(child => child.Name == parameters[1] && child.Directory))
                {
                    _path = _path + parameters[1] + "/";
                    path.Add(parameters[1]);
                    _previosData.Add(currentData);
                    currentData = child;
                    break;
                }
            }
        }
        private void MkDir(string[] parameters)
        {
            if (currentData.Children == null) currentData.Children = new List<Data>();
            currentData.Children.Add(new Data()
            {
                Name = parameters[1],
                Group = _userGroup,
                Owner = _user,
                Directory = true,
                Permission = _userPermission
            });
        }
        private void Touch(string[] parameters)
        {
            if (currentData.Children == null) currentData.Children = new List<Data>();
            currentData.Children.Add(new Data()
            {
                Name = parameters[1],
                Group = _userGroup,
                Owner = _user,
                Directory = false,
                Permission = _userPermission
            });
        }
        private void ChMod(string[] parameters)
        {
            var data = new Data();
            foreach (var child in _currentData.Children)
            {
                if (child.Name == parameters[2])
                {
                    child.Permission = _enum[int.Parse(parameters[1][0].ToString())] +
                                       _enum[int.Parse(parameters[1][1].ToString())] +
                                       _enum[int.Parse(parameters[1][2].ToString())];
                    data = child;
                    break;
                }
            }

            if (parameters.Count() > 3)
            {
                if (parameters[3] != "-r") throw new Exception();
                else RecursiveEdit(parameters, 0, data);
            }

        }
        private void ChGrp(string[] parameters)
        {
            var data = new Data();
            foreach (var child in _currentData.Children)
            {
                if (child.Name == parameters[2])
                {
                    child.Group = parameters[1];
                    data = child;
                    break;
                }
            }

            if (parameters.Count() > 3)
            {
                if (parameters[3] != "-r") throw new Exception();
                else RecursiveEdit(parameters, 1, data);
            }
        }
        private void ChOwn(string[] parameters)
        {
            var data = new Data();
            foreach (var child in _currentData.Children)
            {
                if (child.Name == parameters[2])
                {
                    child.Owner = parameters[1];
                    data = child;
                    break;
                }
            }

            if (parameters.Count() > 3)
            {
                if (parameters[3] != "-r") throw new Exception();
                else RecursiveEdit(parameters, 2, data);
            }
        }
        private void Clear(string[] parameters)
        {
            _currentData = null;
            _previosData = new List<Data>();
            _data = null;
            _path = "/";
            path = new List<string>() { "/" };
        }
        private void CommandList(string[] parameters)
        {
            using (var file = new StreamReader(parameters[1]))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (this.Dictionary.ContainsKey(parameters[0]))
                        this.Dictionary[parameters[0]](parameters);
                }
            }
            UpdateFiles();
            Environment.Exit(0);
        }

        private void RecursiveEdit(string[] parameters, int type, Data data)
        {
            switch (type)
            {
                case 0:
                    data.Permission = _enum[int.Parse(parameters[1][0].ToString())] +
                                       _enum[int.Parse(parameters[1][1].ToString())] +
                                       _enum[int.Parse(parameters[1][2].ToString())];
                    break;
                case 1:
                    data.Group = parameters[1];
                    break;
                case 2:
                    data.Owner = parameters[1];
                    break;
            }

            if (data.Children != null)
            {
                foreach (var child in data.Children)
                {
                    RecursiveEdit(parameters, type, child);
                }
            }

        }

        public void UpdateFiles()
        {
            using (var writer = new StreamWriter("passwd.txt"))
            {
                foreach (var user in this.usersList)
                {
                    writer.WriteLine(user);
                }
            }
            using (var writer = new StreamWriter("group.txt"))
            {
                foreach (var group in this.groupsList)
                {
                    writer.WriteLine(group);
                }
            }
            using (var writer = new StreamWriter("fs.json"))
            {
                var serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(this.Data);
                writer.WriteLine(json);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var functions = new Functions();
            while (true)
            {
                Console.Write("{0}: {1} ", functions.CurrentUser, functions.CurrentPath);
                string[] parameters = Console.ReadLine().ToLower().Split(" ".ToCharArray());

                try
                {
                    if (functions.Dictionary.ContainsKey(parameters[0]))
                    {
                        functions.Dictionary[parameters[0]](parameters);
                        functions.UpdateFiles();
                    }
                    else Console.WriteLine("Wrong command");
                }
                catch (Exception)
                {
                    Console.WriteLine("Error");
                }

            }
        }
    }
}
