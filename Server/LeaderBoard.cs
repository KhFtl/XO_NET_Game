using System.Text.Json;

namespace Server
{
    public class PlayerStats
    {
        public string Nick { get; set; } = "";
        public int Games { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }

    public class Leaderboard
    {
        private readonly object _lock = new();
        private readonly string _filePath;
        private readonly Dictionary<string, PlayerStats> _stats = new(StringComparer.OrdinalIgnoreCase);

        public Leaderboard(string filePath = "leaderboard.json")
        {
            _filePath = filePath;
            Load();
        }

        public void Load()
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath)) return;

                try
                {
                    var json = File.ReadAllText(_filePath);
                    var list = JsonSerializer.Deserialize<List<PlayerStats>>(json) ?? new List<PlayerStats>();
                    _stats.Clear();

                    foreach (var s in list)
                    {
                        if (!string.IsNullOrWhiteSpace(s.Nick))
                            _stats[s.Nick] = s;
                    }
                }
                catch
                {
                    _stats.Clear();
                }
            }
        }

        private void Save()
        {
            lock (_lock)
            {
                var list = _stats.Values
                    .OrderByDescending(x => x.Wins)
                    .ThenByDescending(x => x.Games)
                    .ThenBy(x => x.Nick)
                    .ToList();

                var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }

        private PlayerStats GetOrCreate_NoLock(string nick)
        {
            if (!_stats.TryGetValue(nick, out var s))
            {
                s = new PlayerStats { Nick = nick };
                _stats[nick] = s;
            }
            return s;
        }
        public void EnsurePlayer(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return;

            lock (_lock)
            {
                GetOrCreate_NoLock(nick);
                Save();
            }
        }

        public void RecWin(string winner, string loser)
        {
            if (string.IsNullOrWhiteSpace(winner) || string.IsNullOrWhiteSpace(loser)) return;

            lock (_lock)
            {
                var w = GetOrCreate_NoLock(winner);
                var l = GetOrCreate_NoLock(loser);

                w.Games++; w.Wins++;
                l.Games++; l.Losses++;

                Save();
            }
        }

        public void RecDraw(string p1, string p2)
        {
            if (string.IsNullOrWhiteSpace(p1) || string.IsNullOrWhiteSpace(p2)) return;

            lock (_lock)
            {
                var a = GetOrCreate_NoLock(p1);
                var b = GetOrCreate_NoLock(p2);

                a.Games++; a.Draws++;
                b.Games++; b.Draws++;

                Save();
            }
        }

        public string GetTop(int top = 10)
        {
            lock (_lock)
            {
                var list = _stats.Values
                    .OrderByDescending(x => x.Wins)
                    .ThenByDescending(x => x.Games)
                    .ThenBy(x => x.Nick)
                    .Take(top)
                    .ToList();

                return string.Join(",", list.Select(s => $"{s.Nick}:{s.Wins}:{s.Losses}:{s.Draws}:{s.Games}"));
            }
        }
    }
}