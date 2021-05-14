using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EjemploCliente
{
    public partial class Form1 : Form
    {
        private TcpClient clientSocket;
        private delegate void Cambia(string mensaje);
        private Thread threadReceiveInformation;
        NetworkStream serverStream;

        public Form1()
        {
            InitializeComponent();
        }

        private void SendInformation(string text)
        {
            if (!this.serverStream.CanWrite) return;

            byte[] outStream = Encoding.UTF8.GetBytes(text + "id"+new Random().Next(10000000)+"$");
            
            this.serverStream.Write(outStream, 0, outStream.Length);
            this.serverStream.Flush();
            UpdateTextBox("");
        }

        private void ReceiveInformation()
        {
            if (!serverStream.CanRead) return;

            byte[] inStream = new byte[1000000];
            this.serverStream.Read(inStream, 0, clientSocket.ReceiveBufferSize);
            string returndata = Encoding.UTF8.GetString(inStream);
            returndata = returndata.Substring(0, returndata.IndexOf("$"));
            msg(returndata);
        }

        private void UpdateTextBox(string text)
        {
            textBox2.Text = text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "") return;
            SendInformation(textBox2.Text);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                label1.Text = "Client Started";
                CreateConnection();
                label2.Text = "Client Socket Program - Server Connected ...";
            }
            catch (Exception ex) { listBox1.Items.Add(ex); };
        }

        private void CreateConnection()
        {
            this.clientSocket = new TcpClient();
            this.clientSocket.Connect("127.0.0.1", 8888);
        }

        public void msg(string mesg)
        {
            if (listBox1.InvokeRequired)
            {
                Cambia cambia = new Cambia(AddMessage);
                Invoke(cambia, new object[] { mesg });
            }
        }

        public void AddMessage(string mensaje)
        {
            listBox1.Items.Add(mensaje);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.serverStream = clientSocket.GetStream();
            if (!this.serverStream.DataAvailable) return;

            this.threadReceiveInformation = new Thread(ReceiveInformation);
            this.threadReceiveInformation.SetApartmentState(ApartmentState.STA);
            this.threadReceiveInformation.Start();
        }

    }
}