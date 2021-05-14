using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EjemploServidor
{
    class Servidor
    {
        private TcpListener serverSocket;
        private Hashtable connetions;
        private Hashtable messagge;

        [Obsolete]
        public Servidor()
        {
            serverSocket = new TcpListener(8888);
            connetions = new Hashtable();
            messagge = new Hashtable();
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
            if (!networkStream.DataAvailable) return;

            byte[] bytesFrom = new byte[10000000];
            networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
            string data = Encoding.UTF8.GetString(bytesFrom);
            this.connetions.Add(data, clientSocket);
            string id = data.Substring(data.IndexOf("id"), data.IndexOf("$"));
            data = data.Substring(0, data.IndexOf("id"));
            this.messagge.Add(id, data);
            Console.WriteLine("Received >> " + data);
        }

        private void SendAllUsers()
        {
            if (connetions.Count < 0) return;
            foreach (DictionaryEntry a in this.connetions)
            {
                Send(a.Key, a.Value);
                
            }

        }

        private void Send(object a, object o)
        {
            TcpClient tcpClient = (TcpClient)o;
            NetworkStream networkStream = tcpClient.GetStream();

            if (!networkStream.CanWrite) return;

            string data = (string)a;
            data = data.Substring(0, data.IndexOf("id"));
            Byte[] sendBytes = Encoding.UTF8.GetBytes(data + "$");
            networkStream.Write(sendBytes, 0, sendBytes.Length);
            networkStream.Flush();
            Console.WriteLine("Sent >>" + data);
            string key = (string)a;
        }

    }
}