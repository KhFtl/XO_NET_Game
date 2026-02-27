using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static ConcurrentDictionary<Guid, ClientObject> clients = new ConcurrentDictionary<Guid, ClientObject>();
        static ConcurrentDictionary<string, GameRoom> rooms = new ConcurrentDictionary<string, GameRoom>();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.Title = "Сервер гри Хрестики - Нолики";

            var config = ServerConfig.LoadOrAsk();
            if (config == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Помилка в файлі конфігурації :( видаліть файл та спробуйте знову");
                Console.ResetColor();
                return;
            }

            TcpListener listener = new TcpListener(IPAddress.Parse(config.IpAdress), config.Port);
            listener.Start();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Сервер запущено на {config.IpAdress}:{config.Port}");
            Console.ResetColor();

            _ = Task.Run(() => ConsoleCommandLoop());

            while (true)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var client = new ClientObject(tcpClient);

                clients.TryAdd(client.Id, client);
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private static void ConsoleCommandLoop()
        {
            PrintConsoleHelp();

            while (true)
            {
                string? cmd = Console.ReadLine();
                if (cmd == null) continue;

                cmd = cmd.Trim();

                if (cmd.Equals("/help", StringComparison.OrdinalIgnoreCase))
                {
                    PrintConsoleHelp();
                }
                else if (cmd.Equals("/list", StringComparison.OrdinalIgnoreCase))
                {
                    PrintPlayersToConsole();
                }
                else if (cmd.Equals("/list_room", StringComparison.OrdinalIgnoreCase))
                {
                    PrintRoomsToConsole();
                }
                else if (cmd.Equals("/clear", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    PrintConsoleHelp();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Невідома команда. Напиши /help");
                    Console.ResetColor();
                }
            }
        }

        private static void PrintConsoleHelp()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Команди сервера:");
            Console.ResetColor();

            Console.WriteLine("  /help       - показати команди");
            Console.WriteLine("  /list       - показати всіх активних гравців");
            Console.WriteLine("  /list_room  - показати всі кімнати + гравців");
            Console.WriteLine("  /clear      - очистити консоль");
            Console.WriteLine();
        }

        private static void PrintPlayersToConsole()
        {
            var players = clients.Values
                .Select(c =>
                {
                    string nick = string.IsNullOrWhiteSpace(c.Nickname) ? c.Id.ToString() : c.Nickname!;
                    string room = c.CurrentRoom?.Name ?? "лобі";
                    return $"{nick} (де: {room})";
                })
                .OrderBy(x => x)
                .ToList();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Активні гравці ===");
            Console.ResetColor();

            if (players.Count == 0)
            {
                Console.WriteLine("(немає підключених)");
                return;
            }

            foreach (var p in players)
                Console.WriteLine(p);

            Console.WriteLine();
        }

        private static void PrintRoomsToConsole()
        {
            var list = rooms.Values
                .OrderBy(r => r.Name)
                .Select(r =>
                {
                    string p1 = r.Player1?.Nickname ?? "—";
                    string p2 = r.Player2?.Nickname ?? "—";
                    string status = r.IsFull ? "FULL" : "OPEN";
                    return $"{r.Name} [{p1} vs {p2}] ({status})";
                })
                .ToList();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=== Кімнати ===");
            Console.ResetColor();

            if (list.Count == 0)
            {
                Console.WriteLine("(кімнат немає)");
                return;
            }

            foreach (var item in list)
                Console.WriteLine(item);

            Console.WriteLine();
        }

        private static async Task HandleClientAsync(ClientObject client)
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (client.Client.Connected)
                {
                    int bytesRead = await client.Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[{client.Nickname ?? client.Id.ToString()}] -> {message}");

                    await ProcessMessageAsync(client, message);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка клієнта: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                await ClientDisconnected(client);
            }
        }

        private static async Task ClientDisconnected(ClientObject client)
        {
            clients.TryRemove(client.Id, out _);

            if (client.CurrentRoom != null)
            {
                await client.CurrentRoom.PlayerDisconnected(client);
                rooms.TryRemove(client.CurrentRoom.Name, out _);
                await BroadcastLobbyList();
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Клієнт {client.Nickname ?? client.Id.ToString()} відключився.");
            Console.ResetColor();
        }

        private static async Task BroadcastLobbyList()
        {
            foreach (var c in clients.Values.Where(x => x.CurrentRoom == null))
            {
                await SendLobbyList(c);
            }
        }

        private static async Task ProcessMessageAsync(ClientObject client, string message)
        {
            string trimmed = message.Trim();

            if (trimmed.Equals("/list", StringComparison.OrdinalIgnoreCase))
            {
                await SendPlayersList(client);
                return;
            }
            if (trimmed.Equals("/list_room", StringComparison.OrdinalIgnoreCase))
            {
                await SendRoomsList(client);
                return;
            }

            var parts = message.Split('|');
            string command = parts[0];

            switch (command)
            {
                case "LOGIN":
                    bool firstLogin = string.IsNullOrWhiteSpace(client.Nickname);
                    client.Nickname = parts.Length > 1 ? parts[1] : client.Nickname;

                    if (firstLogin)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Гравець підключився: {client.Nickname ?? client.Id.ToString()}");
                        Console.ResetColor();
                    }

                    await SendLobbyList(client);
                    break;

                case "CREATE_ROOM":
                    if (parts.Length < 2) return;

                    string roomName = parts[1];

                    if (rooms.ContainsKey(roomName))
                    {
                        await client.SendMessageAsync("ERROR|Кімната з такою назвою вже існує");
                        return;
                    }

                    var newRoom = new GameRoom(roomName, client);
                    rooms.TryAdd(roomName, newRoom);
                    client.CurrentRoom = newRoom;

                    await BroadcastLobbyList();
                    await client.SendMessageAsync($"ROOM_CREATED|{roomName}");
                    break;

                case "JOIN_ROOM":
                    if (parts.Length < 2) return;

                    string targetRoom = parts[1];

                    if (rooms.TryGetValue(targetRoom, out var room))
                    {
                        if (room.TryJoin(client))
                        {
                            client.CurrentRoom = room;
                            await BroadcastLobbyList();
                            await room.StartGame();

                            Console.WriteLine($"Гравець {client.Nickname ?? client.Id.ToString()} приєднався до кімнати {room.Name}");
                        }
                        else
                        {
                            await client.SendMessageAsync("ERROR|Кімната повна");
                        }
                    }
                    break;

                case "MOVE":
                    if (client.CurrentRoom != null && parts.Length >= 2)
                    {
                        if (int.TryParse(parts[1], out int index))
                        {
                            await client.CurrentRoom.HandleMoveAsync(client, index);
                        }
                    }
                    break;

                case "REFRESH_LOBBY":
                    await SendLobbyList(client);
                    break;
            }
        }

        private static async Task SendLobbyList(ClientObject client)
        {
            var openRooms = rooms.Values.Where(r => !r.IsFull).Select(r => r.Name);
            string list = string.Join(",", openRooms);
            await client.SendMessageAsync($"LOBBY_LIST|{list}");
        }

        private static async Task SendPlayersList(ClientObject client)
        {
            var players = clients.Values
                .Select(c => string.IsNullOrWhiteSpace(c.Nickname) ? c.Id.ToString() : c.Nickname!)
                .OrderBy(x => x)
                .ToList();

            string payload = players.Count == 0 ? "(порожньо)" : string.Join(", ", players);
            await client.SendMessageAsync($"PLAYERS_LIST|{payload}");
        }

        private static async Task SendRoomsList(ClientObject client)
        {
            var list = rooms.Values
                .OrderBy(r => r.Name)
                .Select(r =>
                {
                    string p1 = r.Player1?.Nickname ?? "—";
                    string p2 = r.Player2?.Nickname ?? "—";
                    return $"{r.Name} [{p1} vs {p2}]";
                })
                .ToList();

            string payload = list.Count == 0 ? "(кімнат немає)" : string.Join("; ", list);
            await client.SendMessageAsync($"ROOMS_LIST|{payload}");
        }
    }
}
