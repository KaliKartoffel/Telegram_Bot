using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Awesome
{
    class Program
    {
        //ServicePointManager.DefaultConnectionLimit = 1000; //TODO Doesnt work, need it for many async requests in parallel: https://stackoverflow.com/questions/2960056/trying-to-run-multiple-http-requests-in-parallel-but-being-limited-by-windows
        static ITelegramBotClient botClient;
        static IDictionary<long, string> userNames = new Dictionary<long, string>();
        static IDictionary<long, List<ushort>> scores = new Dictionary<long, List<ushort>>();
        static IList<string> quotes = new List<string>();
        static string[] words = File.ReadAllLines("..\\..\\..\\Files\\words.txt"); //Don't know if array or list is smarter

        static string[] splitMsg;

        static void Main()
        {
            botClient = new TelegramBotClient("YOUR ACCES TOKEN");
            string[] lines;
            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
            #region databank lookup
            /*string[] lines = File.ReadAllLines("..\\..\\..\\Files\\words.txt");
            foreach (string line in lines)
            {
                words.Add(line);
            } */
            lines = File.ReadAllLines("..\\..\\..\\Files\\userNames.txt");
            foreach (string line in lines)
            {
                string[] keyValuePair = line.Split(":");
                userNames.Add(Convert.ToInt64(keyValuePair[0]), keyValuePair[1]);
            }
            lines = File.ReadAllLines("..\\..\\..\\Files\\quotes.txt");
            foreach (string line in lines)
            {
                quotes.Add(line);
            }
            lines = File.ReadAllLines("..\\..\\..\\Files\\scores.txt");
            foreach (string line in lines)
            {
                string[] arguments = line.Split(",");
                scores[Convert.ToInt64(arguments[0])] = new List<ushort> { Convert.ToUInt16(arguments[1]), Convert.ToUInt16(arguments[2]) };
            }
            #endregion
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            ConsoleInput();
        }

        static void ConsoleInput()
        {
            string[] commands = new string[] { "stop" };
            for (; ; )
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "stop":
                        using (StreamWriter file = new StreamWriter("..\\..\\..\\Files\\userNames.txt"))
                        {
                            foreach (var entry in userNames)
                            {
                                file.WriteLine("{0}:{1}", entry.Key, entry.Value);
                            }
                        }
                        using (StreamWriter file = new StreamWriter("..\\..\\..\\Files\\quotes.txt"))
                        {
                            foreach (var entry in quotes)
                            {
                                file.WriteLine(entry);
                            }
                        }
                        using (StreamWriter file = new StreamWriter("..\\..\\..\\Files\\scores.txt"))
                        {
                            foreach (var entry in userNames)
                            {
                                file.WriteLine("{0},{1},{2}", entry.Key, entry.Value[0], entry.Value[1]);
                            }
                        }
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("I don't get what you are saying. Available commands: " + String.Join(", ", commands));
                        break;
                }
            }
        }
        
        static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            string[] commands = new string[] { "/test", "/name", "/zufallstext", "/klugeworte", "/coin" };
            Telegram.Bot.Types.Message message = e.Message;
            Random rand = new Random();
            if (message.Text == null || message.Type != MessageType.Text)
            {
                throw new ArgumentException("message should be either 0 or != Text");
            }
            Console.WriteLine($"Received a text message in chat {message.Chat.Id} with the Text \"{message.Text}\" from {message.Chat.FirstName}.");
            switch (message.Text.Split(" ").First().ToLower())
            {
                case "help":
                    goto help;

                case "/help":
                    help:
                    Send_Message($"available commands: {String.Join(", ", commands)}\nUse the next line for optional input and the rest of the same line seperated by a whitspace for other options.", e);
                    break; 

                case "/test":
                    Send_Message($"Testing if I work?", e);
                    break;

                case "/name":
                    splitMsg = message.Text.Split(" ", 2);
                    if (splitMsg.Length == 1)
                    {
                        try
                        {
                            Send_Message($"Your Name is {userNames[message.Chat.Id]}", e);
                        }
                        catch
                        {
                            Send_Message($"Define your name first please.", e);
                        }
                        break;
                    }
                    userNames[message.Chat.Id] = splitMsg[1];
                    Send_Message($"Your new Name is {splitMsg[1]}", e);
                    break;

                case "/zufallstext":
                    try
                    {
                        IList<string> wordList = new List<string>();
                        int max = Convert.ToInt32(message.Text.Split(" ", 2).Last());
                        for (int i = 0; i < max; i++)
                        {
                            wordList.Add(words[rand.Next(0, words.Length)]);
                        }
                        if (wordList.Count < 300)
                        {
                            Send_Message($"Your \"sentence\":\n" + String.Join(" ", wordList), e);
                        }
                        else
                        {
                            Send_Message($"Your \"sentence\":\n", e);
                            for (int i = 0; i < Math.Ceiling(wordList.Count / 300.0); i++)
                            {
                                string str = "";
                                for (int j = i * 300; j < wordList.Count && j < 300 + i * 300; j++)
                                {
                                    str += wordList[j] + " ";
                                }
                                Send_Message(str, e);
                                Thread.Sleep(5000);
                            }
                        }
                        Console.WriteLine("Sended message");
                    }
                    catch (Exception a)
                    {
                        Console.WriteLine(Convert.ToString(a));
                        Send_Message($"Please specify the Amount of Words you want, separated by a space.", e);
                    }
                    break;

                case "/klugeworte":
                    if (message.Text.ToLower().Contains("add"))
                    {
                        splitMsg = message.Text.Split("\n");
                        try
                        {
                            for (int i = 1; i < splitMsg.Length; i++)
                            {
                                quotes.Add($"{splitMsg[i]} submitted by {message.Chat.FirstName} {message.Chat.LastName}");
                                Send_Message($"Added your Quote {splitMsg[i]}", e);
                            }
                        }
                        catch (Exception)
                        {
                            Send_Message($"Please add your quote in the following format: /klugeworte add and in the next line the quote with autor. U can add multiple lines with a quote each.", e);
                        }
                    } else
                    {
                        Send_Message(quotes[rand.Next(0, quotes.Count)], e);
                    }
                    break;

                case "/coin":
                    string sendMessage;
                    List<ushort> headsTails = scores[message.Chat.Id];

                    if (rand.Next(0,2) == 0)
                    {
                        sendMessage = "Heads";
                        headsTails[0] += 1;
                    } else
                    {
                        sendMessage = "Tails";
                        headsTails[1] += 1;
                    }
                    Send_Message(sendMessage, e);
                    scores[message.Chat.Id] = headsTails;
                    if (message.Text.Contains("stats"))
                    {
                        Send_Message($"Heads: {Convert.ToString(scores[message.Chat.Id][0])}, Tails: {Convert.ToString(scores[message.Chat.Id][1])}", e);
                    }
                    break;

                default:
                    if (message.Text[0] != Convert.ToChar("/"))
                    {
                        return;
                    }
                    Send_Message($"I don't get what you are saying. PLease use one of the available commands: {String.Join(", ", commands)}.\nLeave a whitspace like Space or newLine after command.", e);
                    break;
            }
        }
        static void Send_Message(string text, MessageEventArgs e)
        {
            botClient.SendTextMessageAsync(
                chatId: e.Message.Chat,
                text: text
            );
            Console.WriteLine($"Sended Message \"{text}\" to {e.Message.Chat.Id} on order {e.Message.Text}.");
        }
    }
}