using Microsoft.Data.SqlClient;
using PRTelegramBot.Attributes;
using PRTelegramBot.Models;
using PRTelegramBot.Utils;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotTest1
{
    
    public static class logic
    {
        private static string group = "";
        private static int day = 0;
        private static int UpOrDown = 1;
        private static DateTime Maindate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,DateTime.Now.Day);
        private static DateTime Changedate = Maindate;


        [SlashHandler("/SetGroup", "/Вернуться к выбору группы","/start")]
        public static async Task Group(ITelegramBotClient botClient, Update update)
        {
            var message = "Выберите группу";

            var menuList = new List<KeyboardButton>();
            var nemuListString = new List<string>();

            menuList.Add(new KeyboardButton("ИСИТ-211"));
            menuList.Add(new KeyboardButton("ИСИТ-212"));
            menuList.Add(new KeyboardButton("ИСИТ-22"));

            var menu = MenuGenerator.ReplyKeyboard(2, menuList);

            var option = new OptionMessage();
            option.MenuReplyKeyboardMarkup = menu;

            var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, message, option);
        }
        [ReplyMenuHandler("ИСИТ-211", "ИСИТ-212", "ИСИТ-22")]
        public static async Task SetGroup(ITelegramBotClient botClient, Update update)
        {
            string set = update.Message.Text.ToString();
            group = set;

            var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, $"Выбрана группа {group}");

            Day(botClient, update);
        }


        [SlashHandler("/Day")]
        public static async Task Day(ITelegramBotClient botClient, Update update)
        {
            var message = "Выберите день недели";

            var menuList = new List<KeyboardButton>();
            var nemuListString =new List<string>();

            menuList.Add(new KeyboardButton("Понедельник"));
            menuList.Add(new KeyboardButton("Вторник"));
            menuList.Add(new KeyboardButton("Среда"));
            menuList.Add(new KeyboardButton("Пятница"));
            menuList.Add(new KeyboardButton("/Вернуться к выбору группы"));

            var menu = MenuGenerator.ReplyKeyboard(2, menuList);

            var option = new OptionMessage();
            option.MenuReplyKeyboardMarkup = menu;

            var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, message, option);
        }
        [ReplyMenuHandler("Понедельник", "Вторник", "Среда", "Пятница")]
        public static async Task SetDay(ITelegramBotClient botClient, Update update)
        {
            string set = Convert.ToString(update.Message.Text);
            DayOfWeek dayOfWeek = DayOfWeek.Monday;

            switch (set)
            {
                case "Понедельник": day = 1; break;
                case "Вторник": day = 2; dayOfWeek = DayOfWeek.Tuesday; break;
                case "Среда": day = 3; dayOfWeek = DayOfWeek.Wednesday; break;
                case "Пятница": day = 5; dayOfWeek = DayOfWeek.Friday; break;
            }

            Changedate = Maindate.AddDays((int)dayOfWeek - (int)Maindate.DayOfWeek);
            var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, $"Выбран день: {set} - {(Changedate.ToShortDateString())}");
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar cal = ci.Calendar;
            CalendarWeekRule rule = ci.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = ci.DateTimeFormat.FirstDayOfWeek;
            int week = cal.GetWeekOfYear(Changedate, rule, firstDayOfWeek);

            if (week % 2 == 0)
            {
                UpOrDown = 2;
            }
            else
            {
                UpOrDown = 1;
            }

            Test(botClient, update);                      
        }


        
        public static async Task Test(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(exePath, "TRY_VEIW.csv");
            string csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);

            Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
            StreamReader reader = new StreamReader(csvPath, Encoding.GetEncoding(1251));

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                string key = values[0];
                List<string> listValues = new List<string>();

                for (int i = 1; i < values.Length; i++)
                {
                    listValues.Add(values[i]);
                }

                data.Add(key, listValues);

            }

            List<string[]> foundValue = FindValueForKeyMatchingWord(group, day, UpOrDown, data);


            if (foundValue != null)
            {
                string sendRes= GetValues(foundValue);  
                
                var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, sendRes);
            }           
            else
            {
                var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, $"Начните с выбора группы");
            }
            
        }
        static List<string[]> FindValueForKeyMatchingWord(string group,int day, int UpDown, Dictionary<string, List<string>> datar)
        {
            List<string[]> ret = new List<string[]>();
            ret.Add(new string[1]);
            int count = 0;

            foreach (var pair in datar)
            {
                if (pair.Value[0] == group && pair.Value[1] == day.ToString() && Convert.ToInt32(pair.Value[2]) != UpDown)
                {
                    count++;

                    ret.Add(pair.Value.ToArray());
                }
                
            }
            if (count == 0)
            {
                return null;
            }
            else
            {
                var sortedList = ret.OrderBy(arr => Convert.ToDateTime(arr[arr.Length-1])).ToList();

                return sortedList;
            }
        }

        private static string GetValues(List<string[]> query1)
        {

            List<string[]> data = new List<string[]>();
            // ----------------------------- Group

            string[] Title = query1[1];
            data.Add(new string[1]);

            string UpDown = "Нижняя неделя";
            if (UpOrDown == 1)
            {
                UpDown = "Верхняя неделя";
            }
            data[data.Count - 1][0] = @$"👩🏻‍🤝‍👨🏼{Title[0].ToString()} - {UpDown}";

            // ----------------------------- DATA
            bool first = true;
            foreach(string[] query in query1)
            {
                if (first)
                {
                    first= false;
                    continue;
                }

                data.Add(new string[1]);
                data[data.Count - 1][0] = @$"🧩{query[7].ToString()} - {query[6].ToString()}
🕔{query[12].ToString()} - {query[13].ToString()}
👨‍🏫{query[8].ToString()} {query[9].ToString()} {query[10].ToString()}     
🏘️{query[3].ToString()} - {query[4].ToString()} {query[5].ToString()}";
            }

            string sendBack = "";

            foreach (string[] s in data)
            {
                foreach (string g in s)
                {
                    sendBack += g;
                }
                sendBack += "\n\n";
            }

            return sendBack;
        }
        /*public static async Task Test(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            sqlConnection.Open();
            try
            {
                
                string query = $"use schediule select * from VIEWSCHEDULE where [№_group] = '{group}' and Day_of_week= {day} and Up_down !={UpOrDown}";
                string comand = GetValues(query, sqlConnection);
                var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, comand);
            }
            catch (Exception ex)
            {
                var sendMessage = await PRTelegramBot.Helpers.Message.Send(botClient, update, $"Начните с выбора группы");
            }
            
            sqlConnection.Close();
        }

        private static string GetValues(string query, SqlConnection sqlConnection)
        {
            SqlCommand command = new SqlCommand(query, sqlConnection);
            SqlDataReader dataReaderStart = command.ExecuteReader();
            query = null;
            List<string[]> data = new List<string[]>();
            // ----------------------------- Group

            dataReaderStart.Read();
            data.Add(new string[1]);
            string UpDown = "Нижняя неделя";
            if (UpOrDown == 1)
            {
                UpDown = "Верхняя неделя";
            }
            data[data.Count - 1][0] = @$"👩🏻‍🤝‍👨🏼{dataReaderStart[1].ToString()} - {UpDown}";
            dataReaderStart.Close();
            // ----------------------------- DATA
            SqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                data.Add(new string[1]);
                data[data.Count - 1][0] = @$"🧩{dataReader[7].ToString()} - {dataReader[6].ToString()}
🕔{dataReader[12].ToString()} - {dataReader[13].ToString()}
👨‍🏫{dataReader[8].ToString()} {dataReader[9].ToString()} {dataReader[10].ToString()}     
🏘️{dataReader[4].ToString()} - {dataReader[5].ToString()}";
            }
            dataReader.Close();
            foreach (string[] s in data)
            {
                foreach (string g in s)
                {
                    query += g;
                }
                query += "\n\n";
            }
            
            return query;
        }*/

    }
}
