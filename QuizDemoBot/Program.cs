using Microsoft.Extensions.Configuration;
using QuizDemoBot.Models;
using System.Collections.ObjectModel;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizBotDemo
{
    public class Program
    {
        static bool isQuiz = false;
        static QuizService quizService;
        static string subject = "";


        private static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("D:\\KOMRON\\Najot_ta'lim\\dot_net\\homework\\4-oy\\QuizBotApp\\QuizDemoBotApp\\config\\appsettings.json").Build();

            string TOKEN = configuration["TelegramToken"];
            var botClient = new TelegramBotClient(TOKEN);
            using CancellationTokenSource cts = new CancellationTokenSource();
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            var me = await botClient.GetMeAsync();
            
            await Console.Out.WriteLineAsync($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();


        }
        
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(botClient, update.Message, cancellationToken),
                UpdateType.Unknown => throw new NotImplementedException(),
                UpdateType.InlineQuery => throw new NotImplementedException(),
                UpdateType.ChosenInlineResult => throw new NotImplementedException(),
                UpdateType.CallbackQuery => HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken),
                UpdateType.EditedMessage => throw new NotImplementedException(),
                UpdateType.ChannelPost => throw new NotImplementedException(),
                UpdateType.EditedChannelPost => throw new NotImplementedException(),
                UpdateType.ShippingQuery => throw new NotImplementedException(),
                UpdateType.PreCheckoutQuery => throw new NotImplementedException(),
                UpdateType.Poll => HandlePollAsync(botClient, update.Poll, cancellationToken),
                UpdateType.PollAnswer => HandlePollAnswerAsync(botClient, update.PollAnswer, cancellationToken),
                UpdateType.MyChatMember => throw new NotImplementedException(),
                UpdateType.ChatMember => throw new NotImplementedException(),
                UpdateType.ChatJoinRequest => throw new NotImplementedException(),
            };
            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }


        }


        private static async Task HandleMessageAsync(ITelegramBotClient botClient, Message updateMessage, CancellationToken cancellationToken)
        {
            if (updateMessage is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            if (messageText == "/start")
            {
                KeyboardButton btn = KeyboardButton.WithRequestContact("Kontack ulashish");
                var contact_keyboard = new KeyboardButton("Lokatsiya jo'natish") {
                        RequestLocation = true,
                };
                KeyboardButton poll = KeyboardButton.WithRequestPoll("Poll jo'nating");
                
                var reply = new ReplyKeyboardMarkup(new[]
                {
                    btn, contact_keyboard, poll
                });
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Assalomu alaykum <b>{message.From.FirstName} {message.From.LastName}</b>\n" +
                    $"<a href =\"https://rb.gy/7zewl\">uzb flag</a>",
                    parseMode: ParseMode.Html,
                    replyMarkup:reply,
                    cancellationToken: cancellationToken
                );

            }
            else if (messageText == "/quiz")
            {
                isQuiz = true;
                quizService = new QuizService();
                var subjects = quizService.GetSubjects();
                var keyboardMarkup = new InlineKeyboardMarkup(GetInlineKeyboard(subjects));

                await botClient.SendTextMessageAsync(
                    chatId,
                    text: "Fanni tanlang",
                    replyMarkup: keyboardMarkup);
            }
            else if(messageText =="/next")
            {
                chatId = message.Chat.Id;
                if(subject==null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Siz fanni tanlamadingiz. Fanni tanlash uchun /quiz buyrug'ini tanlang",
                        cancellationToken: cancellationToken);
                }
                else
                {
                    try
                    {
                        await SendQuizAsync(botClient, message.From, cancellationToken);

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else if(messageText=="/stop")
            {
                subject = "";
                await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text:"test to'xtatildi",
                        cancellationToken: cancellationToken);
                isQuiz = false;
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "You send:\n" +
                    $"{messageText}",
                    replyMarkup:null,
                    cancellationToken: cancellationToken);
                await Console.Out.WriteLineAsync($"from {message.From.FirstName} received: {messageText}");
            }


        }

        private static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery? callbackQuery, CancellationToken cancellationToken)
        {
            if (isQuiz)
            {
                
                subject = callbackQuery.Data;
                try
                {
                    await SendQuizAsync(botClient, callbackQuery.From, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        private static async Task SendQuizAsync(ITelegramBotClient botClient, User from, CancellationToken cancellationToken)
        {
            var question = quizService.GetQuestionBySubject(subject);
            await Console.Out.WriteLineAsync((char)(0 + question.AnswerKey.ToCharArray()[0]));
            try
            {
                IEnumerable<MessageEntity>? entities = new Collection<MessageEntity>()
                    {
                        new MessageEntity()
                        {
                            Type = MessageEntityType.Bold,
                            Length = 0,
                            Offset = 0,
                            User = new User()
                            {
                                Id = from.Id,
                                FirstName = from.FirstName,

                            }
                        },
                    };
                Message poll = await botClient.SendPollAsync(
                    chatId: from.Id,
                    question: question.question,
                    options: question.options,
                    correctOptionId: question.answerkey,
                    //correctOptionId:,
                    isAnonymous: true,
                    type: PollType.Quiz,
                    explanationEntities:entities,
                 
                    explanationParseMode: ParseMode.Markdown,
                    openPeriod:30,
                    cancellationToken: cancellationToken
                    );

            }
            catch ( Exception ex )
            {
                await Console.Out.WriteLineAsync(ex.Message);
                await SendQuizAsync(botClient, from, cancellationToken);
            }
        }

        private static InlineKeyboardButton[][] GetInlineKeyboard(IList<string> subjects)
        {
            int len = subjects.Count;
            var keyboardInline = new InlineKeyboardButton[len][];
            for (int i = 0; i < len; i++)
            {
            var keyboardButtons = new InlineKeyboardButton[1];
                keyboardButtons[0] = InlineKeyboardButton.WithCallbackData(subjects[i]);
                   
                keyboardInline[i] = keyboardButtons;
            }
            return keyboardInline;
        }
        
        private static async Task HandlePollAsync(ITelegramBotClient botClient, Poll? poll, CancellationToken cancellationToken)
        {
            

        }
        private static async Task HandlePollAnswerAsync(ITelegramBotClient botClient, PollAnswer? pollAnswer, CancellationToken cancellationToken)
        {
            await SendQuizAsync(botClient, pollAnswer.User, cancellationToken);
            /*await botClient.SendTextMessageAsync(
                chatId: pollAnswer.User.Id,
                text: "javob qabul qilindi",
                cancellationToken:cancellationToken);
            //await botClient.StopPollAsync(chatId: pollAnswer.User.Id, messageId:Poll.MessageId);*/

        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }


    }
}