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
    public partial class Form_Configuration : Form
    {
/****************************************************************************************************************************************/
/************************************ VARIABLE DEFINITIONS ******************************************************************************/
        private Form_Main parent_form; // so we can have access to the main form
        private Configuration configuration; // A reference for Program.configuration to save the configuration.
        // For the team configuration
        Boolean team1_up;

/*********************************** END OF VARIABLE DEFINITIONS ***********************************************************************/





        public Form_Configuration(Form_Main parent)
        {
            InitializeComponent();
            this.parent_form = parent;
            configuration = Program.getConfiguration();
            
            
            // Default: Team 1 is down
            team1_up = false;


            
        }

        private void Form_Settings_Load(object sender, EventArgs e){}


        private void Form_Configuration_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                load_configuration();
            }
            
        }

        private void Form_Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }

        private void button_cancel_Click(object sender, EventArgs e){
            this.Close();
        }


      


        private void button_apply_Click(object sender, EventArgs e)
        {
            

            configuration.Teama.Name = textBox_teamAname.Text;
            configuration.Teamb.Name = textBox_teamBname.Text;
            configuration.Teama.Player1.Name = textBox_teamAplayer1name.Text;
            configuration.Teama.Player2.Name = textBox_teamAplayer2name.Text;


            configuration.Teamb.Name = textBox_teamBname.Text;
            configuration.Teamb.Player1.Name = textBox_teamBplayer1name.Text;
            configuration.Teamb.Player2.Name = textBox_teamBplayer2name.Text;


            //send the configuration to the main (parent) form:
            // check if the position of the team has changed to also switch the team points:
            if (team1_up != configuration.TeamAup)
            {
                String temp_up_points = parent_form.textBox_scoreteamup.Text;
                parent_form.textBox_scoreteamup.Text = parent_form.textBox_scoreteamdown.Text;
                parent_form.textBox_scoreteamdown.Text = temp_up_points;

                configuration.TeamAup = team1_up; // now we can save the current team positions
            }

            // set the correct team labels
            if (team1_up)
            {
                parent_form.textBox_teamupname.Text = configuration.Teama.Name;
                parent_form.textBox_teamdownname.Text = configuration.Teamb.Name;
                parent_form.radioButton_playerupleft.Text = configuration.Teama.Player1.Name;
                parent_form.radioButton_playerupright.Text = configuration.Teama.Player2.Name;
                parent_form.radioButton_playerdownleft.Text = configuration.Teamb.Player1.Name;
                parent_form.radioButton_playerdownright.Text = configuration.Teamb.Player2.Name;
            }
            else 
            {
                parent_form.textBox_teamupname.Text = configuration.Teamb.Name;
                parent_form.textBox_teamdownname.Text = configuration.Teama.Name;
                parent_form.radioButton_playerupleft.Text = configuration.Teamb.Player1.Name;
                parent_form.radioButton_playerupright.Text = configuration.Teamb.Player2.Name;
                parent_form.radioButton_playerdownleft.Text = configuration.Teama.Player1.Name;
                parent_form.radioButton_playerdownright.Text = configuration.Teama.Player2.Name;
            }



            configuration.FreeText = richBox_freeText.Text;



            this.Close();
        }


      



        /*
         * This method loads the settings that were last saved in the configuration
         * 
         */
        public void load_configuration()
        {
       
            // Load the Team configuartion
            textBox_teamAname.Text = configuration.Teama.Name;
            textBox_teamAplayer1name.Text = configuration.Teama.Player1.Name;
            textBox_teamAplayer2name.Text = configuration.Teama.Player2.Name;

            textBox_teamBname.Text = configuration.Teamb.Name;
            textBox_teamBplayer1name.Text = configuration.Teamb.Player1.Name;
            textBox_teamBplayer2name.Text = configuration.Teamb.Player2.Name;

            team1_up = configuration.TeamAup;

            richBox_freeText.Text = configuration.FreeText;
        }





       


      






