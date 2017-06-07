using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutomationControl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        HarnessTCPClient Client = new HarnessTCPClient();

        private void Form1_Load(object sender, EventArgs e)
        {
            Client.DataReceived += Client_DataReceived;
        }

        private void Client_DataReceived(string obj)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                textBox1.Text = DateTime.Now.ToString() + Environment.NewLine + obj;
            }));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            using (StreamReader sr = new StreamReader(Application.StartupPath + "/New Text Document.txt"))
            {
                Client.Send(sr.ReadToEnd());
            }
        }

        private void btnBackend_Click(object sender, EventArgs e)
        {
            Client.Send("Setup Backend01");
        }

        private void btnFrontend_Click(object sender, EventArgs e)
        {
            Client.Send("Setup Frontend01");
        }

        private void btnControl_Click(object sender, EventArgs e)
        {
            Client.Send("Test PollControl");
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            Client.Send("Test PollChange");
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

            Client.Connect("192.168.4.82", 8000);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Client.Send("Test RELIABILITY");

        }

        private void btnBackend02_Click(object sender, EventArgs e)
        {
            Client.Send("Setup Backend02");
        }

        private void btnBackend03_Click(object sender, EventArgs e)
        {
            Client.Send("Setup Backend03");
        }
    }
}
