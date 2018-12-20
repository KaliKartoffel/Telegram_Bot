using System;
using System.Threading;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Awesome
{
    class Program
    {
        //ServicePointManager.DefaultConnectionLimit = 1000; //TODO Doesnt work, need it for many async requests in parallel: https://stackoverflow.com/questions/2960056/trying-to-run-multiple-http-requests-in-parallel-but-being-limited-by-windows
        static ITelegramBotClient botClient;
        static IDictionary<long, string> userNames = new Dictionary<long, string>();
        static IList<string> words = new List<string>();

        static void Main()
        {
            botClient = new TelegramBotClient("726459976:AAGb5mHnQbFSOtf6mFCVhfBW4t4MuuFsStg");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            var lines = File.ReadAllLines("..\\..\\..\\Files\\words.txt");
            foreach (string line in lines)
            {
                words.Add(line);
            }

            lines = File.ReadAllLines("..\\..\\..\\Files\\userNames.txt");
            foreach (string line in lines)
            {
                string[] keyValue = line.Split(":");
                userNames.Add(Convert.ToInt64(keyValue[0]), keyValue[1]);
            }

            botClient.OnMessage += Bot_OnMessage;

            botClient.StartReceiving();
            ConsoleInput();
        }

        static void ConsoleInput()
        {
            string[] commands = new string[] { "stop" };
            for (;;)
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "stop":
                        using (StreamWriter file = new StreamWriter("..\\..\\..\\Files\\userNames.txt"))
                            foreach (var entry in userNames)
                                file.WriteLine("{0}:{1}", entry.Key, entry.Value);
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("I don't get what you are saying. Available commands: " + String.Join(", ", commands));
                        break;
                }
            }
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            string[] commands = new string[] {"/test", "/name", "/zufallstext" };
            var message = e.Message;
            if (message.Text == null || message.Type != MessageType.Text) throw new System.ArgumentException("message is either 0 or != Text"); 

            Console.WriteLine($"Received a text message in chat {message.Chat.Id} with the Text \"{message.Text}\" from {message.Chat.FirstName}.");
            switch (message.Text.Split(" ").First().ToLower())
            {
                case "/test":
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"Testing if I work?"
                    );
                    break;
                case "/name":
                    string[] splitMsg = message.Text.Split(" ", 2);
                    if (splitMsg.Length == 1)
                    {
                        try
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Your Name is {userNames[message.Chat.Id]}"
                            );
                        } catch {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Define your name first please."
                            );
                        }
                        
                        break;
                    }
                    userNames[message.Chat.Id] = splitMsg[1];
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"Your new Name is {splitMsg[1]}"
                    );
                    break;
                case "/zufallstext":
                    try
                    {
                        IList<string> wordList = new List<string>();
                        Random rand = new Random();
                        int max = Convert.ToInt32(message.Text.Split(" ", 2).Last());
                        for (int i = 0; i < max; i++)
                        {
                            wordList.Add(words[rand.Next(0, words.Count)]);
                        }
                        if (wordList.Count < 300)
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Your \"sentence\":\n" + String.Join(" ", wordList)
                            );
                        } else {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat,
                                text: $"Your \"sentence\":\n"
                            );
                            for (int i = 0; i < Math.Ceiling(wordList.Count/300.0); i++)
                            {
                                string str = "";
                                for (int j = i*300; j < wordList.Count && j < 300+i*300; j++)
                                {
                                    str += wordList[j] + " ";
                                }
                                await botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: str
                                );
                                Thread.Sleep(5000);
                            }
                        }
                        Console.WriteLine("Sended message");
                    }
                    catch (Exception a)
                    {
                        Console.WriteLine(Convert.ToString(a));
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"Please specify the Amount of Words you want, separated by a space."
                        );
                    } 
                    break;
                default:
                    if (message.Text[0] != Convert.ToChar("/")) return;
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"I don't get what you are saying. PLease use one of the available commands: {String.Join(", ",commands)}.\nLeave a whitspace like Space or newLine after command."
                    );
                    break;
            }
        }
    }
}