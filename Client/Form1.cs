using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Button[] gameButtons = new Button[9];
        private bool isConnected = false;

        private string NickName; // Ігровий нікнейм
        private string mySymbol; // "X" або "O"
        private int pressedIndex = -1; // Індекс останньої натиснутої кнопки
        public Form1()
        {
            InitializeComponent();
            SetupCustomLogic();
        }
        private void SetupCustomLogic()
        {
            gameButtons = new Button[] { button1, button2, button3, button4, button5, button6, button7, button8, button9 };
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            SetupLeaderboardGrid();
        }

        private void SetupLeaderboardGrid()
        {
            dgv_Leaderboard.ReadOnly = true;
            dgv_Leaderboard.AllowUserToAddRows = false;
            dgv_Leaderboard.AllowUserToDeleteRows = false;
            dgv_Leaderboard.AllowUserToResizeRows = false;
            dgv_Leaderboard.RowHeadersVisible = false;
            dgv_Leaderboard.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_Leaderboard.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgv_Leaderboard.Columns.Count == 0)
            {
                dgv_Leaderboard.Columns.Add("Place", "#");
                dgv_Leaderboard.Columns.Add("Nick", "Nick");
                dgv_Leaderboard.Columns.Add("Wins", "W");
                dgv_Leaderboard.Columns.Add("Losses", "L");
                dgv_Leaderboard.Columns.Add("Draws", "D");
                dgv_Leaderboard.Columns.Add("Games", "Games");
            }
        }

        private async Task SendPacket(string msg)
        {
            if (!isConnected) return;
            try
            {
                msg += "\n";
                byte[] data = Encoding.UTF8.GetBytes(msg);
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відправки даних: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (!groupBox2.Enabled)
            { 
                MessageBox.Show("Ви не можете зробити хід зараз!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            Button btn = (Button)sender;
            if (!string.IsNullOrEmpty(btn.Text))
            {
                MessageBox.Show("Ця клітинка вже зайнята оберіть іншу!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int index = Array.IndexOf(gameButtons, btn);
            if (index != -1)
            { 
                await SendPacket($"MOVE|{index}");
            }
        }

        private async void btn_Connect_Click(object sender, EventArgs e)
        {
            if (isConnected) return;
            if (string.IsNullOrWhiteSpace(txt_NickName.Text)) return;
            NickName = txt_NickName.Text.Trim();
            try
            {
                isConnected = true;
                client = new TcpClient();
                await client.ConnectAsync(txt_IpServer.Text, Convert.ToInt32(txt_ServerPort.Text));
                stream = client.GetStream();
                _ = ListenForPackets();
                await SendPacket($"LOGIN|{NickName}");
                await SendPacket("GET_LEADERBOARD|10");
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;
                groupBox1.Text = $"Підключено як: {NickName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isConnected = false;
            }
        }

        private async Task ListenForPackets()
        {
            byte[] buffer = new byte[1024];
            var sb = new StringBuilder();

            while (isConnected)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    while (true)
                    {
                        string all = sb.ToString();
                        int nl = all.IndexOf('\n');
                        if (nl < 0) break;

                        string oneMsg = all.Substring(0, nl).Trim('\r');
                        sb.Remove(0, nl + 1);

                        if (!string.IsNullOrWhiteSpace(oneMsg))
                        {
                            this.Invoke((MethodInvoker)(() => ProcessServerMessage(oneMsg)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    isConnected = false;
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Помилка з'єднання: {ex.Message}", "Помилка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        groupBox2.Enabled = false;
                        groupBox3.Enabled = false;
                        groupBox1.Text = "Не підключено";
                    });
                    break;
                }
            }
        }

        private void ProcessServerMessage(string msg)
        {
            Console.WriteLine("SERVER -> " + msg);
            string[] parts = msg.Split('|');
            string command = parts[0];
            switch (command)
            {
                case "LOBBY_LIST":
                    lst_Rooms.Items.Clear();
                    if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                    {
                        var rooms = parts[1].Split(',');
                        lst_Rooms.Items.AddRange(rooms);
                    }
                    break;
                case "ROOM_CREATED":
                    MessageBox.Show($"Кімната '{parts[1]}' створена!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    groupBox3.Enabled = false;
                    break;
                case "GAME_START":
                    mySymbol = parts[1];
                    string opponent = parts[2];
                    StartGameUI(opponent);
                    break;
                case "UPDATE":
                    int index = int.Parse(parts[1]);
                    string symbol = parts[2];
                    UpdateBoard(index, symbol);
                    break;
                case "TURN":
                    string currentTurnNick = parts[1];
                    groupBox2.Text = (currentTurnNick == NickName) ? "Ваш хід!" : $"Хід супротивника: {currentTurnNick}";
                    break;
                case "WIN":
                    MessageBox.Show($"Гра завершена! Переміг: {parts[1]}", "Гра завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetGameUI();
                    _ = SendPacket("GET_LEADERBOARD|10");
                    
                    break;
                case "DRAW":
                    MessageBox.Show("Гра завершена! Нічия!", "Гра завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetGameUI();
                    _ = SendPacket("GET_LEADERBOARD|10");
                    
                    break;
                case "OPPONENT_LEFT":
                    MessageBox.Show("Ваш супротивник вийшов з гри!", "Гра завершена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetGameUI();
                    break;
                case "ERROR":
                    MessageBox.Show($"Помилка від сервера: {parts[1]}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case "LEADERBOARD":
                    RenderLeaderboard(parts.Length > 1 ? parts[1]:"");
                    break;
            }
        }

        private void RenderLeaderboard(string payload)
        {
            dgv_Leaderboard.Rows.Clear();
            if (string.IsNullOrEmpty(payload)) return;

            var items = payload.Split(',', StringSplitOptions.RemoveEmptyEntries);

            int place = 1;
            foreach (var item in items) 
            {
                var parts = item.Split(':');
                if (parts.Length < 5) continue;

                string nick = parts[0];
                int wins = int.TryParse(parts[1], out var w) ? w : 0;
                int losses = int.TryParse(parts[2], out var l) ? l : 0;
                int draws = int.TryParse(parts[3], out var d) ? d : 0;
                int games = int.TryParse(parts[4], out var g) ? g : 0;

                int rowIndex = dgv_Leaderboard.Rows.Add(place, nick, wins, losses, draws, games);

                if (place == 1) dgv_Leaderboard.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Gold;
                else if (place == 2) dgv_Leaderboard.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Silver;
                else if (place == 3) dgv_Leaderboard.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Peru;

                place++;
            }
        }

        private void ResetGameUI()
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = true;
            btn_createRoom.Enabled = true;
            groupBox2.Text = "Ігрове поле";
            foreach (var btn in gameButtons)
            {
                btn.Text = "";
                btn.BackColor = SystemColors.Control;
            }
            _ = SendPacket("REFRESH_LOBBY");
        }

        private void UpdateBoard(int index, string symbol)
        {
            if (index >= 0 && index < gameButtons.Length)
            {
                gameButtons[index].Text = symbol;
                if (symbol == "X")
                {
                    gameButtons[index].BackColor = Color.Yellow;
                }
                else
                {
                    gameButtons[index].BackColor = Color.Blue;
                }
            }
        }

        private void StartGameUI(string opponent)
        {
            groupBox2.Enabled = true;
            groupBox3.Enabled = false;
            groupBox2.Text = $"Ірове поле: Ви {NickName} символ: {mySymbol} vs {opponent} {(mySymbol == "X" ? "O" : "X")}";

            foreach (var btn in gameButtons)
            {
                btn.Text = "";
                btn.BackColor = SystemColors.Control;
            }
        }

        private async void btn_createRoom_Click(object sender, EventArgs e)
        {
            string roomName = txt_roomName.Text.Trim();
            if (string.IsNullOrWhiteSpace(roomName))
            {
                MessageBox.Show("Назва кімнати не може бути порожньою!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            await SendPacket($"CREATE_ROOM|{roomName}");
            await SendPacket("REFRESH_LOBBY");
            btn_createRoom.Enabled = false;
        }

        private async void lst_Rooms_DoubleClick(object sender, EventArgs e)
        {
            if (lst_Rooms.SelectedItems != null)
            {
                string roomName = lst_Rooms.SelectedItem.ToString();
                await SendPacket($"JOIN_ROOM|{roomName}");
            }
        }

        private async void btn_updateRoom_Click(object sender, EventArgs e)
        {
            await SendPacket("REFRESH_LOBBY");
            await SendPacket("GET_LEADERBOARD|10");
        }
    }
}
