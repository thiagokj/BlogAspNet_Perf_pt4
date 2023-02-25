public static class Configuration
{
    public static string JwtKey = "SuachaveJwt";
    public static string ApiKeyName = "api_key";
    public static string ApiKey = "curso_api_suaChave";
    public static string ApplicationUrl = "https://url:port";
    public static SmtpConfiguration Smtp = new();

    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}