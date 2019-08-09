using System;
using System.Net.Sockets;

namespace SocketServer
{
    public class ChatBuffer
    {
        public byte[] buffer;
        public Socket workingSocket; //어떤 Socket에서 메시지를 받았는지 표시하기위함
        public readonly int bufferSize;

        public ChatBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
            buffer = new byte[this.bufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(buffer, 0, bufferSize);
        }
    }
}
