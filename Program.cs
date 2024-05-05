using PRTelegramBot.Core;

const string tokenBot = "7122311834:AAEy1rjfn78M7XQjUt3xUwR96RG0rhFIzTM";
const string EXIT_Comm = "exit";

var telegrtam = new PRBot(option =>
{
    option.Token = tokenBot;
    //option.ClearUpdatesOnStart = true;
    option.WhiteListUsers = new List<long>()
    {

    };
    option.Admins = new List<long>() { };
    option.BotId = 0;
    
});


telegrtam.OnLogError += Telegram_onLogError;
telegrtam.OnLogCommon += Telegrtam_OnLogCommon;

await telegrtam.Start();
void Telegrtam_OnLogCommon(string msg, Enum typeEvent, ConsoleColor color)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    string MSG = $"{DateTime.Now}:{msg}";
    Console.WriteLine(MSG);
    Console.ResetColor();
}

void Telegram_onLogError(Exception e, long? id)
{
    Console.ForegroundColor = ConsoleColor.Red;
    string errorMSG = $"{DateTime.Now}:{e}";
    Console.WriteLine(errorMSG);
    Console.ResetColor();
}


while (true)
{
    var input = Console.ReadLine();
    if (input.ToLower() == EXIT_Comm)
    {
        Environment.Exit(0);
    }
}