using System.Text.Json;

namespace Server
{
    public class ServerConfig
    {
        public string IpAdress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8848;
        private const string _configFile = "serverconfig.json";
        public static ServerConfig? LoadOrAsk()
        {
            if (File.Exists(_configFile))
            {
                try
                {
                    string json = File.ReadAllText(_configFile);
                    var config = JsonSerializer.Deserialize<ServerConfig>(json);
                    return config;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка читання файлу... {ex.Message}");
                }
            }
            var newConfig = new ServerConfig();
            Console.Write("Введіть IP (за замовчуванням 127.0.0.1):");
            string? ipInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ipInput))
            {
                newConfig.IpAdress = ipInput;
            }
            Console.Write("Введіть порт (за замовчуванням 8848):");
            string? portInput = Console.ReadLine();
            int port = 8848;
            if (!string.IsNullOrWhiteSpace(portInput))
            {
                try
                {
                    port = Convert.ToInt32(portInput);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ви ввели невірне значення порт призначено 8888");
                    Console.ResetColor();
                    port = 8888;
                }
            }
            if (port != 0)
            {
                newConfig.Port = port;
            }
            string saveJson = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFile, saveJson);
            Console.WriteLine($"Всі налаштування збережено у файлі {Path.GetFullPath(_configFile)}");
            return newConfig;
        }
    }
}
