using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Bug_o_Tron
{
    public class Program
    {
        #region Constructor 

        private static Program MainProgram;

        static void Main(string[] args)
        {
            MainProgram = new Program();
            MainProgram.Start();
        }

        #endregion

        private DiscordClient discordClient;

        private const ulong ServerID = 194099156988461056;
        private const string ChannelID = "bugs";

        private Server Server;
        private Channel BugsChannel;

        private string AppDirectory;

        private List<string> Admins = new List<string>(); 
        private List<string> Bugs = new List<string>(); 

        public void Start()
        {
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

            LoadFiles();

            discordClient = new DiscordClient();

            discordClient.MessageReceived += async (obj, args) =>
            {
                await Task.Run(() => ProcessMessage(args));
            };

            discordClient.ExecuteAndWait(async () =>
            {
                await discordClient.Connect("MjA4MTkyMjA0NTM4MjQ5MjE3.CnuFmQ.JChooQG0LBg9d4ZTrTEE85bdZjM");

                await Task.Delay(1000);

                Server = discordClient.Servers.First(s => s.Id == ServerID);
                BugsChannel = Server.AllChannels.First(c => c.Name == "bugs");

                Console.WriteLine(" ");
                Console.WriteLine("Loaded bot to server " + Server.Name + " in channel #" + BugsChannel);
            });
        }

        private void LoadFiles()
        {
            if (File.Exists(AppDirectory + "admins.list"))
            {
                string[] admins = File.ReadAllText(AppDirectory + "admins.list").Split(new string[1] {","}, StringSplitOptions.RemoveEmptyEntries);

                Console.WriteLine("Loading admins (" + admins.Length + ") :");

                foreach (string admin in admins)
                {
                    Admins.Add(admin);
                    Console.WriteLine("· " + admin);
                }
            }
            else
            {
                File.Create(AppDirectory + "admins.list").Close();

                Console.WriteLine("Created empty admin list");
            }

            Console.WriteLine(" ");

            if (File.Exists(AppDirectory + "bugs.list"))
            {
                string[] bugs = File.ReadAllText(AppDirectory + "bugs.list").Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);

                Console.WriteLine("Loading bugs (" + bugs.Length + ") :");

                foreach (string bug in bugs)
                {
                    Bugs.Add(bug);
                    Console.WriteLine("· " + bug);
                }
            }
            else
            {
                File.Create(AppDirectory + "bugs.list").Close();

                Console.WriteLine("Created empty bug list");
            }
        }

        private void ProcessMessage(MessageEventArgs args)
        {
            if (args.Message.IsAuthor == false )
            {
                if (args.Server.Id == ServerID && args.Channel.Name == ChannelID)
                {
                    string fullText = args.Message.Text;

                    if (fullText.StartsWith("!"))
                    {
                        string[] commands = fullText.Split();
                        string fullUser = args.User.ToString();
                        bool isAdmin = Admins.Contains(fullUser);

                        switch (commands[0])
                        {
                            case "!addadmin":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    AddAdminCommand(commands[1]);
                                }
                                break;

                            case "!removeadmin":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    RemoveAdminCommand(commands[1]);
                                }
                                break;

                            case "!adminlist":
                                ShowAdminListCommand();
                                break;

                            case "!report":
                                if (commands.Length > 1)
                                {
                                    AddBugCommand(fullText.Substring(commands[0].Length + 1), fullUser);
                                }
                                break;

                            case "!fix":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    RemoveBugCommand(int.Parse(commands[1]));
                                }
                                break;

                            case "!clearbuglist":

                                break;

                            case "!buglist":
                                ShowBugListCommand();
                                break;
                        }
                    }
                }
            }
        }

        #region Admin Related Methods

        private void ShowAdminListCommand()
        {
            BugsChannel.SendMessage("**Showing current admin list **(" + DateTime.Today.ToShortDateString() + ")** :**");

            string adminList = "```";

            for (int i = 0; i < Admins.Count; i++)
            {
                adminList += "· " + Admins[i] + "\n";
            }

            BugsChannel.SendMessage(adminList.Remove(adminList.Length - 3) + " ```");
        }

        private void AddAdminCommand(string admin)
        {
            if (Server.Users.Any(u => u.ToString() == admin))
            {
                if (Admins.Contains(admin))
                {
                    BugsChannel.SendMessage("@" + admin + "** is already an admin.**");
                }
                else
                {
                    AddAdmin(admin);
                    BugsChannel.SendMessage("@" + admin + "** was added to the admin list.**");
                }
            }
            else
            {
                BugsChannel.SendMessage(admin + "** was not found in the server.**");
            }
        }

        private void RemoveAdminCommand(string admin)
        {
            if (Admins.Contains(admin))
            {
                RemoveAdmin(admin);
                BugsChannel.SendMessage(admin + "** was removed from admins.**");
            }
            else
            {
                BugsChannel.SendMessage(admin + "** is not an admin.**");
            }
        }

        private void AddAdmin(string admin)
        {
            Admins.Add(admin);
            SaveAdminFile();
        }

        private void RemoveAdmin(string admin)
        {
            Admins.Remove(admin);
            SaveAdminFile();
        }

        private void SaveAdminFile()
        {
            string adminString = string.Join(",", Admins.ToArray());
            
            if (adminString.StartsWith(","))
            {
                adminString = adminString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "admins.list", adminString);
        }
        
        #endregion

        #region Buglist Related Methods

        private void ShowBugListCommand()
        {
            BugsChannel.SendMessage("**Showing current bug list **(" + DateTime.Today.ToShortDateString() + ")** :**");

            string bugList = "```";

            for (int i = 0; i < Bugs.Count; i++)
            {
                bugList += ("(" + i + ") -> " + Bugs[i] + " \n");
            }

            BugsChannel.SendMessage(bugList.Remove(bugList.Length - 3) + " ```");
        }

        private void AddBugCommand(string bug, string user)
        {
            AddBug(bug, user);

            BugsChannel.SendMessage("**Bug added to the list. ** Use *!buglist* to check the current bug list.");
        }

        private void RemoveBugCommand(int id)
        {
            if (Bugs.Count > id)
            {
                BugsChannel.SendMessage("**Bug with ID ** " + id + " ** removed from the list.** " + Bugs[id]);
                RemoveBug(id);
            }
            else
            {
                BugsChannel.SendMessage("**Bug with ID ** " + id + " ** was not found.**");
            }
        }

        private void AddBug(string bug, string user)
        {
            Bugs.Add(bug + " | " + user + " | " + DateTime.Now);
            SaveBugFile();
        }

        private void RemoveBug(int id)
        {
            Bugs.RemoveAt(id);
            SaveBugFile();
        }

        private void SaveBugFile()
        {
            string bugString = string.Join(",", Bugs.ToArray());

            if (bugString.StartsWith(","))
            {
                bugString = bugString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "bugs.list", bugString);
        }

        #endregion
    }
}
