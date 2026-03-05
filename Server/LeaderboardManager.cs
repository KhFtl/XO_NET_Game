using System.Text.Json;

namespace Server
{
    public class PlayerStats
    {
        public string Nickname { get; set; } = "";
        public int Wins { get; set; }
    }

    public static class LeaderboardManager
    {
        private const string FilePath = "leaderboard.json";
        private static Dictionary<string, int> stats = new();

        static LeaderboardManager()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                stats = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new();
            }
        }

        public static void AddWin(string nickname)
        {
            if (stats.ContainsKey(nickname)) stats[nickname]++;
            else stats[nickname] = 1;
            Save();
        }

        private static void Save() => File.WriteAllText(FilePath, JsonSerializer.Serialize(stats));

        public static string GetLeaderboardString()
        {
            var top = stats.OrderByDescending(x => x.Value).Take(10);
            return string.Join(",", top.Select(x => $"{x.Key}: {x.Value}"));
        }
    }
}