using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Server
{
    public class GameRoom
    {
        public string Name { get; }
        public ClientObject? Player1 { get; }
        public ClientObject? Player2 { get; private set; }
        public bool IsFull => Player2 != null;
        private int[] board = new int[9];
        private ClientObject? currentPlayer;

        public GameRoom(string name, ClientObject creator)
        {
            Name = name;
            Player1 = creator;
            currentPlayer = Player1;
        }

        public bool TryJoin(ClientObject player)
        {
            if (IsFull) return false;
            Player2 = player;
            return true;
        }

        public async Task StartGame()
        { 
            board = new int[9];
            currentPlayer = Player1;
            await Player1!.SendMessageAsync($"GAME_START|X|{Player2!.Nickname}");
            await Player2!.SendMessageAsync($"GAME_START|O|{Player1!.Nickname}");
        }

        public async Task HandleMoveAsync(ClientObject player, int index)
        {
            if (player != currentPlayer || board[index] != 0) return;
            int symbolId = (player == Player1) ? 1 : 2;
            string symbolStr = (player == Player1) ? "X" : "O";
            board[index] = symbolId;
            await Broadcast($"UPDATE|{index}|{symbolStr}");

            if (CheckWin(symbolId))
            {
                await Broadcast($"WIN|{player.Nickname}");
                await EndGame();
            }
            else if (board.All(x => x != 0))
            {
                await Broadcast("DRAW");
                await EndGame();
            }
            else
            { 
                currentPlayer = (currentPlayer == Player1) ? Player2 : Player1;
                string nextTurnNick = currentPlayer!.Nickname;
                await Broadcast($"TURN|{nextTurnNick}");
            }
        }

        public async Task PlayerDisconnected(ClientObject client)
        { 
            var other = (client == Player1) ? Player2 : Player1;
            if (other != null)
            { 
                await other.SendMessageAsync("OPPONENT_LEFT");
                other.CurrentRoom = null;
            }
        }

        private bool CheckWin(int symbolId)
        {
            int[,] wins = new int[,]
            {
                { 0, 1, 2 }, 
                { 3, 4, 5 }, 
                { 6, 7, 8 }, 
                { 0, 3, 6 }, 
                { 1, 4, 7 }, 
                { 2, 5, 8 }, 
                { 0, 4, 8 }, 
                { 2, 4, 6 }  
            };
            int rows = wins.GetLength(0);
            for (int i = 0; i < rows; i++)
            {
                int a = wins[i, 0];
                int b = wins[i, 1];
                int c = wins[i, 2];

                if (board[a] == symbolId && board[b] == symbolId && board[c] == symbolId)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task EndGame()
        {
            Player1.CurrentRoom = null;
            if(Player2 != null) Player2.CurrentRoom = null;
        }

        private async Task Broadcast(string msg)
        {
            if(Player1 != null) await Player1.SendMessageAsync(msg);
            if(Player2 != null) await Player2.SendMessageAsync(msg);
        }
    }
}
