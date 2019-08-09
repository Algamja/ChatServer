using System;
using System.Windows.Forms;

namespace SocketServer
{
    public partial class MainForm : Form
    {
        delegate void UpdateTextCallback(string msg);
        Chat chat = null;

        public MainForm()
        {
            InitializeComponent();

            chat = new Chat(this);
        }

        private void AppendMessage(string msg)
        {
            try
            {
                if (tbInsertMessage.InvokeRequired)
                {
                    UpdateTextCallback callback = new UpdateTextCallback(AppendMessage);
                    Invoke(callback, new object[] { msg });
                }
                else
                {
                    tbChatWindow.AppendText(msg + Environment.NewLine);
                    tbChatWindow.ScrollToCaret();
                    tbInsertMessage.Focus();
                }
            }
            catch { } //크로스 스레드 에러
        }

        public void NotiMessage(string msg)
        {
            msg = "!-------------알림--------------!\r\n" + msg + "\r\n--------------------------------\r\n";
            AppendMessage(msg);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            chat.StartServer(); //서버 start
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            chat.StopServer(); //서버 stop
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string msg = "";
            msg = "SERVER : " + tbInsertMessage.Text;

            chat.SendMessage(msg);
            AppendMessage(msg);

            tbInsertMessage.Text = "";
            tbInsertMessage.Focus();
        }

        private void TbInsertMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string msg = "";
                msg = "SERVER : " + tbInsertMessage.Text;

                chat.SendMessage(msg);
                AppendMessage(msg);

                tbInsertMessage.Text = "";
                tbInsertMessage.Focus();
            }
        }

        public void ReceiveMessage(string msg)
        {
            AppendMessage(msg);
        }
    }
}
