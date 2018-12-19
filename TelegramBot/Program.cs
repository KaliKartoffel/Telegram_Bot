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

namespace Awesome
{
    class Program
    {
        static ITelegramBotClient botClient;
        static IDictionary<long, string> userNames = new Dictionary<long, string>();

        static void Main()
        {
            botClient = new TelegramBotClient("726459976:AAGb5mHnQbFSOtf6mFCVhfBW4t4MuuFsStg");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            
            var lines = File.ReadAllLines("..\\..\\..\\Files\\userNames.txt");
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
            string[] commands = new string[] {"/test", "/name"};
            var message = e.Message;
            if (message.Text == null || message.Type != MessageType.Text) throw new System.ArgumentException("message is either 0 or != Text"); 

            Console.WriteLine($"Received a text message in chat {message.Chat.Id} with the Text \"{message.Text}\" from {message.Chat.FirstName}.");
            switch (message.Text.Split(" ").First())
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
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"Your Name is {userNames[message.Chat.Id]}"
                        );
                        break;
                    }
                    userNames[message.Chat.Id] = splitMsg[1];
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"Your new Name is {splitMsg[1]}"
                    );
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