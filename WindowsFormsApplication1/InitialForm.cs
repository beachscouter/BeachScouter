using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeachScouter
{
    public partial class InitialForm : Form
    {
        private Form_Main game_form;


        public InitialForm()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetupAssistant setupassistant = new SetupAssistant();
            setupassistant.Show();
            this.Hide();
        }

        private void InitialForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

   



      


    }
}