/*******************************************************************************************************************/
/********************************************* TEAM STUFF **********************************************************/ 

        private void pictureBox_team1positionUp_Click(object sender, EventArgs e)
        {
            team1_up = true;
            pictureBox_team1positionUp.Refresh();


            pictureBox_team1player1down.Visible = false;
            pictureBox_team1player2down.Visible = false;
            pictureBox_team1player1up.Visible = true;
            pictureBox_team1player2up.Visible = true;

            pictureBox_team2player1down.Visible = true;
            pictureBox_team2player2down.Visible = true;
            pictureBox_team2player1up.Visible = false;
            pictureBox_team2player2up.Visible = false;
        }

        private void pictureBox_team1positionDown_Click(object sender, EventArgs e)
        {
            team1_up = false;
            pictureBox_team1positionDown.Refresh();

            pictureBox_team1player1down.Visible = true;
            pictureBox_team1player2down.Visible = true;
            pictureBox_team1player1up.Visible = false;
            pictureBox_team1player2up.Visible = false;

            pictureBox_team2player1down.Visible = false;
            pictureBox_team2player2down.Visible = false;
            pictureBox_team2player1up.Visible = true;
            pictureBox_team2player2up.Visible = true;
        }

        private void pictureBox_team2positionUp_Click(object sender, EventArgs e)
        {
            team1_up = false;
            pictureBox_team1positionDown.Refresh();

            pictureBox_team1player1down.Visible = true;
            pictureBox_team1player2down.Visible = true;
            pictureBox_team1player1up.Visible = false;
            pictureBox_team1player2up.Visible = false;

            pictureBox_team2player1down.Visible = false;
            pictureBox_team2player2down.Visible = false;
            pictureBox_team2player1up.Visible = true;
            pictureBox_team2player2up.Visible = true;

        }

        private void pictureBox_team2positionDown_Click(object sender, EventArgs e)
        {
            team1_up = true;
            pictureBox_team1positionUp.Refresh();

            pictureBox_team1player1down.Visible = false;
            pictureBox_team1player2down.Visible = false;
            pictureBox_team1player1up.Visible = true;
            pictureBox_team1player2up.Visible = true;

            pictureBox_team2player1down.Visible = true;
            pictureBox_team2player2down.Visible = true;
            pictureBox_team2player1up.Visible = false;
            pictureBox_team2player2up.Visible = false;
        }





        private void pictureBox_team1positionUp_Paint(object sender, PaintEventArgs e)
        {
            int bw = 4;
            Pen p = new Pen(Brushes.DimGray, bw);

            if (team1_up)
            {
                Graphics g;
                // Team is up
                g = e.Graphics;
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team1positionUp.Size.Width - bw, pictureBox_team1positionUp.Size.Height - bw));

                // Toggle positions
                g = pictureBox_team1positionDown.CreateGraphics();
                g.Clear(Color.LemonChiffon);

                g = pictureBox_team2positionUp.CreateGraphics();
                g.Clear(Color.LemonChiffon);

                g = pictureBox_team2positionDown.CreateGraphics();
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team2positionDown.Size.Width - bw, pictureBox_team2positionDown.Size.Height - bw));
            
  
            }

            if (!team1_up)
            {
                Graphics g; 
                // Team is up
                g = e.Graphics;
                g.Clear(Color.LemonChiffon);
 
                g = pictureBox_team2positionUp.CreateGraphics();
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team2positionUp.Size.Width - bw, pictureBox_team2positionUp.Size.Height - bw));
            }

        }

        private void pictureBox_team1positionDown_Paint(object sender, PaintEventArgs e)
        {
            int bw = 4;
            Pen p = new Pen(Brushes.DimGray, bw);

            if (team1_up)
            {
                Graphics g;
                // Team is up
                g = e.Graphics;
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team1positionUp.Size.Width - bw, pictureBox_team1positionUp.Size.Height - bw));
                
                // Toggle positions
                g = pictureBox_team1positionDown.CreateGraphics();
                g.Clear(Color.LemonChiffon);

                g = pictureBox_team2positionUp.CreateGraphics();
                g.Clear(Color.LemonChiffon);

                g = pictureBox_team2positionDown.CreateGraphics();
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team2positionDown.Size.Width - bw, pictureBox_team2positionDown.Size.Height - bw));
            }

            if (!team1_up)
            {
                Graphics g;
                // Team is up
                g = pictureBox_team1positionUp.CreateGraphics();
                g.Clear(Color.LemonChiffon);

                // Toggle positions
                g = e.Graphics;
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team1positionDown.Size.Width - bw, pictureBox_team1positionDown.Size.Height - bw));

                g = pictureBox_team2positionDown.CreateGraphics();
                g.Clear(Color.LemonChiffon);

                g = pictureBox_team2positionUp.CreateGraphics();
                g.DrawRectangle(p, new Rectangle(1, 1, pictureBox_team2positionUp.Size.Width - bw, pictureBox_team2positionUp.Size.Height - bw));
            }

        }

        private void button_resultdown1_Click(object sender, EventArgs e)
        {
            textBox_result1.Text = (int.Parse(textBox_result1.Text) - 1).ToString();
            if (int.Parse(textBox_result1.Text) <= 0)
            {
                button_resultdown1.Enabled = false;
            }
            configuration.Teama_score = int.Parse(textBox_result1.Text);
        }

        private void button_resultdown2_Click(object sender, EventArgs e)
        {
            textBox_result2.Text = (int.Parse(textBox_result2.Text) - 1).ToString();
            if (int.Parse(textBox_result2.Text) <= 0)
            {
                button_resultdown2.Enabled = false;
            }
            configuration.Teamb_score = int.Parse(textBox_result2.Text);
        }

        private void button_resultup1_Click(object sender, EventArgs e)
        {
            textBox_result1.Text = (int.Parse(textBox_result1.Text) + 1).ToString();
            configuration.Teama_score = int.Parse(textBox_result1.Text);
        }

        private void button_resultup2_Click(object sender, EventArgs e)
        {
            textBox_result2.Text = (int.Parse(textBox_result2.Text) + 1).ToString();
            configuration.Teamb_score = int.Parse(textBox_result2.Text);
        }

/************************************* END OF TEAM STUFF ***********************************************************************************************/





    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            