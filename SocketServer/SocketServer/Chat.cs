using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SocketServer
{
    class Chat
    {
        MainForm mMainForm = null;

        Socket mServerSocket = null;
        Socket mClientSocket = null;

        List<Socket> clients = new List<Socket>(); //대화에 참여중인 Client를 저장하는 List

        readonly string IP = "127.0.0.1";
        readonly int PORT = 1212;

        char mClientName = 'A';

        public Chat() { }

        public Chat(MainForm form)
        {
            mMainForm = form;
        }

        public void StartServer()
        {
            mMainForm.NotiMessage("서버를 시작합니다.");

            IPEndPoint serverIpep = new IPEndPoint(IPAddress.Any, PORT); //모든 Client에서 오는 요청을 받음

            mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mServerSocket.Bind(serverIpep);
            mServerSocket.Listen(10);
            mMainForm.NotiMessage("클라이언트의 접속을 대기합니다.");

            mServerSocket.BeginAccept(WaitToConnect, null); //클라이언트 접속을 대기 but UI가 멈추지 않음 (비동기)
        }

        public void StopServer()
        {
            if (mClientSocket != null && mClientSocket.Connected)
            {
                mMainForm.NotiMessage("연결을 해지합니다.");
                mClientSocket.Close();
            }

            if (mServerSocket != null && mServerSocket.Connected)
            {
                mMainForm.NotiMessage("서버를 종료합니다.");
                mServerSocket.Close();
                mMainForm.Close();
            }
        }

        private void WaitToConnect(IAsyncResult ar) //client 접속을 기다리는 동안 실행
        {
            if (clients.Count < 2)
            {
                mClientSocket = mServerSocket.EndAccept(ar); //들어오는 접속 신호를 비동기적으로 받아들임
                clients.Add(mClientSocket);

                mClientName = Convert.ToChar(clients.Count + 64);
                mMainForm.NotiMessage("클라이언트" + mClientName + "가 접속했습니다.");

                mServerSocket.BeginAccept(WaitToConnect, null); //다른 클라이언트의 접속을 위함

                ChatBuffer chatBuffer = new ChatBuffer(1024); //Client에서 보낸 data를 byte형식으로 받기 위한 class
                chatBuffer.workingSocket = mClientSocket;

                mClientSocket.BeginReceive(chatBuffer.buffer, 0, chatBuffer.buffer.Length, 0, DataReceived, chatBuffer); //data를 비동기적으로 받음
            }
        }

        void DataReceived(IAsyncResult ar)
        {
            if (mClientSocket.Connected)
            {
                ChatBuffer chatBuffer = (ChatBuffer)ar.AsyncState; //BeginReceive의 마지막 매개변수인 buffer를 가져옴

                string msg = Encoding.UTF8.GetString(chatBuffer.buffer).Trim('\0'); //\0을 지우기 위함

                SendMessage(chatBuffer.workingSocket, msg); //받은 msg를 Client에도 전송

                chatBuffer.ClearBuffer();

                chatBuffer.workingSocket.BeginReceive(chatBuffer.buffer, 0, chatBuffer.buffer.Length, 0, DataReceived, chatBuffer); //다른 메시지를 계속 받기 위함
            }
        }

        public void SendMessage(Socket sender, string msg) //Client에서 받은 메시지를 다른 Client에게 보낼 때
        {
            byte[] data = null;
            string message = "";

            if (mClientSocket == null || mClientSocket.Connected == false)
            {
                mMainForm.NotiMessage("메시지를 전송할 수 없습니다.");
                return;
            }

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                Socket imsiSocket = clients[i];

                if (imsiSocket.Handle == sender.Handle)
                {
                    mClientName = Convert.ToChar(i + 65); //clientName지정(List에 들어가있는 순서대로 A,B)
                    break;
                }
            }

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                Socket imsiSocket = clients[i];

                if (imsiSocket.Handle != sender.Handle) //메시지를 보낸 Client를 제외한 Client에만 메시지를 보냄
                {
                    message = msg;

                    message = mClientName + " : " + message;
                    data = Encoding.Default.GetBytes(message);
                    imsiSocket.Send(data, 0, data.Length, SocketFlags.None);
                }
            }

            message = msg;
            mMainForm.ReceiveMessage(mClientName + " : " + message); //서버의 form에서도 메시지를 보여줌
        }

        public void SendMessage(string msg) //서버에서 작성한 메시지를 Client에게 보낼 때
        {
            byte[] data = null;

            if (mClientSocket == null || mClientSocket.Connected == false)
            {
                mMainForm.NotiMessage("메시지를 전송할 수 없습니다.");
                return;
            }

            for (int i = clients.Count - 1; i >= 0; i--)
            {
                data = Encoding.Default.GetBytes(msg);
                clients[i].Send(data, 0, data.Length, SocketFlags.None);
            }
        }
    }
}
