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

        private Role DeveloperRole;
        private Role AdministratorRole;
        private Role ModeratorRole;
        private Role VeteranRole;

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
                await Client.Connect("Bot MjA4MTkyMjA0NTM4MjQ5MjE3.CnuFmQ.JChooQG0LBg9d4ZTrTEE85bdZjM");

                await Task.Delay(1000);

                Client.SetGame("Modstone");

                Server = Client.Servers.First(s => s.Id == ServerID);

                DeveloperRole = Server.FindRoles("Developers").FirstOrDefault();
                AdministratorRole = Server.FindRoles("Administrators").FirstOrDefault();
                ModeratorRole = Server.FindRoles("Moderators").FirstOrDefault();
                VeteranRole = Server.FindRoles("Veterans").FirstOrDefault();

                LogText("Loaded Track-o-Tron bot to server " + Server.Name);
            });
        }

        private void LoadFiles()
        {
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
            Channel channel = args.Channel;
            User user = args.User;
            string fullUser = user.ToString();

            if (args.Message.IsAuthor == false)
            {
                if (args.Server?.Id == ServerID)
                {
                    string fullText = args.Message.Text;

                    if (fullText.StartsWith("!"))
                    {
                        string[] commands = fullText.Split();
                        bool isDeveloper = user.HasRole(DeveloperRole);
                        bool isAdmin = isDeveloper || user.HasRole(AdministratorRole);
                        bool isModerator = isDeveloper || isAdmin || user.HasRole(ModeratorRole);
                        bool isVeteran = isDeveloper || isAdmin || isModerator || user.HasRole(VeteranRole);

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
                                if (isModerator)
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    channel.SendMessage("`Latency : " + new Ping().Send("www.discordapp.com").RoundtripTime + " ms`");
                                }
                                break;

                            case "!help":
                                if (commands.Length == 1)
                                {
                                    channel.SendMessage("Use `!help track` to get the full list of Track-o-Tron commands");
                                }
                                else if (commands[1].ToLower() == "track")
                                {
                                    LogNormalCommand(channel, commands[0], fullUser);
                                    channel.SendMessage("**· Normal Commands :**\n " +
                                                        "```!hello - HELLO! (admin+ only)\n" +
                                                        "!ping - Checks bot status (mod+ only)\n" +
                                                        "!help - Shows this message\n" +
                                                        "!clean <quantity> - Cleans x messages from chat (admin only)```\n" +

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
