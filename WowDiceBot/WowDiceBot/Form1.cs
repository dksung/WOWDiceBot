using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;

namespace WowDiceBot
{
    public partial class Form1 : Form
    {
        private bool startCheck = false;
        // int : ID
        // Tuple : 주사위값, UserName
        //private Dictionary<int, string[]> keyValuePairs = new Dictionary<int, string[]>();
        private Dictionary<int, Tuple<int, string>> keyValuePairs = new Dictionary<int, Tuple<int, string>>();

        public Form1()
        {
            InitializeComponent();
        }

        //@wow_dice_bot : 965378885:AAECg689Kuz-j819Yz84H4ZJu636ruXPu9s
        private TelegramBotClient bot = new TelegramBotClient("965378885:AAECg689Kuz-j819Yz84H4ZJu636ruXPu9s");

        //테스트용
        //@clienDice_bot : 915917363:AAFNXIaWNGJE3nq2f0vUyeTk1vL1F516RUg;
        //private TelegramBotClient bot = new TelegramBotClient("915917363:AAFNXIaWNGJE3nq2f0vUyeTk1vL1F516RUg");


        private void Form1_Load(object sender, EventArgs e)
        {
            telegramAPIAsync();
            setEvent();
        }

        private async void telegramAPIAsync()
        {
            var me = await bot.GetMeAsync();
        }

        private void setEvent()
        {
            bot.OnMessage += Bot_OnMessage;
            bot.StartReceiving();
        }

        private async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null || message.Type != Telegram.Bot.Types.Enums.MessageType.Text || !message.Text.StartsWith("/"))
            {
                return;
            }

            string inputText = message.Text;
            var splitedText = inputText.Split(' ');
            string formatedText = string.Empty;

            if (inputText.StartsWith("/주사위"))
            {
                bool rollDice = true;

                if (startCheck == true)
                {
                    if (keyValuePairs.ContainsKey(message.From.Id) == true)
                    {
                        var value = keyValuePairs[message.From.Id];
                        await bot.SendTextMessageAsync(message.Chat.Id, $"개인당 주사위를 1번씩만 굴릴 수 있습니다.\n{value.Item2} 의 이전 결과값은 {value.Item1} 입니다.");
                        rollDice = false;
                    }
                }

                if (rollDice == true)
                {
                    Random random = new Random();
                    int returnValue = 0;
                    int maxValue = 100;
                    if (splitedText.Count() > 1)
                    {
                        if (int.TryParse(splitedText[1], out maxValue) == false)
                        {
                            maxValue = 100;
                        }
                    }

                    returnValue = random.Next(1, maxValue);
                    var messageInfo = message.From;
                    //string username = string.Format(@"@{0} ({1}{2})", messageInfo.Username, messageInfo.LastName, messageInfo.FirstName);
                    string username = string.Format(@"@{0}", messageInfo.Username);

                    if (string.IsNullOrEmpty(messageInfo.Username))
                    {
                        username = string.Format(@"{0}{1}", messageInfo.LastName, messageInfo.FirstName);
                    }

                    Console.WriteLine($"LastName : {messageInfo.LastName}");
                    Console.WriteLine($"FirstName : {messageInfo.FirstName}");

                    formatedText = string.Format(@"{0} 이(가) 주사위를 굴려 {1}이 나왔습니다.(1-{2})", username, returnValue, maxValue);
                    if (startCheck == true)
                    {
                        keyValuePairs.Add(messageInfo.Id, new Tuple<int, string>(returnValue, username));
                    }
                    await bot.SendTextMessageAsync(message.Chat.Id, formatedText);
                }
            }
            else if (inputText.StartsWith("/시작"))
            {
                startCheck = true;
                keyValuePairs.Clear();
                await bot.SendTextMessageAsync(message.Chat.Id, "지금부터 굴리는 주사위 값에 순위를 매깁니다.\n주사위는 한 번 씩만 굴릴 수 있습니다.");
            }
            else if (inputText.StartsWith("/결과"))
            {
                startCheck = false;
                if (keyValuePairs.Count() == 0)
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "아무도 주사위를 굴리지 않았습니다\n/시작을 한 뒤에 주사위를 굴려주세요");
                    return;
                }
                var orderedItem = keyValuePairs.OrderByDescending(t => t.Value.Item1);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"******** 결과를 보여드립니다 ********");

                int rank = 1;
                foreach (var item in orderedItem)
                {
                    sb.AppendLine(string.Format($"{rank++}위 : \t {item.Value.Item2} \t {item.Value.Item1}"));
                }
                keyValuePairs.Clear();
                await bot.SendTextMessageAsync(message.Chat.Id, sb.ToString());
            }
            else if (inputText.StartsWith("/도움말"))
            {
                // 도움말 출력
                await bot.SendTextMessageAsync(message.Chat.Id, "도움말 제작 예정");
            }
            else
            {
                await bot.SendTextMessageAsync(message.Chat.Id, inputText + " 는 미지원 명령어");
            }
        }
    }
}
