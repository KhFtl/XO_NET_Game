using System.Net.Sockets;
using System.Text;

namespace Client
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Button[] gameButtons = new Button[9];

        private string NickName;
        private string mySymbol; 
        private int pressedIndex = -1; 
        public Form1()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            gameButtons[0] = button1;
            gameButtons[1] = button2;
            gameButtons[2] = button3;
            gameButtons[3] = button4;
            gameButtons[4] = button5;
            gameButtons[5] = button6;
            gameButtons[6] = button7;
            gameButtons[7] = button8;
            gameButtons[8] = button9;
        }
        private async Task MakeMove(int index)
        {
            if (gameButtons[index].Text != "") return;
            await SendPacket($"MOVE|{index}");
        }

        private async Task SendPacket(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private void button1_Click(object sender, EventArgs e)
        {
 
            Button btn = (Button)sender;
            int index = Array.IndexOf(gameButtons, btn);
            pressedIndex = index;
        }

        private async void btn_Connect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_NickName.Text)) return;
            NickName = txt_NickName.Text.Trim();
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(txt_IpServer.Text, Convert.ToInt32(txt_ServerPort.Text));
                stream = client.GetStream();
                await SendPacket($"LOGIN|{NickName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення: {ex.Message}","Помилка",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
