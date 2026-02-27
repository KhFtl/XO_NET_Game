using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    public class PlayerStats
    {
        public string Nickname { get; set; } = "";
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int TotalGames => Wins + Losses + Draws;
        public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;
    }

    public static class LeaderboardManager
    {
        private const string LeaderboardFile = "leaderboard.json";
        private static Dictionary<string, PlayerStats> stats = new Dictionary<string, PlayerStats>();
        private static readonly object lockObject = new object();

        static LeaderboardManager()
        {
            LoadLeaderboard();
        }

        private static void LoadLeaderboard()
        {
            try
            {
                if (File.Exists(LeaderboardFile))
                {
                    string json = File.ReadAllText(LeaderboardFile);
                    var loadedStats = JsonSerializer.Deserialize<Dictionary<string, PlayerStats>>(json);
                    if (loadedStats != null)
                    {
                        stats = loadedStats;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Завантажено статистику для {stats.Count} гравців");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка завантаження таблиці лідерів: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void SaveLeaderboard()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(stats, options);
                File.WriteAllText(LeaderboardFile, json);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка збереження таблиці лідерів: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static Task RecordGame(string winner, string loser, bool isWin)
        {
            return Task.Run(() =>
            {
                lock (lockObject)
                {
                    if (!stats.ContainsKey(winner))
                    {
                        stats[winner] = new PlayerStats { Nickname = winner };
                    }
                    
                    if (isWin)
                    {
                        stats[winner].Wins++;
                    }
                    else
                    {
                        stats[winner].Losses++;
                    }
                    
                    SaveLeaderboard();
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Статистика оновлена: {winner} - Перемог: {stats[winner].Wins}, Поразок: {stats[winner].Losses}");
                    Console.ResetColor();
                }
            });
        }

        public static Task RecordDraw(string nickname)
        {
            return Task.Run(() =>
            {
                lock (lockObject)
                {
                    if (!stats.ContainsKey(nickname))
                    {
                        stats[nickname] = new PlayerStats { Nickname = nickname };
                    }
                    
                    stats[nickname].Draws++;
                    SaveLeaderboard();
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Нічия зафіксована для {nickname}");
                    Console.ResetColor();
                }
            });
        }

        public static string GetLeaderboard()
        {
            lock (lockObject)
            {
                var topPlayers = stats.Values
                    .Where(p => p.TotalGames > 0)
                    .OrderByDescending(p => p.Wins)
                    .ThenByDescending(p => p.WinRate)
                    .Take(10)
                    .ToList();

                if (topPlayers.Count == 0)
                {
                    return "LEADERBOARD|Таблиця лідерів порожня";
                }

                var leaderboardLines = new List<string>();
                for (int i = 0; i < topPlayers.Count; i++)
                {
                    var player = topPlayers[i];
                    leaderboardLines.Add($"{i + 1}|{player.Nickname}|{player.Wins}|{player.Losses}|{player.Draws}|{player.WinRate:F1}%");
                }

                return "LEADERBOARD|" + string.Join(";", leaderboardLines);
            }
        }

        public static PlayerStats? GetPlayerStats(string nickname)
        {
            lock (lockObject)
            {
                return stats.ContainsKey(nickname) ? stats[nickname] : null;
            }
        }
    }
}
