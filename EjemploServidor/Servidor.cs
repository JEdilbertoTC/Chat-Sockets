using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EjemploServidor
{
    class Servidor
    {
        private TcpListener serverSocket;
        private List<TcpClient> tcpClients;
        private List<string> messages;

        [Obsolete]
        public Servidor()
        {
            this.serverSocket = new TcpListener(8888);
            this.messages = new List<string>();
            this.tcpClients = new List<TcpClient>();
        }

        public void Start()
        {
            serverSocket.Start();
            Console.WriteLine(" >> Server Started");

            TcpClient clientSocket = default(TcpClient);
            Thread thread = new Thread(Listen);
            thread.Start(clientSocket);
        }

        private void Listen(object a)
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = (TcpClient)a;
                    tcpClient = serverSocket.AcceptTcpClient();
                    Console.WriteLine(" >> Accept connection from client");
                    this.tcpClients.Add(tcpClient);
                    Thread ThreadReadInformation = new Thread(ListenClient);
                    ThreadReadInformation.Start(tcpClient);

                }
                catch (Exception ex) { Console.WriteLine(ex); return; };
            }
        }

        private void ListenClient(object o)
        {
            TcpClient client = (TcpClient)o;
            while (true)
            {
                ReceiveClientInformation(client);
                SendAllUsers();
            }
        }

        private void ReceiveClientInformation(object a)
        {
            TcpClient clientSocket = (TcpClient)a;
            NetworkStream networkStream = clientSocket.GetStream();

            if (networkStream == null) return;

            byte[] bytesFrom = new byte[10000000];
            networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
            string data = Encoding.UTF8.GetString(bytesFrom);
            if (data != "")
                this.messages.Add(data);
            data = data.Substring(0, data.IndexOf("id"));
            Console.WriteLine("Received >> " + data);
        }

        private void SendAllUsers()
        {
            if (this.messages.Count < 0) return;

            foreach (var i in this.messages)
            {
                foreach (var j in this.tcpClients)
                {
                    if (j == null) return;
                    Send(i, j);
                }

            }
            this.messages.Clear();
        }

        private void Send(string data, object o)
        {
            TcpClient tcpClient = (TcpClient)o;
            NetworkStream networkStream = tcpClient.GetStream();

            if (!networkStream.CanWrite) return;

            data = data.Substring(0, data.IndexOf("id"));
            Byte[] sendBytes = Encoding.UTF8.GetBytes(data + "$");
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();

            Console.WriteLine("Sent >>" + data);
        }

    }
}