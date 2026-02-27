using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class GameRoom
    {
        public string Name { get; }
        public ClientObject Player1 { get; private set; }
        public ClientObject? Player2 { get; private set; }
        public bool IsFull => Player2 != null;
        private string[] board = new string[9];
        private string currentTurnSymbol = "X";
        private bool gameActive = false;

        public GameRoom(string name, ClientObject creator)
        {
            Name = name;
            Player1 = creator;
            for (int i = 0; i < 9; i++)
            {
                board[i] = "";
            }
        }

        public bool TryJoin(ClientObject client)
        {
            if (Player2 != null) return false;
            Player2 = client;
            return true;
        }

        public async Task StartGame()
        {
            if (Player1 == null || Player2 == null) return;
            
            gameActive = true;
            await Player1.SendMessageAsync($"GAME_START|X|{Player2.Nickname}");
            await Player2.SendMessageAsync($"GAME_START|O|{Player1.Nickname}");
            
            await NotifyTurn();
        }

        private async Task NotifyTurn()
        {
            var currentPlayer = currentTurnSymbol == "X" ? Player1 : Player2;
            await Player1.SendMessageAsync($"TURN|{currentPlayer.Nickname}");
            await Player2.SendMessageAsync($"TURN|{currentPlayer.Nickname}");
        }

        public async Task HandleMoveAsync(ClientObject client, int index)
        {
            if (!gameActive) return;
            
            string playerSymbol = (client == Player1) ? "X" : "O";
            
            if (playerSymbol != currentTurnSymbol)
            {
                await client.SendMessageAsync("ERROR|Зараз не ваш хід!");
                return;
            }

            if (index < 0 || index >= 9 || !string.IsNullOrEmpty(board[index]))
            {
                await client.SendMessageAsync("ERROR|Неправильний хід!");
                return;
            }

            board[index] = playerSymbol;
            
            await Player1.SendMessageAsync($"UPDATE|{index}|{playerSymbol}");
            await Player2.SendMessageAsync($"UPDATE|{index}|{playerSymbol}");

            if (CheckWin(playerSymbol))
            {
                await Player1.SendMessageAsync($"WIN|{client.Nickname}");
                await Player2.SendMessageAsync($"WIN|{client.Nickname}");
                gameActive = false;
                
                // Оновлюємо статистику
                await LeaderboardManager.RecordGame(client.Nickname, client == Player1 ? Player2.Nickname : Player1.Nickname, true);
                await LeaderboardManager.RecordGame(client == Player1 ? Player2.Nickname : Player1.Nickname, client.Nickname, false);
                return;
            }

            if (CheckDraw())
            {
                await Player1.SendMessageAsync("DRAW|");
                await Player2.SendMessageAsync("DRAW|");
                gameActive = false;
                
                // Записуємо нічию
                await LeaderboardManager.RecordDraw(Player1.Nickname);
                await LeaderboardManager.RecordDraw(Player2.Nickname);
                return;
            }

            currentTurnSymbol = (currentTurnSymbol == "X") ? "O" : "X";
            await NotifyTurn();
        }

        private bool CheckWin(string symbol)
        {
            int[][] winPatterns = new int[][]
            {
                new int[] {0, 1, 2}, new int[] {3, 4, 5}, new int[] {6, 7, 8},
                new int[] {0, 3, 6}, new int[] {1, 4, 7}, new int[] {2, 5, 8},
                new int[] {0, 4, 8}, new int[] {2, 4, 6}
            };

            foreach (var pattern in winPatterns)
            {
                if (board[pattern[0]] == symbol && 
                    board[pattern[1]] == symbol && 
                    board[pattern[2]] == symbol)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckDraw()
        {
            return board.All(cell => !string.IsNullOrEmpty(cell));
        }

        public async Task PlayerDisconnected(ClientObject client)
        {
            if (!gameActive) return;
            
            var otherPlayer = (client == Player1) ? Player2 : Player1;
            if (otherPlayer != null)
            {
                await otherPlayer.SendMessageAsync("OPPONENT_LEFT|");
            }
            gameActive = false;
        }

        public bool IsGameActive()
        {
            return gameActive;
        }
    }
}
