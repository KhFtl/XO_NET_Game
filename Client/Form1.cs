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

        private string NickName;
        private string mySymbol;
        
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


            foreach (var btn in gameButtons)
            {
                btn.Font = new Font("Segoe UI", 36, FontStyle.Bold);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.FromArgb(64, 64, 64);
                btn.BackColor = Color.White;
                btn.Cursor = Cursors.Hand;
            }

        }
 

        private async Task SendPacket(string msg)
        {
            if (!isConnected) return;
            try
            {
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
                MessageBox.Show("Ви не можете зробити хід зараз!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            Button btn = (Button)sender;
            if (!string.IsNullOrEmpty(btn.Text))
            {
                MessageBox.Show("Ця клітинка вже зайнята! Оберіть іншу!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (string.IsNullOrWhiteSpace(txt_NickName.Text))
            {
                MessageBox.Show("Будь ласка, введіть нікнейм!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            NickName = txt_NickName.Text.Trim();
            try
            {
                isConnected = true;
                client = new TcpClient();
                await client.ConnectAsync(txt_IpServer.Text, Convert.ToInt32(txt_ServerPort.Text));
                stream = client.GetStream();
                _ = ListenForPackets();
                await SendPacket($"LOGIN|{NickName}");
                
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;
                groupBox1.Text = $"Підключено як: {NickName}";
                btn_Connect.Text = "Підключено ✓";
                btn_Connect.Enabled = false;
                btn_Connect.BackColor = Color.FromArgb(16, 137, 62);
                
                MessageBox.Show($"Успішно підключено як {NickName}!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isConnected = false;
                btn_Connect.Enabled = true;
            }
        }

        private async Task ListenForPackets()
        {
            byte[] buffer = new byte[1024];
            while (isConnected)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    this.Invoke((MethodInvoker)delegate
                    {
                        ProcessServerMessage(msg);
                    });
                }
                catch (Exception ex)
                {
                    isConnected = false;
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"З'єднання втрачено: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        groupBox2.Enabled = false;
                        groupBox3.Enabled = false;
                        groupBox1.Text = "Не підключено";
                        btn_Connect.Enabled = true;
                        btn_Connect.Text = "Підключитись";
                        btn_Connect.BackColor = Color.FromArgb(0, 120, 215);
                    });
                    break;
                }
            }
        }

        private void ProcessServerMessage(string msg)
        {
            string[] parts = msg.Split('|');
            string command = parts[0];
            switch (command)
            {
                case "LOBBY_LIST":
                    lst_Rooms.Items.Clear();
                    if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                    {
                        var rooms = parts[1].Split(',');
                        foreach (var room in rooms)
                        {
                            if (!string.IsNullOrWhiteSpace(room))
                            {
                                lst_Rooms.Items.Add($"🎮 {room}");
                            }
                        }
                    }
                    if (lst_Rooms.Items.Count == 0)
                    {
                        lst_Rooms.Items.Add("Немає доступних кімнат");
                    }
                    break;
                    
                case "ROOM_CREATED":
                    MessageBox.Show($"Кімната '{parts[1]}' створена!\nОчікування суперника...", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    groupBox3.Enabled = false;
                    groupBox2.Text = "Очікування суперника...";
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
                    if (currentTurnNick == NickName)
                    {
                        groupBox2.Text = "🎯 ВАШ ХІД!";
                        groupBox2.ForeColor = Color.FromArgb(16, 137, 62);
                    }
                    else
                    {
                        groupBox2.Text = $"⏳ Хід гравця: {currentTurnNick}";
                        groupBox2.ForeColor = Color.FromArgb(200, 50, 50);
                    }
                    break;
                    
                case "WIN":
                    string winner = parts[1];
                    if (winner == NickName)
                    {
                        MessageBox.Show("🎉 ВІТАЄМО! ВИ ПЕРЕМОГЛИ! 🎉", "Перемога!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"😢 Ви програли. Переможець: {winner}", "Поразка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    ResetGameUI();
                    break;
                    
                case "DRAW":
                    MessageBox.Show("🤝 Нічия! Гра завершена без переможця.", "Нічия", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetGameUI();
                    break;
                    
                case "OPPONENT_LEFT":
                    MessageBox.Show("😔 Ваш суперник покинув гру!", "Гра завершена", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetGameUI();
                    break;
                    
                case "ERROR":
                    MessageBox.Show($"Помилка від сервера: {parts[1]}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                    
                case "LEADERBOARD":
                    ShowLeaderboard(parts);
                    break;
            }
        }

        private void ShowLeaderboard(string[] parts)
        {
            if (parts.Length < 2) return;
            
            var leaderboardForm = new Form();
            leaderboardForm.Text = "🏆 Таблиця лідерів";
            leaderboardForm.Size = new Size(800, 600);
            leaderboardForm.StartPosition = FormStartPosition.CenterParent;
            leaderboardForm.BackColor = Color.FromArgb(240, 240, 245);
            
            var listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.Font = new Font("Segoe UI", 12);
            
            listView.Columns.Add("Місце", 80);
            listView.Columns.Add("Гравець", 250);
            listView.Columns.Add("Перемоги", 120);
            listView.Columns.Add("Поразки", 120);
            listView.Columns.Add("Нічиї", 100);
            listView.Columns.Add("% Перемог", 120);
            
            if (parts[1] == "Таблиця лідерів порожня")
            {
                var emptyItem = new ListViewItem("Немає даних");
                listView.Items.Add(emptyItem);
            }
            else
            {
                var players = parts[1].Split(';');
                foreach (var player in players)
                {
                    var playerData = player.Split('|');
                    if (playerData.Length == 6)
                    {
                        var item = new ListViewItem(playerData[0]); // Місце
                        item.SubItems.Add(playerData[1]); // Нікнейм
                        item.SubItems.Add(playerData[2]); // Перемоги
                        item.SubItems.Add(playerData[3]); // Поразки
                        item.SubItems.Add(playerData[4]); // Нічиї
                        item.SubItems.Add(playerData[5]); // % перемог
                        listView.Items.Add(item);
                    }
                }
            }
            
            leaderboardForm.Controls.Add(listView);
            leaderboardForm.ShowDialog();
        }

        private void ResetGameUI()
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = true;
            btn_createRoom.Enabled = true;
            groupBox2.Text = "Ігрове поле";
            groupBox2.ForeColor = Color.FromArgb(50, 50, 50);
            
            foreach (var btn in gameButtons)
            {
                btn.Text = "";
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
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
                    gameButtons[index].BackColor = Color.FromArgb(255, 200, 100);
                    gameButtons[index].ForeColor = Color.FromArgb(200, 100, 0);
                }
                else
                {
                    gameButtons[index].BackColor = Color.FromArgb(150, 200, 255);
                    gameButtons[index].ForeColor = Color.FromArgb(0, 100, 200);
                }
            }
        }

        private void StartGameUI(string opponent)
        {
            groupBox2.Enabled = true;
            groupBox3.Enabled = false;
            string opponentSymbol = (mySymbol == "X") ? "O" : "X";
            groupBox2.Text = $"🎮 {NickName} ({mySymbol}) vs {opponent} ({opponentSymbol})";

            foreach (var btn in gameButtons)
            {
                btn.Text = "";
                btn.BackColor = Color.White;
                btn.ForeColor = Color.Black;
            }
            
            MessageBox.Show($"Гра розпочалася!\nВи граєте за: {mySymbol}\nСуперник: {opponent}", "Гра розпочалася", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (lst_Rooms.SelectedItem != null)
            {
                string selectedText = lst_Rooms.SelectedItem.ToString();
                if (selectedText == "Немає доступних кімнат") return;
                
                string roomName = selectedText.Replace("🎮 ", "").Trim();
                await SendPacket($"JOIN_ROOM|{roomName}");
            }
        }

        private async void btn_updateRoom_Click(object sender, EventArgs e)
        {
            await SendPacket("REFRESH_LOBBY");
            MessageBox.Show("Список кімнат оновлено!", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btn_showLeaderboard_Click(object sender, EventArgs e)
        {
            await SendPacket("GET_LEADERBOARD");
        }
    }
}
