using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing.Drawing2D;

namespace BeachScouter
{
    public partial class SetupAssistant : Form
    {
        /****************************************************************************************************************************************/
        /************************************ VARIABLE DEFINITIONS ******************************************************************************/
        private Configuration configuration; // A reference for Program.configuration to save the configuration.
        private CaptureStream capture_stream;
        private Form_Main game_form;
        private Capture capture;
        private XmlOutput xmlDoc;
        private int capture_device_index =0;
        private String loaded_video_path;
        // For the team configuration
        Boolean team1_up;

        /*********************************** END OF VARIABLE DEFINITIONS ***********************************************************************/


      
        public SetupAssistant()
        {
            InitializeComponent();

            configuration = Program.getConfiguration();
            capture_stream = new CaptureStream();
            capture = Program.Capture;


            // Default: Team 1 is down
            team1_up = false;

            textBox_savelocation.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + textBox_gamename.Text;
        }

  


        private void SetupAssistant_VisibleChanged(object sender, EventArgs e)
        {
            
            if (this.Visible == true)
            {
                xmlDoc = new XmlOutput();
            }
            
        }




        private void button_next_Click(object sender, EventArgs e)
        {
            tabControl_setup.SelectedIndex++;
            button_back.Enabled = true;

            if (button_next.Text == "Finish")
                Finish();

            if (tabControl_setup.SelectedIndex == 1)
                button_next.Text = "Finish";

            
        }

        private void button_back_Click(object sender, EventArgs e)
        {
            tabControl_setup.SelectedIndex--;
            if (tabControl_setup.SelectedIndex == 0)
                button_back.Enabled = false;

             button_next.Text = "Next";

            
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }





        private void Finish()
        {
            Application.Idle -= ProcessFrame;

            disposeCapture();

            configuration.Teama.Name = textBox_teamAname.Text;
            configuration.Teamb.Name = textBox_teamBname.Text;
            configuration.Teama.Player1.Name = textBox_teamAplayer1name.Text;
            configuration.Teama.Player1.Firstname = getFirstandLastName(textBox_teamAplayer1name.Text)[0];
            configuration.Teama.Player1.Surname = getFirstandLastName(textBox_teamAplayer1name.Text)[1];
            configuration.Teama.Player2.Name = textBox_teamAplayer2name.Text;
            configuration.Teama.Player2.Firstname = getFirstandLastName(textBox_teamAplayer2name.Text)[0];
            configuration.Teama.Player2.Surname = getFirstandLastName(textBox_teamAplayer2name.Text)[1];



            configuration.Teamb.Name = textBox_teamBname.Text;
            configuration.Teamb.Player1.Name = textBox_teamBplayer1name.Text;
            configuration.Teamb.Player1.Firstname = getFirstandLastName(textBox_teamBplayer1name.Text)[0];
            configuration.Teamb.Player1.Surname = getFirstandLastName(textBox_teamBplayer1name.Text)[1];
            configuration.Teamb.Player2.Name = textBox_teamBplayer2name.Text;
            configuration.Teamb.Player2.Firstname = getFirstandLastName(textBox_teamBplayer2name.Text)[0];
            configuration.Teamb.Player2.Surname = getFirstandLastName(textBox_teamBplayer2name.Text)[1];

            configuration.LoadXmlFile = textBox_savelocation.Text;
            configuration.FreeText = richBox_freeText.Text;
            configuration.setGameIDAndCreateOutputFilesDir(textBox_savelocation.Text, textBox_gamename.Text);

            loaded_video_path = label_videopath.Text;
            if (radioButton_camera.Checked)
                capture_device_index = listBox_cameras.SelectedIndex;

            this.Dispose();

            // Start a new Thread for the Game Form so we can close the setup assistant form
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(Main));
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
        }

        // For the Main Form thread
        public void Main()
        {
            if (this.game_form != null)
                this.game_form.Dispose();

            this.game_form = new Form_Main(xmlDoc);

            if (radioButton_camera.Checked)
                this.game_form.setCaptureDeviceIndex(capture_device_index);
            else
            {
                this.game_form.setLoadedVideoPath(loaded_video_path);
            }

            
            //send the configuration to the main (parent) form:
            // check if the position of the team has changed to also switch the team points:
            if (team1_up != configuration.TeamAup)
            {
                String temp_up_points = game_form.textBox_scoreteamup.Text;
                game_form.textBox_scoreteamup.Text = game_form.textBox_scoreteamdown.Text;
                game_form.textBox_scoreteamdown.Text = temp_up_points;

                configuration.TeamAup = team1_up; // now we can save the current team positions
            }

            // set the correct team labels
            if (team1_up)
            {
                game_form.textBox_teamupname.Text = configuration.Teama.Name;
                game_form.textBox_teamdownname.Text = configuration.Teamb.Name;
                game_form.radioButton_playerupleft.Text = configuration.Teama.Player1.Name;
                game_form.radioButton_playerupright.Text = configuration.Teama.Player2.Name;
                game_form.radioButton_playerdownleft.Text = configuration.Teamb.Player1.Name;
                game_form.radioButton_playerdownright.Text = configuration.Teamb.Player2.Name;
            }
            else
            {
                game_form.textBox_teamupname.Text = configuration.Teamb.Name;
                game_form.textBox_teamdownname.Text = configuration.Teama.Name;
                game_form.radioButton_playerupleft.Text = configuration.Teamb.Player1.Name;
                game_form.radioButton_playerupright.Text = configuration.Teamb.Player2.Name;
                game_form.radioButton_playerdownleft.Text = configuration.Teama.Player1.Name;
                game_form.radioButton_playerdownright.Text = configuration.Teama.Player2.Name;
            }



            

            Application.EnableVisualStyles();
            Application.Run(game_form);
        }

