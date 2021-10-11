using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarDisplay
{
    public partial class RMRLoginForm : Form
    {
        public SocketManager sm;
        public bool Silent = false;
        public bool isClosed = false;

        public RMRLoginForm()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (sm != null)
            {
                sm.isClosed = true;
                sm = null;
                button1.Text = "Login";
                return;
            }
            try
            {
                sm = new SocketManager(serverTextBox.Text, int.Parse(portTextBox.Text), textBoxCategory.Text);
                button1.Text = "Stop";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RMR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                sm = null;
                return;
            }
            MessageBox.Show("Login Finished", "RMR", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        }

        private void RMRLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && false /*sm != null*/)
            {
                e.Cancel = true;
            }
            else
            {
                isClosed = true;
            }
        }


    }
}
