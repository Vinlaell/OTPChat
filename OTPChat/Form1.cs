using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace OTPChat
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private TcpListener tcpListener;
        private Thread listenThread;

        string modnum(int c)
        {
            string result = "";
            switch (c)
            {
                case 1: result = "a"; break;
                case 2: result = "b"; break;
                case 3: result = "c"; break;
                case 4: result = "d"; break;
                case 5: result = "e"; break;
                case 6: result = "f"; break;
                case 7: result = "g"; break;
                case 8: result = "h"; break;
                case 9: result = "i"; break;
                case 10: result = "j"; break;
                case 11: result = "k"; break;
                case 12: result = "l"; break;
                case 13: result = "m"; break;
                case 14: result = "n"; break;
                case 15: result = "o"; break;
                case 16: result = "p"; break;
                case 17: result = "q"; break;
                case 18: result = "r"; break;
                case 19: result = "s"; break;
                case 20: result = "t"; break;
                case 21: result = "u"; break;
                case 22: result = "v"; break;
                case 23: result = "w"; break;
                case 24: result = "x"; break;
                case 25: result = "y"; break;
                case 26: result = "z"; break;
            }
            return result;
        }

        int modchar(char c)
        {
            int result = 0;
            switch (c)
            {
                case 'a': result = 1; break;
                case 'b': result = 2; break;
                case 'c': result = 3; break;
                case 'd': result = 4; break;
                case 'e': result = 5; break;
                case 'f': result = 6; break;
                case 'g': result = 7; break;
                case 'h': result = 8; break;
                case 'i': result = 9; break;
                case 'j': result = 10; break;
                case 'k': result = 11; break;
                case 'l': result = 12; break;
                case 'm': result = 13; break;
                case 'n': result = 14; break;
                case 'o': result = 15; break;
                case 'p': result = 16; break;
                case 'q': result = 17; break;
                case 'r': result = 18; break;
                case 's': result = 19; break;
                case 't': result = 20; break;
                case 'u': result = 21; break;
                case 'v': result = 22; break;
                case 'w': result = 23; break;
                case 'x': result = 24; break;
                case 'y': result = 25; break;
                case 'z': result = 26; break;
            }
            return result;
        }
        void padgen()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                uint value;
                byte[] data = new byte[4096];
                System.IO.StreamWriter file = new System.IO.StreamWriter("pad.txt");
                System.IO.StreamWriter file2 = new System.IO.StreamWriter("pad2.txt");


                for (int i = 0; i < 1000; i++)
                {
                    rng.GetBytes(data);
                    value = BitConverter.ToUInt32(data, 0);
                    file.Write(value);
                    file2.Write(value);
                }
                file.Close();
                file2.Close();

            }
        }

        string cipher(string msg)
        {
            string textread = System.IO.File.ReadAllText(@"pad.txt");
            string padmatch = textread.Substring(0, msg.Length);
            string ctext = "";
            int[] resa = new int[26];
            int[] resb = new int[26];
            if (System.Text.RegularExpressions.Regex.IsMatch(msg, @"^[a-z]+$")) { }
            else
            {
                MessageBox.Show("input is not regular lowercase letters with no spaces!");
                return "";
            }
            for (int i = 0; i < msg.Length; i++)
            {
                resa[i] = modchar(msg[i]);
                resb[i] = resa[i] + Convert.ToInt32(padmatch.Substring(i, 1));
                if (resb[i] > 26) resb[i] = resb[i] - 26;
            }
            foreach (int i in resb)
            {
                ctext = ctext + modnum(i).ToString();
            }
            //MessageBox.Show("ciphering "+msg);
            textread = textread.Substring(msg.Length, textread.Length - msg.Length);
            System.IO.StreamWriter file = new System.IO.StreamWriter("pad.txt");
            file.Write(textread);
            file.Close();
            return ctext;
        }

        string decipher(string msg)
        {
            string textread = System.IO.File.ReadAllText(@"pad2.txt");
            string padmatch = textread.Substring(0, msg.Length);
            string ctext = "";
            int[] resa = new int[26];
            int[] resb = new int[26];
            for (int i = 0; i < msg.Length; i++)
            {
                resa[i] = modchar(msg[i]);
                resb[i] = resa[i] - Convert.ToInt32(padmatch.Substring(i, 1));
                if (resb[i] < 1) resb[i] = resb[i] + 26;
            }
            foreach (int i in resb)
            {
                ctext = ctext + modnum(i).ToString();
            }
            //MessageBox.Show("deciphering " + msg);

            textread = textread.Remove(0, msg.Length);
            System.IO.StreamWriter file = new System.IO.StreamWriter("pad2.txt");
            file.Write(textread);
            file.Close();
            return ctext;
        }

        public void Sendmsg()
        {
            TcpClient client = new TcpClient();

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.ip), Convert.ToInt32(Properties.Settings.Default.port2));

            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(cipher(textBox2.Text));
            textBox1.Text = textBox1.Text + "[me] " + textBox2.Text + Environment.NewLine;
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
            
        }

        public void Server()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, Convert.ToInt32(Properties.Settings.Default.port));
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                string msg = encoder.GetString(message, 0, bytesRead);
                //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
                
                    Properties.Settings.Default.message = msg;

                
            }

            //tcpClient.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripLabel1.Text = "Idle";
            textBox3.Text = Properties.Settings.Default.ip;
            textBox4.Text = Properties.Settings.Default.port;
            textBox5.Text = Properties.Settings.Default.port2;
        }



        private void button1_Click(object sender, EventArgs e)
        {

            //listen
            toolStripLabel1.Text = "Listening..";
            Server();


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            Properties.Settings.Default.message = "";
            Properties.Settings.Default.msg2 = "";
            Properties.Settings.Default.Save();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.message != Properties.Settings.Default.msg2)
            {
                Properties.Settings.Default.msg2 = Properties.Settings.Default.message;
                //MessageBox.Show(Properties.Settings.Default.message);

                textBox1.AppendText(decipher(Properties.Settings.Default.message) + Environment.NewLine);
                

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            Sendmsg();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            //if (Properties.Settings.Default.message != Properties.Settings.Default.msg2)
            //{
            //textBox1.AppendText(Properties.Settings.Default.message);
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //toolStripLabel1.Text = "Connecting..";
            //button1.Text = "Cancel";
            //connect
            //
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.port = textBox4.Text;
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            DialogResult result;
            result = MessageBox.Show("Are you sure you want to generate a new pad?", "Pad Generator", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {

            }
            if (result == DialogResult.Yes)
            {
                padgen();
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.port2 = textBox5.Text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ip = textBox3.Text;
        }
    }
}