        /********************************************************************************************************************************************/
        /************************************************ CAPTURE STUFF *****************************************************************************/

        public void listCaptureDevices()
        {
            disposeCapture();

            listBox_cameras.Items.Clear();

            for (int i = 0; i < 2; i++)
            {
                capture = new Capture(i);
                if (capture.QueryFrame() != null)
                    listBox_cameras.Items.Add("Camera "+i);
                capture.Dispose();
            }

            if (listBox_cameras.Items.Count > 0)
                listBox_cameras.SelectedIndex = 0;
            
        }

        private void listBox_cameras_SelectedIndexChanged(object sender, EventArgs e)
        {


            capture_device_index = listBox_cameras.SelectedIndex;
            radioButton_camera.Checked = true;


            // Show video
            Application.Idle -= ProcessFrame;

            disposeCapture();

            capture_device_index = listBox_cameras.SelectedIndex;

            capture = new Capture(capture_device_index);

            Application.Idle += ProcessFrame;
           
        }


        private void radioButton_camera_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_camera.Checked)
            {
                listCaptureDevices();
            }

        }


        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (capture != null)
            {
                try
                {
                    Image<Bgr, Byte> nextFrame = (capture).QueryFrame();
                    try
                    {
                        if (nextFrame!=null)
                            pictureBox_calibrationvideo.Image = nextFrame.ToBitmap();
                    }
                    catch (ArgumentException) { Console.WriteLine("EXCEPTION: ArgumentException"); }
                }
                catch (AccessViolationException) { Console.WriteLine("EXCEPTION: AccessViolationException"); }
            }
        }



        /*******************************************************************************************************************/
        /********************************************* TEAM STUFF **********************************************************/
        private String[] getFirstandLastName(string name)
        {
            string[] splitedname = new string[2];
            char[] splitter = { ' ' };
            splitedname = name.Split(splitter);

            if (splitedname.Length == 2)
                return splitedname;

            return new String[2] { name, "" };
        }


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

        /************************************* END OF TEAM STUFF ***********************************************************************************************/



        


        private void button_opensavefolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderbrowser = new FolderBrowserDialog();
            DialogResult result = STAShowBrowseDialog(folderbrowser);

            if (result == DialogResult.OK)
            {
                textBox_savelocation.Text = folderbrowser.SelectedPath + @"\" + textBox_gamename.Text;
            }
        }

        private void textBox_gamename_TextChanged(object sender, EventArgs e)
        {
            int index = textBox_savelocation.Text.LastIndexOf(@"\");
            String path = textBox_savelocation.Text.Substring(0, index);
            textBox_savelocation.Text = path + @"\" + textBox_gamename.Text;
        }

        private void textBox_gamename_Leave(object sender, EventArgs e)
        {
            if (textBox_gamename.Text == "")
                textBox_gamename.Text = "NewGame";
        }

        private void button_videofile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfiledialog = new OpenFileDialog();
            DialogResult result = STAShowSaveDialog(openfiledialog);
            if (result == DialogResult.OK)
                {
                    if (openfiledialog.CheckPathExists)
                    {
                        loaded_video_path = openfiledialog.FileName;
                        label_videopath.Text = loaded_video_path;
                        radioButton_videofile.Checked = true;
                    }
                }


        }

        private DialogResult STAShowSaveDialog(FileDialog dialog)
        {
            SaveDialogState state = new SaveDialogState();
            state.dialog = dialog;
            System.Threading.Thread t = new System.Threading.Thread(state.ThreadProcShowDialog);
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            t.Join();
            return state.result;
        }

        private DialogResult STAShowBrowseDialog(FolderBrowserDialog dialog)
        {
            BrowseDialogState state = new BrowseDialogState();
            state.dialog = dialog;
            System.Threading.Thread t = new System.Threading.Thread(state.ThreadProcShowDialog);
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            t.Join();
            return state.result;
        }

        private void disposeCapture()
        {
            if (capture != null)
                capture.Dispose();
        }

        private void SetupAssistant_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void SetupAssistant_Load(object sender, EventArgs e)
        {

        }

       

       

  
         
    }


    


    
}
