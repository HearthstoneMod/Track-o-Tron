using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Discord;

namespace Track_o_Tron
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

        private const ulong ServerID = 194099156988461056;

        private DiscordClient Client;
        private Server Server;

        private string AppDirectory;

        private List<string> Admins = new List<string>(); 
        private List<string> Bugs = new List<string>(); 
        private List<string> Todos = new List<string>(); 
        private List<string> Ideas = new List<string>();

        public void Start()
        {
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

            LoadFiles();

            Client = new DiscordClient();

            Client.MessageReceived += async (obj, args) =>
            {
                await Task.Run(() => ProcessMessage(args));
            };

            Client.ExecuteAndWait(async () =>
            {
                await Client.Connect("MjA4MTkyMjA0NTM4MjQ5MjE3.CnuFmQ.JChooQG0LBg9d4ZTrTEE85bdZjM");

                await Task.Delay(1000);

                Client.SetGame("Modstone");

                Server = Client.Servers.First(s => s.Id == ServerID);
                
                LogText("Loaded Track-o-Tron bot to server " + Server.Name);
            });
        }

        private void LoadFiles()
        {
            if (File.Exists(AppDirectory + "admins.list"))
            {
                string[] admins = File.ReadAllText(AppDirectory + "admins.list").Split(new string[1] {";"}, StringSplitOptions.RemoveEmptyEntries);

                foreach (string admin in admins)
                {
                    Admins.Add(admin);
                }

                LogText("Loaded " + admins.Length + " admins");
            }
            else
            {
                File.Create(AppDirectory + "admins.list").Close();

                LogText("Created empty admin list");
            }

            if (File.Exists(AppDirectory + "bugs.list"))
            {
                string[] bugs = File.ReadAllText(AppDirectory + "bugs.list").Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string bug in bugs)
                {
                    Bugs.Add(bug);
                }

                LogText("Loaded " + bugs.Length + " bugs");
            }
            else
            {
                File.Create(AppDirectory + "bugs.list").Close();

                LogText("Created empty bug list");
            }
            
            if (File.Exists(AppDirectory + "todos.list"))
            {
                string[] todos = File.ReadAllText(AppDirectory + "todos.list").Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string todo in todos)
                {
                    Todos.Add(todo);
                }

                LogText("Loaded " + todos.Length + " to-dos");
            }
            else
            {
                File.Create(AppDirectory + "todos.list").Close();

                LogText("Created empty todo list");
            }
            
            if (File.Exists(AppDirectory + "ideas.list"))
            {
                string[] ideas = File.ReadAllText(AppDirectory + "ideas.list").Split(new string[1] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string idea in ideas)
                {
                    Ideas.Add(idea);
                }

                LogText("Loaded " + ideas.Length + " ideas");
            }
            else
            {
                File.Create(AppDirectory + "ideas.list").Close();

                LogText("Created empty ideas list");
            }

            LogText(" ");
        }

        private void ProcessMessage(MessageEventArgs args)
        {
            if (args.Message.IsAuthor == false)
            {
                if (args.Server?.Id == ServerID)
                {
                    string fullText = args.Message.Text;

                    if (fullText.StartsWith("!"))
                    {
                        string[] commands = fullText.Split();
                        string fullUser = args.User.ToString();
                        bool isAdmin = Admins.Contains(fullUser);
                        Channel channel = args.Channel;

                        switch (commands[0].ToLower())
                        {
                            case "!hello":
                                if (isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    channel.SendTTSMessage("***HELLO! HELLO! HELLO!***");
                                }
                                break;

                            case "!ping":
                                LogNormalCommand(channel, commands[0], fullUser);
                                channel.SendMessage("`Latency : " + new Ping().Send("www.discordapp.com").RoundtripTime + " ms`");
                                break;

                            case "!help":
                                if (commands.Length == 1)
                                {
                                    channel.SendMessage("Use `!help track` to get the full list of Track-o-Tron commands");
                                }
                                else if (commands[1] == "track")
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    channel.SendMessage("**· Normal Commands :**\n " +
                                                        "```!hello - HELLO! (admin only)\n" +
                                                        "!ping - Checks bot status and latency\n" +
                                                        "!help - Shows this message\n" +
                                                        "!clean <quantity> - Cleans x messages from chat (admin only)```\n" +

                                                        "**· Admin Commands: **\n" +
                                                        "```!addadmin <fullname> - Adds an admin to the admin list (admin only)\n" +
                                                        "!removeadmin <fullname> - Removes an admin from the admin list (admin only)\n" +
                                                        "!adminlist - Shows the full list of admins```\n" +

                                                        "**· TO-DO Commands: **\n" +
                                                        "```!todo <text> - Adds a to-do to the to-do list\n" +
                                                        "!removetodo <id> - Removes a to-do from the to-do list (admin only)\n" +
                                                        "!todolist - Shows the full list of to-dos\n" +
                                                        "!cleartodolist - Clears the list of to-dos (admin only)```\n" +

                                                        "**· Bug Commands: **\n" +
                                                        "```!bug <text> - Adds a bug to the bug list\n" +
                                                        "!removebug <id> - Removes a bug from the bug list (admin only)\n" +
                                                        "!buglist - Shows the full list of bugs\n" +
                                                        "!clearbuglist - Clears the list of bugs (admin only)```\n" +

                                                        "**· Idea Commands: **\n" +
                                                        "```!idea <text> - Adds an idea to the idea list\n" +
                                                        "!removeidea <id> - Removes an idea from the idea list (admin only)\n" +
                                                        "!idealist - Shows the full list of ideas\n" +
                                                        "!clearidealist - Clears the list of ideas (admin only)```\n");
                                }
                                break;

                            case "!clean":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    CleanCommand(channel, int.Parse(commands[1]));
                                }
                                break;

                            case "!addadmin":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    AddAdminCommand(channel, commands[1]);
                                }
                                break;

                            case "!removeadmin":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    RemoveAdminCommand(channel, commands[1]);
                                }
                                break;

                            case "!adminlist":
                                LogNormalCommand(channel, commands[0], fullUser);
                                ShowAdminListCommand(channel);
                                break;

                            case "!bug":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    AddBugCommand(channel, fullText.Substring(commands[0].Length + 1), fullUser);
                                }
                                break;

                            case "!removebug":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    RemoveBugCommand(channel, int.Parse(commands[1]));
                                }
                                break;

                            case "!buglist":
                                LogNormalCommand(channel, commands[0], fullUser);
                                ShowBugListCommand(channel);
                                break;

                            case "!clearbuglist":
                                if (isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    ClearBugListCommand(channel);
                                }
                                break;

                            case "!todo":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    AddTodoCommand(channel, fullText.Substring(commands[0].Length + 1), fullUser);
                                }
                                break;

                            case "!removetodo":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    RemoveTodoCommand(channel, int.Parse(commands[1]));
                                }
                                break;

                            case "!todolist":
                                LogNormalCommand(channel, commands[0], fullUser);
                                ShowTodoListCommand(channel);
                                break;

                            case "!cleartodolist":
                                if (isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    ClearTodoListCommand(channel);
                                }
                                break;

                            case "!idea":
                                if (commands.Length > 1)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    AddIdeaCommand(channel, fullText.Substring(commands[0].Length + 1), fullUser);
                                }
                                break;

                            case "!removeidea":
                                if (commands.Length > 1 && isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    RemoveIdeaCommand(channel, int.Parse(commands[1]));
                                }
                                break;

                            case "!idealist":
                                LogNormalCommand(channel, commands[0], fullUser);
                                ShowIdeaListCommand(channel);
                                break;

                            case "!clearidealist":
                                if (isAdmin)
                                {
                                    LogAdminCommand(channel, commands[0], fullUser);
                                    ClearIdeaListCommand(channel);
                                }
                                break;
                        }
                    }
                }
            }
        }

        #region Admin Related Methods

        private void ShowAdminListCommand(Channel channel)
        {
            if (Admins.Count > 0)
            {
                string adminList = "**Showing current admin list **(" + DateTime.Today.ToShortDateString() + ")** :**\n\n```";

                for (int i = 0; i < Admins.Count; i++)
                {
                    adminList += "· " + Admins[i] + "\n";
                }

                channel.SendMessage(adminList + "```");
            }
            else
            {
                channel.SendMessage("**Admin list is empty.**");
            }
        }

        private void AddAdminCommand(Channel channel, string admin)
        {
            if (Server.Users.Any(u => u.ToString() == admin))
            {
                if (Admins.Contains(admin))
                {
                    channel.SendMessage("@" + admin + "** is already an admin.**");
                }
                else
                {
                    AddAdmin(admin);
                    channel.SendMessage("@" + admin + "** was added to the admin list.**");
                }
            }
            else
            {
                channel.SendMessage(admin + "** was not found in the server.**");
            }
        }

        private void RemoveAdminCommand(Channel channel, string admin)
        {
            if (Admins.Contains(admin))
            {
                RemoveAdmin(admin);
                channel.SendMessage(admin + "** was removed from admins.**");
            }
            else
            {
                channel.SendMessage(admin + "** is not an admin.**");
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
            string adminString = string.Join(";", Admins.ToArray());
            
            if (adminString.StartsWith(";"))
            {
                adminString = adminString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "admins.list", adminString);
        }
        
        #endregion

        #region Buglist Related Methods

        private void ShowBugListCommand(Channel channel)
        {
            if (Bugs.Count > 0)
            {
                channel.SendMessage("**Showing current bug list **(" + DateTime.Today.ToShortDateString() + ")** :**");

                string bugList = "```";

                for (int i = 0; i < Bugs.Count; i++)
                {
                    bugList += ("(" + i + ") -> " + Bugs[i] + " \n");
                }

                channel.SendMessage(bugList.Remove(bugList.Length - 3) + " ```");
            }
            else
            {
                channel.SendMessage("**Bug list is empty.**");
            }
        }

        private void ClearBugListCommand(Channel channel)
        {
            Bugs.Clear();
            SaveBugFile();

            channel.SendMessage("**Bug list has been cleared.**");
        }

        private void AddBugCommand(Channel channel, string bug, string user)
        {
            AddBug(bug, user);

            channel.SendMessage("**Bug added to the list. ** Use *!buglist* to check the current bug list.");
        }

        private void RemoveBugCommand(Channel channel, int id)
        {
            if (Bugs.Count > id)
            {
                channel.SendMessage("**Bug with ID ** " + id + " ** removed from the list.** " + Bugs[id]);
                RemoveBug(id);
            }
            else
            {
                channel.SendMessage("**Bug with ID ** " + id + " ** was not found.**");
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
            string bugString = string.Join(";", Bugs.ToArray());

            if (bugString.StartsWith(";"))
            {
                bugString = bugString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "bugs.list", bugString);
        }

        #endregion

        #region Todolist Related Methods

        private void ShowTodoListCommand(Channel channel)
        {
            if (Todos.Count > 0)
            {
                string todoList = "```";

                for (int i = 0; i < Todos.Count; i++)
                {
                    todoList += ("(" + i + ") -> " + Todos[i] + " \n");
                }

                channel.SendMessage("**Showing current to-do list **(" + DateTime.Today.ToShortDateString() + ")** :**\n" + todoList + "```");
            }
            else
            {
                channel.SendMessage("**To-do list is empty.**");
            }
        }

        private void ClearTodoListCommand(Channel channel)
        {
            Todos.Clear();
            SaveTodoFile();

            channel.SendMessage("**To-do list has been cleared.**");
        }

        private void AddTodoCommand(Channel channel, string todo, string user)
        {
            AddTodo(todo, user);

            channel.SendMessage("**To-do added to the list. ** Use *!todolist* to check the current to-do list.");
        }

        private void RemoveTodoCommand(Channel channel, int id)
        {
            if (Todos.Count > id)
            {
                channel.SendMessage("**To-do with ID ** " + id + " ** removed from the list.** " + Todos[id]);
                RemoveTodo(id);
            }
            else
            {
                channel.SendMessage("**To-do with ID ** " + id + " ** was not found.**");
            }
        }

        private void AddTodo(string todo, string user)
        {
            Todos.Add(todo + " | " + user + " | " + DateTime.Now);
            SaveTodoFile();
        }

        private void RemoveTodo(int id)
        {
            Todos.RemoveAt(id);
            SaveTodoFile();
        }

        private void SaveTodoFile()
        {
            string todoString = string.Join(";", Todos.ToArray());

            if (todoString.StartsWith(";"))
            {
                todoString = todoString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "todos.list", todoString);
        }

        #endregion

        #region Idealist Related Methods

        private void ShowIdeaListCommand(Channel channel)
        {
            if (Ideas.Count > 0)
            {
                channel.SendMessage("**Showing current idea list **(" + DateTime.Today.ToShortDateString() + ")** :**");

                string ideaList = "```";

                for (int i = 0; i < Ideas.Count; i++)
                {
                    ideaList += ("(" + i + ") -> " + Ideas[i] + " \n");
                }

                channel.SendMessage(ideaList.Remove(ideaList.Length - 3) + " ```");
            }
            else
            {
                channel.SendMessage("**Idea list is empty.**");
            }
        }

        private void ClearIdeaListCommand(Channel channel)
        {
            Ideas.Clear();
            SaveIdeaFile();

            channel.SendMessage("**Idea list has been cleared.**");
        }

        private void AddIdeaCommand(Channel channel, string idea, string user)
        {
            AddIdea(idea, user);

            channel.SendMessage("**Idea added to the list. ** Use *!idealist* to check the current idea list.");
        }

        private void RemoveIdeaCommand(Channel channel, int id)
        {
            if (Ideas.Count > id)
            {
                channel.SendMessage("**Idea with ID ** " + id + " ** removed from the list.** " + Ideas[id]);
                RemoveIdea(id);
            }
            else
            {
                channel.SendMessage("**Idea with ID ** " + id + " ** was not found.**");
            }
        }

        private void AddIdea(string idea, string user)
        {
            Ideas.Add(idea + " | " + user + " | " + DateTime.Now);
            SaveIdeaFile();
        }

        private void RemoveIdea(int id)
        {
            Ideas.RemoveAt(id);
            SaveIdeaFile();
        }

        private void SaveIdeaFile()
        {
            string ideaString = string.Join(";", Ideas.ToArray());

            if (ideaString.StartsWith(";"))
            {
                ideaString = ideaString.Substring(1);
            }

            File.WriteAllText(AppDirectory + "ideas.list", ideaString);
        }

        #endregion

        #region Utility Related Methods

        public async void CleanCommand(Channel channel, int quantity)
        {
            Message[] removeMessages = await channel.DownloadMessages(quantity + 1);

            await channel.DeleteMessages(removeMessages);
        }

        #endregion

        #region Log Methods

        public void LogText(string text)
        {
            Console.WriteLine("<white>" + text + "</white>");
        }

        public void LogNormalCommand(Channel channel, string cmd, string user)
        {
            Console.WriteLine("<cyan>" + cmd + " requested in #" + channel.Name + " by " + user + "</cyan>");
        }

        public void LogAdminCommand(Channel channel, string cmd, string user)
        {
            Console.WriteLine("<green>" + cmd + " requested in #" + channel.Name + " by " + user + "</green>");
        }

        #endregion
    }
}
