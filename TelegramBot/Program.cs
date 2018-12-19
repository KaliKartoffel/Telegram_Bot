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

namespace Awesome
{
    class Program
    {
        static ITelegramBotClient botClient;

        static void Main()
        {
            botClient = new TelegramBotClient("726459976:AAGb5mHnQbFSOtf6mFCVhfBW4t4MuuFsStg");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

            botClient.OnMessage += Bot_OnMessage;

            botClient.StartReceiving();
            for (;;) Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            string[] commands = new string[] {"/test"};
            var message = e.Message;
            if (message.Text == null || message.Type != MessageType.Text) throw new System.ArgumentException("message is either 0 or != Text"); 

            Console.WriteLine($"Received a text message in chat {message.Chat.Id} with the Text {message.Text}.");

            switch (message.Text.Split(" ").First())
            {
                case "/test":
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: $"Testing if I work?"
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