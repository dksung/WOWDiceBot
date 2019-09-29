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
using Telegram.Bot.Types;

namespace WowDiceBot
{
    public partial class Form1 : Form
    {
        // int : ID
        // Tuple : 주사위값, UserName
        private Dictionary<int, Tuple<int, string>> keyValuePairs = new Dictionary<int, Tuple<int, string>>();

        private List<long> rankList = new List<long>();

        private TelegramBotClient bot;
        string token = string.Empty;

        public Form1()
        {
            InitializeComponent();
            //@wow_dice_bot : 965378885:AAECg689Kuz-j819Yz84H4ZJu636ruXPu9s
            //@clienDice_bot : 915917363:AAFNXIaWNGJE3nq2f0vUyeTk1vL1F516RUg;

            token = @"965378885:AAECg689Kuz-j819Yz84H4ZJu636ruXPu9s";
            //token = @"915917363:AAFNXIaWNGJE3nq2f0vUyeTk1vL1F516RUg";

            bot = new TelegramBotClient(token);
        }

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
                bool startCheck = false;
                if (rankList.Contains(message.Chat.Id) == true)
                    startCheck = true;

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
                    if (splitedText.Count() > 1 && startCheck == false)
                    {
                        if (int.TryParse(splitedText[1], out maxValue) == false)
                        {
                            maxValue = 100;
                        }
                    }

                    returnValue = random.Next(1, maxValue);
                    string username = GetDisplayname(message.From);

                    formatedText = string.Format(@"{0} 이(가) 주사위를 굴려 {1}이 나왔습니다.(1-{2})", username, returnValue, maxValue);
                    if (startCheck == true)
                    {
                        keyValuePairs.Add(message.From.Id, new Tuple<int, string>(returnValue, username));
                    }
                    await bot.SendTextMessageAsync(message.Chat.Id, formatedText);
                }
            }
            else if (inputText.StartsWith("/시작"))
            {
                if(rankList.Contains(message.Chat.Id) == false)
                {
                    rankList.Add(message.Chat.Id);
                    keyValuePairs.Clear();
                    await bot.SendTextMessageAsync(message.Chat.Id, "지금부터 굴리는 주사위 값에 순위를 매깁니다.\n주사위는 한 번 씩만 굴릴 수 있습니다.\n주사위 값은 1과 100 사이의 값으로 강제합니다.");
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "결과 값을 먼저 확인 후 다시 시작해주세요.");
                }
            }
            else if (inputText.StartsWith("/결과"))
            {
                if (rankList.Contains(message.Chat.Id) == true)
                {
                    if (keyValuePairs.Count() == 0)
                    {
                        await bot.SendTextMessageAsync(message.Chat.Id, "아무도 주사위를 굴리지 않았습니다\n시작을 한 뒤에 주사위를 굴려주세요");
                        return;
                    }
                    rankList.Remove(message.Chat.Id);

                    
                    await bot.SendTextMessageAsync(message.Chat.Id, GetResultValue());
                }
                else
                {
                    await bot.SendTextMessageAsync(message.Chat.Id, "시작을 하지 않았습니다.");
                }
            }
            else if (inputText.StartsWith("/로또"))
            {
                int generateCount = 1;
                if (splitedText.Count() > 1)
                {
                    if (int.TryParse(splitedText[1], out generateCount) == false)
                    {
                        generateCount = 1;
                    }
                }
                
                await bot.SendTextMessageAsync(message.Chat.Id, GetDisplayname(message.From) + " 님의 로또 예상 번호는\n" + GetLottoNumbers(generateCount) + " 입니다.");
            }
            else if (inputText.StartsWith("/도움말"))
            {
                // 도움말 출력
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(@"현재 지원 기능은 아래와 같습니다.");
                sb.AppendLine(@"/로또 : 랜덤한 로또 번호 6자리 생성");
                sb.AppendLine(@"/주사위 : 1부터 100사이의 랜덤한 값 생성");
                sb.AppendLine(@"/시작 : 주사위 순위 기능");
                sb.AppendLine(@"/결과 : 주사위 순위 보기");

                await bot.SendTextMessageAsync(message.Chat.Id, sb.ToString());
            }
        }

        private string GetLottoNumbers(int count)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                var set = new HashSet<int>();

                while (set.Count() < 6)
                {
                    Random random = new Random();
                    set.Add(random.Next(1, 46));
                }
                sb.AppendLine(string.Join(", ", set.OrderBy(t => t).Select(n => n.ToString())));
            }

            return sb.ToString();
        }

        private string GetResultValue()
        {
            var orderedItem = keyValuePairs.OrderByDescending(t => t.Value.Item1);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"********** 순위 **********");
            
            int rank = 1;
            foreach (var item in orderedItem)
            {
                sb.AppendLine(string.Format($"{rank++}위 : \t\t {item.Value.Item2} \t\t {item.Value.Item1}"));
            }

            sb.AppendLine(@"************************");
            keyValuePairs.Clear();
            return sb.ToString();
        }

        private string GetDisplayname(User user)
        {
            string username = string.Format(@"@{0}", user.Username);

            if (string.IsNullOrEmpty(user.Username))
            {
                username = string.Format(@"{0}{1}", user.LastName, user.FirstName);
            }

            return username;
        }
    }
}
