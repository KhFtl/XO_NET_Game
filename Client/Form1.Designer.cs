namespace Client
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            button10 = new Button();
            btn_createRoom = new Button();
            textBox1 = new TextBox();
            label3 = new Label();
            txt_NickName = new TextBox();
            label4 = new Label();
            btn_Connect = new Button();
            txt_ServerPort = new TextBox();
            txt_IpServer = new TextBox();
            label2 = new Label();
            label1 = new Label();
            groupBox2 = new GroupBox();
            button9 = new Button();
            button8 = new Button();
            button7 = new Button();
            button6 = new Button();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            groupBox3 = new GroupBox();
            lst_Rooms = new ListBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button10);
            groupBox1.Controls.Add(btn_createRoom);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(txt_NickName);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(btn_Connect);
            groupBox1.Controls.Add(txt_ServerPort);
            groupBox1.Controls.Add(txt_IpServer);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(13, 10);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1454, 229);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Керування";
            // 
            // button10
            // 
            button10.Location = new Point(598, 165);
            button10.Name = "button10";
            button10.Size = new Size(233, 47);
            button10.TabIndex = 17;
            button10.Text = "Оновити кімнати";
            button10.UseVisualStyleBackColor = true;
            // 
            // btn_createRoom
            // 
            btn_createRoom.Location = new Point(598, 103);
            btn_createRoom.Name = "btn_createRoom";
            btn_createRoom.Size = new Size(233, 47);
            btn_createRoom.TabIndex = 16;
            btn_createRoom.Text = "Створити";
            btn_createRoom.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(196, 108);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(396, 39);
            textBox1.TabIndex = 15;
            textBox1.Text = "My Room";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label3.Location = new Point(6, 115);
            label3.Name = "label3";
            label3.Size = new Size(184, 32);
            label3.TabIndex = 14;
            label3.Text = "Назва кімнати";
            // 
            // txt_NickName
            // 
            txt_NickName.Location = new Point(884, 39);
            txt_NickName.Name = "txt_NickName";
            txt_NickName.Size = new Size(251, 39);
            txt_NickName.TabIndex = 13;
            txt_NickName.Text = "Vetal";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label4.Location = new Point(817, 46);
            label4.Name = "label4";
            label4.Size = new Size(52, 32);
            label4.TabIndex = 12;
            label4.Text = "Нік";
            // 
            // btn_Connect
            // 
            btn_Connect.Location = new Point(1141, 36);
            btn_Connect.Name = "btn_Connect";
            btn_Connect.Size = new Size(192, 42);
            btn_Connect.TabIndex = 11;
            btn_Connect.Text = "Підключитись";
            btn_Connect.UseVisualStyleBackColor = true;
            btn_Connect.Click += btn_Connect_Click;
            // 
            // txt_ServerPort
            // 
            txt_ServerPort.Location = new Point(677, 43);
            txt_ServerPort.Name = "txt_ServerPort";
            txt_ServerPort.Size = new Size(106, 39);
            txt_ServerPort.TabIndex = 10;
            txt_ServerPort.Text = "8848";
            // 
            // txt_IpServer
            // 
            txt_IpServer.Location = new Point(105, 45);
            txt_IpServer.Name = "txt_IpServer";
            txt_IpServer.Size = new Size(487, 39);
            txt_IpServer.TabIndex = 9;
            txt_IpServer.Text = "127.0.0.1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label2.Location = new Point(598, 50);
            label2.Name = "label2";
            label2.Size = new Size(73, 32);
            label2.TabIndex = 8;
            label2.Text = "Порт";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label1.Location = new Point(6, 45);
            label1.Name = "label1";
            label1.Size = new Size(99, 32);
            label1.TabIndex = 7;
            label1.Text = "Сервер";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button9);
            groupBox2.Controls.Add(button8);
            groupBox2.Controls.Add(button7);
            groupBox2.Controls.Add(button6);
            groupBox2.Controls.Add(button5);
            groupBox2.Controls.Add(button4);
            groupBox2.Controls.Add(button3);
            groupBox2.Controls.Add(button2);
            groupBox2.Controls.Add(button1);
            groupBox2.Location = new Point(15, 245);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(707, 439);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Ігрове поле";
            // 
            // button9
            // 
            button9.Location = new Point(502, 295);
            button9.Name = "button9";
            button9.Size = new Size(184, 83);
            button9.TabIndex = 8;
            button9.Text = "button9";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button1_Click;
            // 
            // button8
            // 
            button8.Location = new Point(252, 295);
            button8.Name = "button8";
            button8.Size = new Size(184, 83);
            button8.TabIndex = 7;
            button8.Text = "button8";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button1_Click;
            // 
            // button7
            // 
            button7.Location = new Point(6, 295);
            button7.Name = "button7";
            button7.Size = new Size(184, 83);
            button7.TabIndex = 6;
            button7.Text = "button7";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button1_Click;
            // 
            // button6
            // 
            button6.Location = new Point(502, 176);
            button6.Name = "button6";
            button6.Size = new Size(184, 83);
            button6.TabIndex = 5;
            button6.Text = "button6";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button1_Click;
            // 
            // button5
            // 
            button5.Location = new Point(252, 176);
            button5.Name = "button5";
            button5.Size = new Size(184, 83);
            button5.TabIndex = 4;
            button5.Text = "button5";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button1_Click;
            // 
            // button4
            // 
            button4.Location = new Point(6, 176);
            button4.Name = "button4";
            button4.Size = new Size(184, 83);
            button4.TabIndex = 3;
            button4.Text = "button4";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button1_Click;
            // 
            // button3
            // 
            button3.Location = new Point(502, 60);
            button3.Name = "button3";
            button3.Size = new Size(184, 83);
            button3.TabIndex = 2;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(252, 60);
            button2.Name = "button2";
            button2.Size = new Size(184, 83);
            button2.TabIndex = 1;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button1_Click;
            // 
            // button1
            // 
            button1.Location = new Point(6, 60);
            button1.Name = "button1";
            button1.Size = new Size(184, 83);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(lst_Rooms);
            groupBox3.Location = new Point(728, 245);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(739, 439);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Кімнати";
            // 
            // lst_Rooms
            // 
            lst_Rooms.FormattingEnabled = true;
            lst_Rooms.ItemHeight = 31;
            lst_Rooms.Location = new Point(6, 59);
            lst_Rooms.Name = "lst_Rooms";
            lst_Rooms.Size = new Size(727, 376);
            lst_Rooms.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1479, 696);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Font = new Font("Segoe UI", 14F);
            Margin = new Padding(5);
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Client for X_O Game";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private TextBox txt_NickName;
        private Label label4;
        private Button btn_Connect;
        private TextBox txt_ServerPort;
        private TextBox txt_IpServer;
        private Label label2;
        private Label label1;
        private Button button9;
        private Button button8;
        private Button button7;
        private Button button6;
        private Button button5;
        private Button button4;
        private Button button3;
        private Button button2;
        private Button button1;
        private GroupBox groupBox3;
        private ListBox lst_Rooms;
        private Button button10;
        private Button btn_createRoom;
        private TextBox textBox1;
        private Label label3;
    }
}
