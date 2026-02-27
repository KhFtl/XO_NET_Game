using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Program
    {
        static ConcurrentDictionary<Guid, ClientObject> clients = new ConcurrentDictionary<Guid, ClientObject>();
        static ConcurrentDictionary<string, GameRoom> rooms = new ConcurrentDictionary<string, GameRoom>();
        static Leaderboard leaderboard = new Leaderboard("leaderboard.json");
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

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
            while (true)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var client = new ClientObject(tcpClient);
                clients.TryAdd(client.Id, client);
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private static async Task HandleClientAsync(ClientObject client)
        {
            try
            {
                byte[] buffer = new byte[1024];
                var sb = new StringBuilder();

                while (client.Client.Connected)
                {
                    int bytesRead = await client.Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    while (true)
                    {
                        string all = sb.ToString();
                        int nl = all.IndexOf('\n');
                        if (nl < 0) break;

                        string oneMsg = all.Substring(0, nl).Trim('\r');
                        sb.Remove(0, nl + 1);

                        if (string.IsNullOrWhiteSpace(oneMsg)) continue;

                        Console.WriteLine($"[{client.Nickname ?? client.Id.ToString()}] -> {oneMsg}");
                        await ProcessMessageAsync(client, oneMsg);
                    }
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
            Console.WriteLine($"Клієнт {client.Nickname} відключився.");
            Console.ResetColor();
        }

        private static async Task BroadcastLobbyList()
        {
            foreach (var client in clients.Values.Where(x => x.CurrentRoom == null))
            {
                await SendLobbyList(client);
            }
        }

        private static async Task ProcessMessageAsync(ClientObject client, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var parts = message.Split('|');
            string command = parts[0];
            switch (command)
            {
                case "LOGIN":
                    if (parts.Length < 2) { await client.SendMessageAsync("ERROR|Bad LOGIN"); break; }
                    client.Nickname = parts[1];
                    leaderboard.EnsurePlayer(client.Nickname);
                    await SendLobbyList(client);
                    await client.SendMessageAsync($"LEADERBOARD|{leaderboard.GetTop(10)}");
                    break;
                case "CREATE_ROOM":
                    string roomName = parts[1];
                    if (rooms.ContainsKey(roomName))
                    {
                        await client.SendMessageAsync("ERROR|Кімната з такою назвою вже існує");
                        return;
                    }
                    var newRoom = new GameRoom(roomName, client);
                    newRoom.OnWin += (winner, loser) => leaderboard.RecWin(winner, loser);
                    newRoom.OnDraw += (player1, player2) => leaderboard.RecDraw(player1, player2);
                    rooms.TryAdd(roomName, newRoom);
                    client.CurrentRoom = newRoom;
                    await BroadcastLobbyList();
                    await client.SendMessageAsync($"ROOM_CREATED|{roomName}");
                    Console.WriteLine($"Кімната {roomName} створена користувачем {client.Nickname}...");
                    break;
                case "JOIN_ROOM":
                    string targetRoom = parts[1];
                    if (rooms.TryGetValue(targetRoom, out var room))
                    {
                        if (room.TryJoin(client))
                        {
                            client.CurrentRoom = room;
                            await BroadcastLobbyList();
                            await room.StartGame();
                        }
                        else
                        {
                            await client.SendMessageAsync("ERROR|Кімната повна");
                        }
                    }
                    break;
                case "MOVE":
                    if (client.CurrentRoom != null)
                    {
                        int index = int.Parse(parts[1]);
                        var currentRoom = client.CurrentRoom;
                        bool finished = await currentRoom.HandleMoveAsync(client, index);
                        if (finished)
                        {
                            rooms.TryRemove(currentRoom.Name, out _);
                            await BroadcastLobbyList();
                            await BroadcastLeaderboard(10);
                        }
                    }
                    break;
                case "REFRESH_LOBBY":
                    await SendLobbyList(client);
                    break;
                case "GET_LEADERBOARD":
                    int top = 10;
                    if (parts.Length > 1)
                        int.TryParse(parts[1], out top);
                    string payload = leaderboard.GetTop(top);
                    await client.SendMessageAsync($"LEADERBOARD|{payload}");
                    break;
            }
        }

        private static async Task SendLobbyList(ClientObject client)
        {
            var openRooms = rooms.Values.Where(r => !r.IsFull).Select(r => r.Name);
            string list = string.Join(",", openRooms);
            await client.SendMessageAsync($"LOBBY_LIST|{list}");
        }

        private static async Task BroadcastLeaderboard(int top = 10)
        {
            string payload = leaderboard.GetTop(top);
            foreach (var client in clients.Values.Where(x => x.CurrentRoom == null))
            {
                await client.SendMessageAsync($"LEADERBOARD|{payload}");
            }
        }
    }
}
