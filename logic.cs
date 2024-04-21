using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.FileIO;
using PRTelegramBot.Attributes;
using PRTelegramBot.Models;
using PRTelegramBot.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TelegramBotTest1
{
    
    public static class logic
    {
        public static SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-Q85L5BK\IKOSQL;Initial Catalog=schediule;Integrated Security=True;TrustServerCertificate=True");
        public static string group = "";
        public static int day = 0;
        public static int UpOrDown = 1;
        public static DateTime Maindate = new DateTime(DateTime.Now.Year, DateTime.Now.Month,DateTime.Now.Day);
        public static DateTime Changedate = Maindate;


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
            if (Convert.ToInt32(Changedate.DayOfWeek) != day-1)
            {
               // Changedate.AddDays(4);
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
        
        public static string GetValues(string query, SqlConnection sqlConnection)
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
        }

    }
}
