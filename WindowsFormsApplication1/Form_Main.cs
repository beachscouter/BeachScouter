using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Splicer;
using Splicer.Timeline;
using Splicer.Renderer;using System.Threading;

namespace BeachScouter
{
    public partial class Form_Main : Form
    {
        /***************************************************************************************************************************/
        /************************************** VARIABLE DEFINITIONS ***************************************************************/
        Form configuration_form;
        Configuration configuration;

        /******* Video stuff ********/
        List<Image<Bgr, Byte>> buffer = new List<Image<Bgr, Byte>>();
        private CaptureStream capture_stream;
        private VideoWriter videoWriter;
        delegate void SetTextCallback(string text);
        private bool new_move = false;
        private int fps = 25;
        private List<long> list_timestamps; // this list contains the timestamp of each rally
        private ImageList imageList_screenshots;
        private Capture capture_review;
        private Capture capture;
        private int capture_device_index;
        private Boolean reviewercamera_mousedown = false;
        private Boolean reviewercamera_mousemoving = false;
        private int imagex = -100; // this variables are needed in the reviewer picturebox to draw the icon at position (imagex, imagey)
        private int imagey = -100;
        private int move_imagex = -100; // just for the drag and drop drawing
        private int move_imagey = -100;
        private Boolean togglemode = false;
        private int editing_position = 0; // the selected position in the listview
        private int live_video_click_count; // for the first and last position that can be tracked in live mode.
        private int codec;
        private String loaded_videopath;
        private double startmilisecond; // for the rally recording out of the mdeia player
        private double endmilisecond;
        private List<Image<Bgr, Byte>> rallyframes; // capture all frames first before writing them out
        /****************************/



        /******** Calibration stuff ***************/
        Calibration calibration;
        private int cmperpixel = 100;
        private int field_width = 95 * 100; // also change in configuration
        private int field_height = 192 * 100;
        private int outwidth = 50;
        private int outheight = 25;
        private Boolean show_calibration_lines = true;
        private Boolean calibrating = false;
        /******************************************/


        /******* Various control variables ********/
        // may vary from the configuration!!! 
        private Game game; // this is where all the data is saved (current_rally, rally_list, etc).

        
        private Boolean newgame; // So we can check in the FormClosing event what triggered the closing event


        // Xml 
        private XmlOutput xmlDoc;

        /************************************* END OF VARIABLE DEFINITIONS ********************************************************/





        public Form_Main(XmlOutput xmlDoc)
        {
            InitializeComponent();

            // Load default configuration
            configuration = Program.getConfiguration();
            applyConfiguration(configuration);


            newgame = false;


            this.game = new Game(configuration.Teama, configuration.Teamb);


            // add the first set
            this.game.Sets.Add(new Set(1));

            capture_stream = new CaptureStream();
            capture = Program.Capture;
           
            list_timestamps = new List<long>();
            imageList_screenshots = new ImageList();
            imageList_screenshots.ImageSize = new Size(84, 68);

            codec = Emgu.CV.CvInvoke.CV_FOURCC('P', 'I', 'M', '1');

            this.StartPosition = FormStartPosition.Manual;
            this.DesktopLocation = new Point(0, 0);

            this.xmlDoc = xmlDoc;

            xmlDoc.setGame(this.game);
        }


        private void Form_Main_Shown(object sender, EventArgs e)
        {
           
            xmlDoc.Configuration = configuration;
            xmlDoc.readXml();

            setCaptureDeviceIndex(capture_device_index);
  
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!newgame)
                Application.Exit();
            else
            {
                if (capture != null)
                    capture.Dispose();

                if (capture_review != null)
                    capture_review.Dispose();

                this.Dispose();
            }
        }


        
        /*************************************** TOOLSTRIP MENU STUFF ****************************************************/

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
  


            if (capture != null)
                capture.Dispose();

            if (capture_review != null)
                capture_review.Dispose();



            this.Dispose();

            newgame = true;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(SetupAssistant));
            thread.Start();
        }

        public void SetupAssistant()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SetupAssistant setupassistant = new SetupAssistant();
            Application.Run(setupassistant);
        }




        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configuration_form = new Form_Configuration(this);
            configuration_form.Show();
            configuration_form.BringToFront();
        }


        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {

            timer_review_capture.Stop();
            Application.Idle -= ProcessFrame;

            if (capture != null)
                capture.Dispose();

            if (capture_review != null)
                capture_review.Dispose();



            this.Dispose();

            System.Windows.Forms.Application.Exit();
    
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.StartPosition = FormStartPosition.Manual;

            about.DesktopLocation = new Point((Screen.PrimaryScreen.Bounds.Width / 2)-about.Width/2, (Screen.PrimaryScreen.Bounds.Height / 2)-about.Height/2);
            about.Show();
        }



        private void applyConfiguration(Configuration config)
        {
            if (config.TeamAup)
            {
                textBox_teamupname.Text = config.Teama.Name;
                textBox_teamdownname.Text = config.Teamb.Name;
                radioButton_playerupleft.Text = config.Teama.Player1.Name;
                radioButton_playerupright.Text = config.Teama.Player2.Name;
                radioButton_playerdownleft.Text = config.Teamb.Player1.Name;
                radioButton_playerdownright.Text = config.Teamb.Player2.Name;
            }
            else
            {
                textBox_teamupname.Text = config.Teamb.Name;
                textBox_teamdownname.Text = config.Teama.Name;
                radioButton_playerupleft.Text = config.Teamb.Player1.Name;
                radioButton_playerupright.Text = config.Teamb.Player2.Name;
                radioButton_playerdownleft.Text = config.Teama.Player1.Name;
                radioButton_playerdownright.Text = config.Teama.Player2.Name;
            }

        }


       

        /***************************** END OF XML STUFF ****************************************************************************/

        

        /***************************************************************************************************************************/
        /****************************************** GETTER AND SETTER METHODS ******************************************************/

        /*
         * returns the elapsed time since capture start in milliseconds
         *  
         * each move in the game and the corresponding video is uniqueliy identified by this timestamp
         * 
         * the stopwatch is invoked by the method which handles the play/stop
         */
        public long getCurrentTime()
        {
            DateTime timestamp = DateTime.Now;
            return timestamp.Ticks;
        }


        public PictureBox getBirdviewpicturebox() { return pictureBox_birdview; }
        public PictureBox getLivemodepicturebox() { return pictureBox_livevideo; }
        public PictureBox getReviewmodepicturebox() { return pictureBox_reviewercamera; }
        public ImageList getImageListScreenshots() { return imageList_screenshots; }
        public void setImageListScreenshots(ImageList list) { imageList_screenshots = list; }

        

        public void setLoadedVideoPath(String path)
        {
            loaded_videopath = path;
            capture_device_index = - 1;
            labelstatus_videosource.Text = "Loaded Video mode";
            startLoadedVideo(loaded_videopath);
            
        }

        public List<long> List_timestamps
        {
            get { return list_timestamps; }
            set { list_timestamps = value; }
        }

        public Game Game
        {
            get { return game; }
            set { game = value; }
        }

        /******************************** END OF GETTER AND SETTER METHODS ******************************************************/


        /******************************* MISC GUI STUFF ********************************************************/
        // During timeout, all buttons in Live mode should be disabled.
        private void disableAllLiveButtons(bool upperTimeoutButtonPressed)
        {
            button_drop.Enabled = false;
            button_bigPoint.Enabled = false;
            button_kill.Enabled = false;
            button_minusscoreteam1.Enabled = false;
            button_minusscoreteam2.Enabled = false;
            button_pluscoreteam1.Enabled = false;
            button_pluscoreteam2.Enabled = false;
            button_setminus.Enabled = false;
            button_setplus.Enabled = false;
            button_smash.Enabled = false;
            button_startmove.Enabled = false;
            button_switchplayersdown.Enabled = false;
            button_switchplayersup.Enabled = false;
            button_switchteams.Enabled = false;

            
        }

        // After timeout, enable all previously disabled buttons.
        private void enableAllLiveButtons(bool upperTimeoutButtonPressed)
        {
            button_drop.Enabled = true;
            button_bigPoint.Enabled = true;
            button_kill.Enabled = true;
            button_minusscoreteam1.Enabled = true;
            button_minusscoreteam2.Enabled = true;
            button_pluscoreteam1.Enabled = true;
            button_pluscoreteam2.Enabled = true;
            button_setminus.Enabled = true;
            button_setplus.Enabled = true;
            button_smash.Enabled = true;
            button_startmove.Enabled = true;
            button_switchplayersdown.Enabled = true;
            button_switchplayersup.Enabled = true;
            button_switchteams.Enabled = true;

          
        }





        /***************************************************************************************************************************/
        /******************************************** VIDEO STUFF *****************************************************************/
        /*
         * Called after the Setup, after the Configuration etc. It basically restarts the Live Video Capture
         */ 
        public void setCaptureDeviceIndex(int i)
        {
            
            capture_device_index = i;

            if (capture_device_index != -1) // If we are in Live Mode
            {
                labelstatus_videosource.Text = "Live mode";

                Application.Idle -= ProcessFrame;
                timer_review_capture.Stop();

                if (capture != null)
                    capture.Dispose();

                capture = new Capture(i);
                Application.Idle += ProcessFrame;

                pictureBox_livevideo.Visible = true;
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
                        if (nextFrame != null)
                        {

                            if (new_move)
                                buffer.Add(nextFrame.Clone());
                                //videoWriter.WriteFrame(nextFrame);

                            pictureBox_livevideo.Image = nextFrame.ToBitmap(pictureBox_livevideo.Width, pictureBox_livevideo.Height);
                            nextFrame = null;
                        }
                    }
                    catch (ArgumentException) { 
                        Console.WriteLine("EXCEPTION: ArgumentException in processframe");
                    }
                }
                catch (AccessViolationException) { Console.WriteLine("EXCEPTION: AccessViolationException"); }
            }
        }



     











        
        /*************************************** VIDEO STUFF END ******************************************************************/









        /**********************************************************************************************************************************/
        /************************************* CALIBRATION STUFF **************************************************************************/


        // This is called in the Configuration form uppon apply.
        public void setCalibration(Calibration c)
        {
            calibration = c;
            if (calibration != null)
            {
                toolStripStatusLabel_calibration.Text = "Camera is calibrated";
                toolStripStatusLabel_calibration.ForeColor = Color.Green;
                checkBox_calibrationlines.Enabled = true;
            }
            else
            {
                toolStripStatusLabel_calibration.Text = "Camera is not calibrated";
                toolStripStatusLabel_calibration.ForeColor = Color.Red;
                checkBox_calibrationlines.Enabled = false;
            }

            pictureBox_reviewercamera.Refresh(); // to draw the calibratio lines
        }



        private int[][] getActuallPositions(Rally rally)
        {
            int[][] positions = new int[6][];
            
            positions[0] = new int[2];
            positions[0][0] = rally.Service_pos.WorldX;
            positions[0][1] = rally.Service_pos.WorldY;

            positions[1] = new int[2];
            positions[1][0] = rally.Reception_pos.WorldX;
            positions[1][1] = rally.Reception_pos.WorldY;

            positions[2] = new int[2];
            positions[2][0] = rally.Set_pos.WorldX;
            positions[2][1] = rally.Set_pos.WorldY;

            positions[3] = new int[2];
            positions[3][0] = rally.Approach_pos.WorldX;
            positions[3][1] = rally.Approach_pos.WorldY;

            positions[4] = new int[2];
            positions[4][0] = rally.Takeoff_pos.WorldX;
            positions[4][1] = rally.Takeoff_pos.WorldY;

            positions[5] = new int[2];
            positions[5][0] = rally.Defence_pos.WorldX;
            positions[5][1] = rally.Defence_pos.WorldY;

            return positions;

        }

        // just to draw the separation line and the points
        private void pictureBox_birdview_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Draw the line
            Graphics g = e.Graphics;
            Pen field_pen = new Pen(Color.Gray, 1.0f);
            g.DrawRectangle(field_pen, new Rectangle(outwidth, outheight, field_width/cmperpixel, field_height/cmperpixel));
            g.DrawLine(field_pen, outwidth, outheight + ((field_height/cmperpixel) / 2 - 1), outwidth + (field_width/cmperpixel), outheight + (field_height/cmperpixel) / 2 - 1);

            // Draw the points
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            field_pen = new Pen(Color.Gray, 2.0f);

            if (Game.Reviewing_rally != null)
            {
                int[][] positions = getActuallPositions(Game.Reviewing_rally);
                for (int i = 0; i < 6; i++)
                {
                    if (positions[i] != null)
                    {
                        int[] points = positions[i];
                        int x = Math.Abs(outwidth * cmperpixel + points[0] * (field_width / 800)) / 100;
                        int y = Math.Abs(outheight * cmperpixel + points[1] * (field_height / 1600)) / 100;
                        if (points[0] != 0 && points[1] != 0)
                            g.DrawEllipse(field_pen, x - 2, y - 2, 4, 4);
                    }
                }
            }
            

        }


        /******************************************* END OF CALIBRATION STUFF ************************************************************************/








        /**********************************************************************************************************************************/
        /************************************* BUTTON FUNCTIONALITY STUFF **************************************************************************/

        // Tab CHANGE 
        // Used to save changes, made in the reviewer to XML output, so the user is not bothered
        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl_main.SelectedIndex == 0) // Begin Live Capture again
            {
                button_reviewerplaypause_Click(sender, e); // Reset all Reviewer Stuff

                if (capture_device_index!=-1)
                    Application.Idle += ProcessFrame;
                
            }
            else // Stop Live Capture
            {
                if (capture_device_index != -1)
                    Application.Idle -= ProcessFrame;
            }
        }


        // TODO set scores for both teams when set change
        private void teamXhasWon()
        {
            DialogResult result = MessageBox.Show("Team " + Game.getWinner().Name + " has won the game.", "Finish this game?", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                disableAllLiveButtons(true);
                //xmlDoc.addWinner(game.getWinner());
            }
        }

        private void button_setplus_Click(object sender, EventArgs e)
        {
            button_startmove.Enabled = true;
            button_setminus.Enabled = true;

            Set current_set = Game.Sets[Game.Sets.Count - 1];

            switch (Game.Sets.Count)
            {
                case 1:
                    button_setplus.Enabled = true;
                    Game.Sets.Add(new Set(Game.Sets.Count + 1));
                    current_set = Game.Sets[Game.Sets.Count - 1];

                    textBox_setcount.Text = Game.Sets.Count.ToString();
                    textBox_scoreteamup.Text = current_set.TeamBScore.ToString();
                    textBox_scoreteamdown.Text = current_set.TeamAScore.ToString();
                    setScoreColor(current_set);
                    break;

                case 2:
                    button_setplus.Enabled = false;
                    if (Game.getWinner() == null)
                    {
                        Game.Sets.Add(new Set(Game.Sets.Count + 1));
                        current_set = Game.Sets[Game.Sets.Count - 1];

                        textBox_setcount.Text = Game.Sets.Count.ToString();
                        textBox_scoreteamup.Text = current_set.TeamBScore.ToString();
                        textBox_scoreteamdown.Text = current_set.TeamAScore.ToString();
                        setScoreColor(current_set);
                        break;
                    }
                    else
                    {
                        teamXhasWon();
                        break;
                    }

                case 3:
                    button_setplus.Enabled = false;
                    if (Game.getWinner() == null)
                    {
                        teamXhasWon();
                    }
                    break;

                default:
                    // should not be reached
                    throw new Exception("DEBUG: No more sets can be created, there are already 3.");
            }
        }

        private void button_setminus_Click(object sender, EventArgs e)
        {
            button_startmove.Enabled = true;
            button_setplus.Enabled = true;

            Set current_set = Game.Sets[Game.Sets.Count - 1];

            switch (Game.Sets.Count)
            {
                case 1:
                    button_setminus.Enabled = false;
                    Game.Sets.RemoveAt(Game.Sets.Count - 1);
                    current_set = Game.Sets[Game.Sets.Count - 1];

                    textBox_setcount.Text = Game.Sets.Count.ToString();
                    textBox_scoreteamup.Text = current_set.TeamBScore.ToString();
                    textBox_scoreteamdown.Text = current_set.TeamAScore.ToString();
                    setScoreColor(current_set);
                    break;

                case 2:
                    goto case 1;

                case 3:
                    button_setminus.Enabled = true;
                    Game.Sets.RemoveAt(Game.Sets.Count - 1);
                    current_set = Game.Sets[Game.Sets.Count - 1];

                    textBox_setcount.Text = Game.Sets.Count.ToString();
                    textBox_scoreteamup.Text = current_set.TeamBScore.ToString();
                    textBox_scoreteamdown.Text = current_set.TeamAScore.ToString();
                    setScoreColor(current_set);
                    break;

                default:
                    // should not be reached
                    throw new Exception("DEBUG: No more sets can be created, there are already 3.");
            }
        }

        private void button_pluscoreteam1_Click(object sender, EventArgs e)
        {
            Set currentSet = Game.Sets[Game.Sets.Count - 1];

            button_minusscoreteam1.Enabled = true;

            if (!configuration.TeamAup)
            {
                currentSet.TeamBScore += 1;
                textBox_scoreteamup.Text = currentSet.TeamBScore.ToString();
            }
            else
            {
                currentSet.TeamAScore += 1;
                textBox_scoreteamup.Text = currentSet.TeamAScore.ToString();
            }
            setScoreColor(currentSet);
        }

        private void button_minusscoreteam1_Click(object sender, EventArgs e)
        {
            Set currentSet = Game.Sets[Game.Sets.Count - 1];

            if (!configuration.TeamAup)
            {
                currentSet.TeamBScore -= 1;
                textBox_scoreteamup.Text = currentSet.TeamBScore.ToString();
            }
            else
            {
                currentSet.TeamAScore -= 1;
                textBox_scoreteamup.Text = currentSet.TeamAScore.ToString();
            }

            if (int.Parse(textBox_scoreteamup.Text) <= 0)
            {
                button_minusscoreteam1.Enabled = false;
            }
            setScoreColor(currentSet);
        }

        private void button_pluscoreteam2_Click(object sender, EventArgs e)
        {
            Set currentSet = Game.Sets[Game.Sets.Count - 1];

            button_minusscoreteam2.Enabled = true;

            if (configuration.TeamAup)
            {
                currentSet.TeamBScore += 1;
                textBox_scoreteamdown.Text = currentSet.TeamBScore.ToString();
            }
            else
            {
                currentSet.TeamAScore += 1;
                textBox_scoreteamdown.Text = currentSet.TeamAScore.ToString();
            }
            setScoreColor(currentSet);
        }

        private void button_minusscoreteam2_Click(object sender, EventArgs e)
        {
            Set currentSet = Game.Sets[Game.Sets.Count - 1];

            if (configuration.TeamAup)
            {
                currentSet.TeamBScore -= 1;
                textBox_scoreteamdown.Text = currentSet.TeamBScore.ToString();
            }
            else
            {
                currentSet.TeamAScore -= 1;
                textBox_scoreteamdown.Text = currentSet.TeamAScore.ToString();
            }

            setScoreColor(currentSet);

            if (int.Parse(textBox_scoreteamdown.Text) <= 0)
            {
                button_minusscoreteam2.Enabled = false;
            }
        }

        private void setScoreColor(Set currentSet)
        {
            if (currentSet.isSetWon())
            {
                if (int.Parse(textBox_scoreteamdown.Text) > int.Parse(textBox_scoreteamup.Text))
                {
                    textBox_scoreteamdown.ForeColor = Color.Green;
                    textBox_scoreteamup.ForeColor = Color.Gray;
                }
                else
                {
                    textBox_scoreteamdown.ForeColor = Color.Gray;
                    textBox_scoreteamup.ForeColor = Color.Green;
                }
            }
            else
            {
                textBox_scoreteamdown.ForeColor = Color.Gray;
                textBox_scoreteamup.ForeColor = Color.Gray;
            }
        }


        // Re-mapps the current positions of the players on click switch teams
        private void switchTeams_AssignCurrentPosition(Person p)
        {
            switch (p.Current_position)
            { 
                case Rally.PlayerPosition.lowerLeft:
                    p.Current_position = Rally.PlayerPosition.upperLeft;
                    break;
                
                case Rally.PlayerPosition.lowerRight:
                    p.Current_position = Rally.PlayerPosition.upperRight;
                    break;

                case Rally.PlayerPosition.upperLeft:
                    p.Current_position = Rally.PlayerPosition.lowerLeft;
                    break;

                case Rally.PlayerPosition.upperRight:
                    p.Current_position = Rally.PlayerPosition.lowerRight;
                    break;

                default:
                    // should not be reached
                    throw new Exception("DEBUG: PlayerPosition can not be set.");
            }
        }
        
        // TODO picturebox change
        private void button_switchteams_Click(object sender, EventArgs e)
        {
            // update configuration
            switchTeams_AssignCurrentPosition(configuration.Teama.Player1);
            switchTeams_AssignCurrentPosition(configuration.Teama.Player2);
            switchTeams_AssignCurrentPosition(configuration.Teamb.Player1);
            switchTeams_AssignCurrentPosition(configuration.Teamb.Player2);

            configuration.TeamAup = !configuration.TeamAup;

            // Adjust labels
            string tempName = textBox_teamupname.Text;
            string tempLabelLeft = radioButton_playerupleft.Text;
            string tempLabelRight = radioButton_playerupright.Text;

            string tempscore = textBox_scoreteamup.Text;
            textBox_scoreteamup.Text = textBox_scoreteamdown.Text;
            textBox_scoreteamdown.Text = tempscore;


            textBox_teamupname.Text = textBox_teamdownname.Text;
            textBox_teamdownname.Text = tempName;

            radioButton_playerupleft.Text = radioButton_playerdownleft.Text;
            radioButton_playerupright.Text = radioButton_playerdownright.Text;
            radioButton_playerdownleft.Text = tempLabelLeft;
            radioButton_playerdownright.Text = tempLabelRight;
        }


        // Re-mapps the current positions of the players on player switch
        private void switchPlayers_AssignCurrentPosition(Person p)
        {
            switch (p.Current_position)
            {
                case Rally.PlayerPosition.lowerLeft:
                    p.Current_position = Rally.PlayerPosition.lowerRight;
                    break;

                case Rally.PlayerPosition.lowerRight:
                    p.Current_position = Rally.PlayerPosition.lowerLeft;
                    break;

                case Rally.PlayerPosition.upperLeft:
                    p.Current_position = Rally.PlayerPosition.upperRight;
                    break;

                case Rally.PlayerPosition.upperRight:
                    p.Current_position = Rally.PlayerPosition.upperLeft;
                    break;

                default:
                    // should not be reached
                    throw new Exception("DEBUG: PlayerPosition can not be set.");
            }
        }

        private void button_switchplayersup_Click(object sender, EventArgs e)
        {
            if (configuration.TeamAup)
            {
                switchPlayers_AssignCurrentPosition(configuration.Teama.Player1);
                switchPlayers_AssignCurrentPosition(configuration.Teama.Player2);
            }
            else
            {
                switchPlayers_AssignCurrentPosition(configuration.Teamb.Player1);
                switchPlayers_AssignCurrentPosition(configuration.Teamb.Player2);
            }

            // Adjust labels
            string tempName = radioButton_playerupright.Text;

            radioButton_playerupright.Text = radioButton_playerupleft.Text;
            radioButton_playerupleft.Text = tempName;
        }

        private void button_switchplayersdown_Click(object sender, EventArgs e)
        {
            if (configuration.TeamAup)
            {
                switchPlayers_AssignCurrentPosition(configuration.Teamb.Player1);
                switchPlayers_AssignCurrentPosition(configuration.Teamb.Player2);
            }
            else
            {
                switchPlayers_AssignCurrentPosition(configuration.Teama.Player1);
                switchPlayers_AssignCurrentPosition(configuration.Teama.Player2);
            }
            
            
            // Adjust labels
            string tempName = radioButton_playerdownright.Text;

            radioButton_playerdownright.Text = radioButton_playerdownleft.Text;
            radioButton_playerdownleft.Text = tempName;
        }


       


        private void button_startmove_Click(object sender, EventArgs e)
        {
            long start_time;
     

            // initiating a new move along with a new timestamp as identifier
            if (!new_move)
            {
                live_video_click_count = 0;

                // Enable the Spielzug/Move property buttons
                button_kill.Enabled = true;
                button_smash.Enabled = true;
                button_drop.Enabled = true;
                button_bigPoint.Enabled = true;
                button_timeout.Enabled = true;

                radioButton_playerupright.Enabled = true;
                radioButton_playerupleft.Enabled = true;
                radioButton_playerdownleft.Enabled = true;
                radioButton_playerdownright.Enabled = true;

                radioButton_playerupright.Checked = false;
                radioButton_playerupleft.Checked = false;
                radioButton_playerdownleft.Checked = false;
                radioButton_playerdownright.Checked = false;
                

                start_time = getCurrentTime(); // get current time as identifier
                while (List_timestamps.Contains(start_time))
                    start_time = getCurrentTime();
                
                List_timestamps.Add(start_time); // add timestamp to the list we use for the screenshots

                // Create a new Rally 
                Game.Current_rally = 
                    new Rally(configuration.Teama.Player1.Current_position,
                              configuration.Teama.Player2.Current_position,
                              configuration.Teamb.Player1.Current_position,
                              configuration.Teamb.Player2.Current_position,
                              start_time, Game.Sets.Count);

                
   

                // Clear the BirdView
                pictureBox_birdview.Invalidate();

                rallyframes = new List<Image<Bgr, byte>>();

                
                String move_identifier = start_time.ToString();
                String videopath = Program.getConfiguration().Mediafolderpath + @"\" + move_identifier + ".mpg";
                this.videoWriter = new VideoWriter(videopath, Emgu.CV.CvInvoke.CV_FOURCC('P', 'I', 'M', '1'), 25, 640, 480, true);


                // start a new video capture from video
                if (capture_device_index == -1)
                {
                    Capture tempcapture = new Capture(loaded_videopath);
                    int tempfps = (int)tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);
                    this.videoWriter = new VideoWriter(videopath, Emgu.CV.CvInvoke.CV_FOURCC('P', 'I', 'M', '1'), tempfps, 640, 480, true);
                    startmilisecond = axWindowsMediaPlayer_live.Ctlcontrols.currentPosition;
                    axWindowsMediaPlayer_live.Ctlcontrols.play();
                    tempcapture.Dispose();
                }


                button_startmove.Text = "End of rally";
                button_startmove.ForeColor = System.Drawing.Color.Red;
                new_move = true;
            }
            else
            {
                live_video_click_count = 0;

                // Disable the Spielzug/Move property buttons
                button_kill.Enabled = false;
                button_smash.Enabled = false;
                button_drop.Enabled = false;
                button_bigPoint.Enabled = false;
                button_timeout.Enabled = false;

                radioButton_playerupright.Enabled = false;
                radioButton_playerupleft.Enabled = false;
                radioButton_playerdownleft.Enabled = false;
                radioButton_playerdownright.Enabled = false;

                radioButton_playerupright.Checked = false;
                radioButton_playerupleft.Checked = false;
                radioButton_playerdownleft.Checked = false;
                radioButton_playerdownright.Checked = false;

                // AUTO handling of score
                // Save into the list and add to xml output
                if (Game.Current_rally != null)
                {
                    Set current_set = Game.Sets[Game.Sets.Count - 1];
                    current_set.Rallies.Add(Game.Current_rally);
                    

                    // Set End Time
                    Game.Current_rally.EndRally_time = getCurrentTime();
                    Game.Current_rally.Duration_ticks = Game.Current_rally.EndRally_time - Game.Current_rally.Start_time;

                    // calculate the point for the successful team
                    Game.Current_rally.setNewScore(Game, configuration.TeamAup);
                    

                    xmlDoc.addRally(Game.Current_rally);


                    if (Game.Current_rally.Kill)
                        button_kill.Text = "KILL";
                    else
                        button_kill.Text = "NO KILL";

                    if (configuration.TeamAup)
                    {
                        textBox_scoreteamup.Text = current_set.TeamAScore.ToString();
                        textBox_scoreteamdown.Text = current_set.TeamBScore.ToString();
                    }
                    else
                    {
                        textBox_scoreteamup.Text = current_set.TeamBScore.ToString();
                        textBox_scoreteamdown.Text = current_set.TeamAScore.ToString();
                    }
                    // set color 
                    setScoreColor(current_set);
                    Team winner = current_set.getSetWinner(Game);

                    if (winner != null)
                    {
                        teamXhasWon();
                    }
                }


                

                // stop the capturing and write video
                if (capture_device_index != -1) // camera capture
                {
                    start_time = Game.Current_rally.Start_time;
                    WriteRallyVideoThread writevideoobject = new WriteRallyVideoThread(buffer, videoWriter, start_time);
                    writevideoobject.donewritingrallyvideo += new DoneWritingRallyVideoEventHandler(writevideothread_donewriting);
                    writeRallyVideoFromBuffer(writevideoobject);
                }
                else // loaded video
                {
                    endmilisecond = axWindowsMediaPlayer_live.Ctlcontrols.currentPosition;
                    start_time = Game.Current_rally.Start_time;
                    WriteRallyVideoThread writevideoobject = new WriteRallyVideoThread(startmilisecond, endmilisecond, loaded_videopath, videoWriter, start_time);
                    writevideoobject.donewritingrallyvideo += new DoneWritingRallyVideoEventHandler(writevideothread_donewriting);
                    writeRallyVideoFromLoaded(writevideoobject);
                    
                }


                button_startmove.Text = "Start of rally…"; // SAVE
                button_startmove.ForeColor = System.Drawing.Color.Black;
                new_move = false;

            }
        }


        // This is called when a WriteVideoThread thread throws a DoneWritingRallyVideoEvent
        private void writevideothread_donewriting(object sender, DoneWritingRallyVideoEventArgs e)
        {
            long videoid = e.videoID();
            createScreeshot(videoid);
        }


        


        private void createScreeshot(long start_time)
        {
            Bitmap screenshot = capture_stream.createScreenshot(start_time, false);
            setScreeshot(screenshot);
        }

        private delegate void AddScreenshotToListViewEventHandler(Bitmap screenshot);
        private void setScreeshot(Bitmap screenshot)
        {
            if (this.listView_screenshots.InvokeRequired)
            {
                this.listView_screenshots.Invoke(new AddScreenshotToListViewEventHandler(this.setScreeshot), screenshot);
            }
            else
            {
                if (screenshot != null)
                {

                    imageList_screenshots.Images.Add(screenshot);

                    int screenshot_index = listView_screenshots.Items.Count;
                    ListViewItem screenshot_item = new ListViewItem("", screenshot_index);

                    listView_screenshots.Items.Add(screenshot_item);


                    listView_screenshots.LargeImageList = imageList_screenshots;

                    listView_screenshots.Refresh();

                    listView_screenshots.EnsureVisible(listView_screenshots.Items.Count - 1);

                }
            }

        }













        private void button_bigPoint_Click(object sender, EventArgs e)
        {
            Game.Current_rally.BigPoint = !Game.Current_rally.BigPoint;
            if (button_bigPoint.Text == "NO BIG POINT")
            { button_bigPoint.Text = "BIG POINT"; }
            else
            { button_bigPoint.Text = "NO BIG POINT"; }

        }

        private void button_kill_Click(object sender, EventArgs e)
        {
            Game.Current_rally.Kill = !Game.Current_rally.Kill;
            if (button_kill.Text == "KILL")
            { button_kill.Text = "NO KILL"; }
            else
            { button_kill.Text = "KILL"; }
        }

        private void button_smash_Click(object sender, EventArgs e)
        {
            Game.Current_rally.Smash = !Game.Current_rally.Smash;
            if (button_smash.Text == "SMASH")
            { button_smash.Text = "SHOT"; }
            else
            { button_smash.Text = "SMASH"; }
        }

        private void button_drop_Click(object sender, EventArgs e)
        {
            Game.Current_rally.Drop = !Game.Current_rally.Drop;
            if (button_drop.Text == "NO DROP")
            { button_drop.Text = "DROP"; }
            else
            { button_drop.Text = "NO DROP"; }
        }


        private void button_timeout_Click(object sender, EventArgs e)
        {
            Game.Current_rally.Timeout = !Game.Current_rally.Timeout;
            if (button_timeout.Text == "TIMEOUT")
            { button_timeout.Text = "NO TIMEOUT"; }
            else
            { button_timeout.Text = "TIMEOUT"; }
        }


        private void radioButton_playerupleft_CheckedChanged(object sender, EventArgs e)
        {
            if (new_move)
            {
                if (configuration.TeamAup)
                    Game.Current_rally.Reception_pos.Player = configuration.Teama.Player1;
                else
                    Game.Current_rally.Reception_pos.Player = configuration.Teamb.Player1;
            }
        }

        private void radioButton_playerupright_CheckedChanged(object sender, EventArgs e)
        {
            if (new_move)
            {
                if (configuration.TeamAup)
                    Game.Current_rally.Reception_pos.Player = configuration.Teama.Player2;
                else
                    Game.Current_rally.Reception_pos.Player = configuration.Teamb.Player2;
            }
        }

        private void radioButton_playerdownleft_CheckedChanged(object sender, EventArgs e)
        {
            if (new_move)
            {
                if (!configuration.TeamAup)
                    Game.Current_rally.Reception_pos.Player = configuration.Teama.Player1;
                else
                    Game.Current_rally.Reception_pos.Player = configuration.Teamb.Player1;
            }
        }

        private void radioButton_playerdownright_CheckedChanged(object sender, EventArgs e)
        {
            if (new_move)
            {
                if (!configuration.TeamAup)
                    Game.Current_rally.Reception_pos.Player = configuration.Teama.Player2;
                else
                    Game.Current_rally.Reception_pos.Player = configuration.Teamb.Player2;
            }
        }




        private void pictureBox_livevideo_Click(object sender, EventArgs e)
        {
            int imagex = ((MouseEventArgs)e).X;
            int imagey = ((MouseEventArgs)e).Y;
            int wx = 0;
            int wy = 0;

            // Draw the Icon at the positions location
            Graphics g = pictureBox_livevideo.CreateGraphics();

            Pen field_pen;
            field_pen = new Pen(Color.Red, 3.0f);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawLine(field_pen, new Point(imagex - 5, imagey - 5), new Point(imagex + 5, imagey + 5));
            g.DrawLine(field_pen, new Point(imagex + 5, imagey - 5), new Point(imagex - 5, imagey + 5));
            


            if (calibration != null)
            {
                PointF drawPoint3D = calibration.Calibrate(imagex, imagey);

                int x = (int)drawPoint3D.X;
                int y = (int)drawPoint3D.Y;


                // world x and word y
                wx = (int)(x / (field_width / 800));
                wy = (int)(y / (field_height / 1600));
            }

            if (new_move && live_video_click_count == 0)
            {
                Game.Current_rally.Service_pos.ImageX = imagex;
                Game.Current_rally.Service_pos.ImageY = imagey;
                Game.Current_rally.Service_pos.WorldX = wx;
                Game.Current_rally.Service_pos.WorldY = wy;
                live_video_click_count++;
            }
            else
            if (new_move && live_video_click_count == 1)
            {
                button_startmove_Click(sender, e);
                Game.Current_rally.Defence_pos.ImageX = imagex;
                Game.Current_rally.Defence_pos.ImageY = imagey;
                Game.Current_rally.Defence_pos.WorldX = wx;
                Game.Current_rally.Defence_pos.WorldY = wy;
            }
            else
            if (!new_move)
            {
                button_startmove_Click(sender, e);
                Game.Current_rally.Service_pos.ImageX = imagex;
                Game.Current_rally.Service_pos.ImageY = imagey;
                Game.Current_rally.Service_pos.WorldX = wx;
                Game.Current_rally.Service_pos.WorldY = wy;
                live_video_click_count = 1; ;
            }

        }


        private void axWindowsMediaPlayer_live_ClickEvent(object sender, AxWMPLib._WMPOCXEvents_ClickEvent e)
        {
            int imagex = e.fX - axWindowsMediaPlayer_live.Location.X;
            int imagey = e.fY + axWindowsMediaPlayer_live.Location.Y;
            int wx = 0;
            int wy = 0;

            // Draw the Icon at the positions location
            Graphics g = pictureBox_livevideo.CreateGraphics();

            Pen field_pen;
            field_pen = new Pen(Color.Red, 3.0f);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawLine(field_pen, new Point(imagex - 5, imagey - 5), new Point(imagex + 5, imagey + 5));
            g.DrawLine(field_pen, new Point(imagex + 5, imagey - 5), new Point(imagex - 5, imagey + 5));



            if (calibration != null)
            {
                PointF drawPoint3D = calibration.Calibrate(imagex, imagey);

                int x = (int)drawPoint3D.X;
                int y = (int)drawPoint3D.Y;


                // world x and word y
                wx = (int)(x / (field_width / 800));
                wy = (int)(y / (field_height / 1600));
            }

            if (new_move && live_video_click_count == 0)
            {
                Game.Current_rally.Service_pos.ImageX = imagex;
                Game.Current_rally.Service_pos.ImageY = imagey;
                Game.Current_rally.Service_pos.WorldX = wx;
                Game.Current_rally.Service_pos.WorldY = wy;
                live_video_click_count++;
            }
            else
                if (new_move && live_video_click_count == 1)
                {
                    button_startmove_Click(sender, null);
                    Game.Current_rally.Defence_pos.ImageX = imagex;
                    Game.Current_rally.Defence_pos.ImageY = imagey;
                    Game.Current_rally.Defence_pos.WorldX = wx;
                    Game.Current_rally.Defence_pos.WorldY = wy;
                }
                else
                    if (!new_move)
                    {
                        button_startmove_Click(sender, null);
                        Game.Current_rally.Service_pos.ImageX = imagex;
                        Game.Current_rally.Service_pos.ImageY = imagey;
                        Game.Current_rally.Service_pos.WorldX = wx;
                        Game.Current_rally.Service_pos.WorldY = wy;
                        live_video_click_count = 1; ;
                    }
        }


        private void writeRallyVideoFromLoaded(double s, double e, VideoWriter writer, String loadedvideopath)
        {
            double start = Math.Floor(s);
            double end = Math.Ceiling(e);
            double startmsec = start * 1000;
            double endmsec = end * 1000;


            Capture tempcapture = new Capture(loaded_videopath);

            Image<Bgr, Byte> frame;
            if (tempcapture != null)
            {
                //tempcapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC, start);

                double fps2 = tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);
                //tempcapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC, 100);

                for (int i = 0; i < (start * fps2); i++)
                    (tempcapture).QueryFrame();

                int durationframes = (int)((end - start) * fps2); // since c# sucks i have to do it manually just like any other crap

                int count = 0;
                while (count < durationframes)
                {
                    frame = (tempcapture).QueryFrame();
                    videoWriter.WriteFrame(frame);
                    count++;
                }
            }


            tempcapture.Dispose();
            videoWriter.Dispose();
        }


        /*
         * This method writes the video part form start-end in video file out of the loaded game video
         */
        private void writeRallyVideoFromLoaded(WriteRallyVideoThread writevideothread)
        { 
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(writevideothread.write));
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
        }


        private void writeRallyVideoFromBuffer(WriteRallyVideoThread writevideothread)
        {
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(writevideothread.writeFromBuffer));
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
        }

        /**************************************************************************************************************************************/
        /********************************************* REVIEWER STUFF *************************************************************************/
        


        /*
         * This method gets called on refresh() when we want to display the
         * position icon at a certain position
         */
        private void pictureBox_reviewercamera_Paint(object sender, PaintEventArgs e)
        {
            // Draw the Icon at the positions location
            Graphics g = e.Graphics;

            Pen field_pen;

            // Draw the point (A cross)
            if (!togglemode)
                field_pen = new Pen(Color.Red, 3.0f);
            else
                field_pen = new Pen(Color.Green, 3.0f);



            if (reviewercamera_mousemoving)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(field_pen, new Point(move_imagex - 5, move_imagey - 5), new Point(move_imagex + 5, move_imagey + 5));
                g.DrawLine(field_pen, new Point(move_imagex + 5, move_imagey - 5), new Point(move_imagex - 5, move_imagey + 5));
            }
            else
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawLine(field_pen, new Point(imagex - 5, imagey - 5), new Point(imagex + 5, imagey + 5));
                g.DrawLine(field_pen, new Point(imagex + 5, imagey - 5), new Point(imagex - 5, imagey + 5));
            }


            // draw calibration lines
            if (show_calibration_lines || calibrating)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen pen = new Pen(Color.Red, 3.0f);

                if (corner_count == 2)
                    g.DrawLine(pen, corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);

                if (corner_count == 3)
                {
                    g.DrawLine(pen, corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);
                    g.DrawLine(pen, corners[1].X, corners[1].Y, corners[2].X, corners[2].Y);
                }

                if (corner_count >= 4)
                {
                    g.DrawLine(pen, corners[0].X, corners[0].Y, corners[1].X, corners[1].Y);
                    g.DrawLine(pen, corners[1].X, corners[1].Y, corners[2].X, corners[2].Y);
                    g.DrawLine(pen, corners[2].X, corners[2].Y, corners[3].X, corners[3].Y);
                    g.DrawLine(pen, corners[3].X, corners[3].Y, corners[0].X, corners[0].Y);
                }
            }
        }


        
        private void pictureBox_reviewercamera_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            // We dont want to draw if we dont have a calibration.
            if (calibration != null && Game.Reviewing_rally != null && togglemode)
                reviewercamera_mousedown = true;
        }


        /*
         * Draw the position Icon along the Mouse cursor
         */
        private void pictureBox_reviewercamera_MouseMove(object sender, MouseEventArgs e)
        {
            if (reviewercamera_mousedown && togglemode)
            {
                reviewercamera_mousemoving = true;
                move_imagex = e.X;
                move_imagey = e.Y;
                pictureBox_reviewercamera.Refresh(); // draw the icon at the new position
            }
        }


        /* 
         * Here does the point tracking happen.
         * On release save the position we released the mouse.
         */
        private void pictureBox_reviewercamera_MouseUp(object sender, MouseEventArgs e)
        {
            reviewercamera_mousedown = false;
            reviewercamera_mousemoving = false;
        }



        private void pictureBox_reviewercamera_Click(object sender, EventArgs e)
        {
            if (!calibrating)
            {
                int mx = ((MouseEventArgs)e).X;
                int my = ((MouseEventArgs)e).Y;

                // first check if we have a position icon (cross) there. if we do toggle the cross
                // else proceed in drawing and recording a new position
                if (mx >= imagex - 5 && mx <= imagex + 5 && my >= imagey - 5 && my <= imagey + 5 && !togglemode)
                {
                    togglemode = true;
                    pictureBox_reviewercamera.Refresh();

                    //disable play button
                    button_reviewerplaypause.Enabled = false;
                }
                else
                    if (mx >= imagex - 5 && mx <= imagex + 5 && my >= imagey - 5 && my <= imagey + 5 && togglemode) // jump off toggle mode and save
                    {
                        togglemode = false;
                        button_reviewerplaypause.Enabled = true;
                    }

                    else // we are still in toggle mode, but we are just clicking other positions. Draw them but dont save them
                        if (togglemode)
                        {
                            imagex = mx;
                            imagey = my;
                            pictureBox_reviewercamera.Refresh(); // draw the icon at the new position
                        }


                if (calibration != null && Game.Reviewing_rally != null)
                {

                    //Only save when we exit toggle mode
                    if (!togglemode)
                    {
                        savePosition(mx, my);


                        imagex = mx;
                        imagey = my;
                        pictureBox_reviewercamera.Refresh(); // draw the icon at the new position



                        //select position in the list
                        if (editing_position+1 < 6)
                            listView_reviewertrackedpoints.Items[editing_position+1].Selected = true;
                        listView_reviewertrackedpoints.Focus();
                    }

                }

            }


            if (calibrating)
            {
                corner_count = corner_count + 1;

                // Not for drawing, but for the transformation
                if (corner_count > 4)
                {
                    Graphics g = pictureBox_reviewercamera.CreateGraphics();
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    Pen field_pen = new Pen(Color.Red, 2.0f);
                    g.DrawEllipse(field_pen, ((MouseEventArgs)e).X - 2, ((MouseEventArgs)e).Y - 2, 4, 4);
                }

                // For the field corners
                if (corner_count <= 4)
                {
                    // 1. up left, 2. up right, 3. down right, 4. down left
                    corners[corner_count - 1] = new PointF(((MouseEventArgs)e).X, ((MouseEventArgs)e).Y);
                    progressBar_calibrationsteps.Value = progressBar_calibrationsteps.Value + 10;
                    showCalibrationStep(corner_count + 1);
                }
            }
        }


       
        private void savePosition(int mx, int my)
        {
            togglemode = false;

            PointF drawPoint3D = calibration.Calibrate(mx, my);
            Graphics g = pictureBox_birdview.CreateGraphics();
            g.Clear(Color.LemonChiffon);


            // draw field and line in BirdView again
            Pen field_pen = new Pen(Color.Gray, 1.0f);
            g.DrawRectangle(field_pen, new Rectangle(outwidth, outheight, field_width / cmperpixel, field_height / cmperpixel));
            g.DrawLine(field_pen, outwidth, outheight + ((field_height / cmperpixel) / 2 - 1), outwidth + (field_width / cmperpixel), outheight + (field_height / cmperpixel) / 2 - 1);


            // Draw point in Birdview
            
            int x = (int)drawPoint3D.X;
            int y = (int)drawPoint3D.Y;
            

            // world x and word y
            int wx = (int)(x / (field_width / 800));
            int wy = (int)(y / (field_height / 1600));
            
            Person player = new Person();

            long position_time = 0;
            if (capture_review != null)
            {
                /*
                for (int i = 0; i < List_timestamps.Count-1; i++)
                {

                    String move_identifier = List_timestamps[i].ToString();
                    String videopath = Program.getConfiguration().Mediafolderpath + @"\" + move_identifier + ".mpg";
                    Capture temp = new Capture(videopath);
                    double framecount = temp.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
                    temp.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, framecount);
                    position_time = position_time + (long)temp.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC);
                }
                 * */
                position_time = position_time + (long)capture_review.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC);

            }
            if (position_time < 0)
                position_time = 0;


            switch (editing_position)
            {
                case 0:
                    Game.Reviewing_rally.Service_pos = new Position(wx, wy, 0, mx, my, position_time, player);
                    break;
                case 1:
                    Game.Reviewing_rally.Reception_pos = new Position(wx, wy, 0, mx, my, position_time, player);
                    break;
                case 2:
                    Game.Reviewing_rally.Set_pos = new Position(wx, wy, 0, mx, my, position_time, player);
                    break;
                case 3:
                    Game.Reviewing_rally.Approach_pos = new Position(wx, wy, 0, mx, my, position_time, player);
                    break;
                case 4:
                    Game.Reviewing_rally.Takeoff_pos = new Position(wx, wy, 0, mx, my, position_time, player);
                    break;
                case 5:
                    Game.Reviewing_rally.Defence_pos = new Position(wx, wy, 0, mx, my, position_time, player);
                    break;
                default:
                    break;
            }

            updatePositionList(); // display the new tracked positions
            xmlDoc.updateReviewRally(Game.Reviewing_rally);
            pictureBox_reviewercamera.Refresh();
            pictureBox_birdview.Refresh();
        }


        private void listView_reviewertrackedpoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (togglemode)
                savePosition(imagex, imagey);

            // Decide which click belongs to what property (serve, reception, set, takeoff, defence, kill
            if (listView_reviewertrackedpoints.SelectedIndices.Count > 0)
                editing_position = listView_reviewertrackedpoints.SelectedIndices[0];

        }




        private void listView_screenshots_Click(object sender, EventArgs e)
        {
            ListView listview = (ListView)sender;




            if (listview.SelectedItems.Count > 0)
            {
                if (listview.SelectedItems[0] != null)
                {
                    int index = listview.SelectedItems[0].ImageIndex; // get index of the screenshot

                    if (index < list_timestamps.Count && index>=0)
                    {
                        long start_time = list_timestamps[index]; // get timestamp associated with the screenshot
                        // search for the rally
                        Rally temp = null;
                        for (int i = 0; i < Game.Sets.Count; i++)
                            for (int c = 0; c < Game.Sets[i].Rallies.Count; c++)
                            {
                                temp = Game.Sets[i].Rallies[c];
                                if (temp.Start_time == start_time)
                                {
                                    loadReviewingRally(temp);
                                }
                            }
                        
                    }
                }
            }
        }


        private void timer_review_capture_Tick(object sender, EventArgs e)
        {
            try
            {
                if (capture_review != null)
                {
                    using (Image<Bgr, Byte> nextFrame = this.capture_review.QueryFrame())
                    {
                        if (nextFrame != null)
                        {
                            pictureBox_reviewercamera.Image = nextFrame.ToBitmap();
                            //changes the trackbar position
                            if (trackBar_reviewervideo.Value < trackBar_reviewervideo.Maximum)
                                trackBar_reviewervideo.Value++;
                        }
                        else // no next frame
                        {
                            trackBar_reviewervideo.Value = trackBar_reviewervideo.Maximum;
                        }
                    }
                }
            }
            catch (AccessViolationException) { Console.WriteLine("EXCEPTION: AccessViolationException"); }
        }

        /*
         * Switch between slow motion and normal speed
         */ 
        private void checkBox_review_slow_motion_CheckedChanged(object sender, EventArgs e)
        {

            // So we wont need two timers
            if (checkBox_review_slow_motion.Checked)
                timer_review_capture.Interval = 160;
            else
                timer_review_capture.Interval = 30;
        }


        


        /************************************* REVIEWER VIDEO TRACKBAR *******************************************************/
        private Boolean trackbar_mousedown = false;
        private void trackBar_reviewervideo_ValueChanged(object sender, EventArgs e)
        {
            TrackBar trackbar = (TrackBar)sender;

            if (!trackbar_mousedown) // if the mouse (dragging) changes the value don't load frames
            {

                if (this.capture_stream != null)
                {

                    updateVideoPlayTime();

                    if (trackbar.Value >= trackbar.Maximum) // we reached the end, reset everything
                    {
                        button_reviewerplaypause.Text = "Play";
                        timer_review_capture.Stop();
                        trackbar.Value = 0;
                        pictureBox_reviewercamera.Refresh();
                        this.capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
                        Image<Bgr, Byte> nextFrame = this.capture_review.QueryFrame();
                        if (nextFrame != null)
                            pictureBox_reviewercamera.Image = nextFrame.ToBitmap();

                    }
                }
            }
        }


        /*
         * This function updates the reviewing video play time based
         * on the TrackBar value
         */ 
        private void updateVideoPlayTime()
        {
            TrackBar trackbar = trackBar_reviewervideo;

            int value = trackbar.Value; // in frames
            int min = (int)value / fps / 60;
            int sec = value / fps - min * 60;

            string minutes = "";
            string seconds = "";

            if (min < 10)
                minutes = "0";
            if (sec < 10)
                seconds = "0";

            label_reviewerplaytime.Text = minutes + min + ":" + seconds + sec;
        }


        private void trackBar_reviewervideo_Scroll(object sender, EventArgs e)
        {
            updateVideoPlayTime();   
        }


        private void trackBar_reviewervideo_MouseDown(object sender, MouseEventArgs e)
        {
            trackbar_mousedown = true;
            button_reviewerplaypause_Click(sender, e);
        }

        private void trackBar_reviewervideo_MouseUp(object sender, MouseEventArgs e)
        {
            trackbar_mousedown = false;

            int frame = trackBar_reviewervideo.Value;
            //double msec = (frame / fps ) * 1000;
            //capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC,msec);
            capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, frame);


            for (int i = 1; i < fps; i++)
            {
                Image<Bgr, Byte> nextFrame = this.capture_review.QueryFrame();
                if (nextFrame != null)
                    pictureBox_reviewercamera.Image = nextFrame.ToBitmap();
            }
            pictureBox_reviewercamera.Refresh();

            //button_reviewerplaypause_Click(sender, e);
        }



        private void button_reviewerplaypause_Click(object sender, EventArgs e)
        {
            timer_review_capture.Stop();

            if (button_reviewerplaypause.Text=="Play" && !trackbar_mousedown)
            {
                if (capture_review != null)
                {
                    int frame = trackBar_reviewervideo.Value;
                    capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, frame);
                    button_reviewerplaypause.Text = "Pause";
                    timer_review_capture.Start();
                }
            }
            else
            {
                button_reviewerplaypause.Text = "Play"; 
            }

            listView_reviewertrackedpoints.Focus(); // so we can see what position is selected
        }


        private void buttonreviewer_next_Click(object sender, EventArgs e)
        {
            
            int value = trackBar_reviewervideo.Value + 5;
            if (value <= trackBar_reviewervideo.Maximum)
                trackBar_reviewervideo.Value = value;
            else
                trackBar_reviewervideo.Value = trackBar_reviewervideo.Maximum;

            double frame = trackBar_reviewervideo.Value;

            capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, frame);


            for (int i = 1; i < 5; i++)
            {
                Image<Bgr, Byte> nextFrame = this.capture_review.QueryFrame();
                if (nextFrame != null)
                    pictureBox_reviewercamera.Image = nextFrame.ToBitmap();
            }


            pictureBox_reviewercamera.Refresh();
            listView_reviewertrackedpoints.Focus();
        }

        private void buttonreviewer_previous_Click(object sender, EventArgs e)
        {
           
            int value = trackBar_reviewervideo.Value - 5;
            if (value >= trackBar_reviewervideo.Minimum)
                trackBar_reviewervideo.Value = value;
            else
                trackBar_reviewervideo.Value = 0;

            double frame = trackBar_reviewervideo.Value;

            capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, frame);
            for (int i = 1; i < 5; i++)
            {
                Image<Bgr, Byte> nextFrame = this.capture_review.QueryFrame();
                if (nextFrame != null)
                    pictureBox_reviewercamera.Image = nextFrame.ToBitmap();
            }

            pictureBox_reviewercamera.Refresh();
            listView_reviewertrackedpoints.Focus();
        }




        /*
         * This function loads the Rally that is to be reviewed.
         * That is, it sets its frame position to 00:00, Enables the control buttons in the reviewer, etc
         */
        private void loadReviewingRally(Rally rally)
        {

            Game.Reviewing_rally = rally;

            if (Game.Reviewing_rally != null)
            {
                

                pictureBox_birdview.Refresh();

                updatePositionList();

                // Enable control buttons
                panel_reviewervideocontrols.Enabled = true;
                panel_reviewerpositions.Enabled = true;

                if (Game.Reviewing_rally.Finalized)
                    button_reviewerfinalized.Text = "Unfinalize";
                else
                    button_reviewerfinalized.Text = "Finalize";



                // Reception Player Radiobuttons
                if (Game.Reviewing_rally.Reception_pos.Player == configuration.Teama.Player1)
                    radioButton_reviewerplayer1.Checked = true;

                if (Game.Reviewing_rally.Reception_pos.Player == configuration.Teama.Player2)
                    radioButton_reviewerplayer2.Checked = true;

                if (Game.Reviewing_rally.Reception_pos.Player == configuration.Teamb.Player1)
                    radioButton_reviewerplayer3.Checked = true;

                if (Game.Reviewing_rally.Reception_pos.Player == configuration.Teamb.Player2)
                    radioButton_reviewerplayer4.Checked = true;

                radioButton_reviewerplayer1.Text = configuration.Teama.Player1.Name;
                radioButton_reviewerplayer2.Text = configuration.Teama.Player2.Name;
                radioButton_reviewerplayer3.Text = configuration.Teamb.Player1.Name;
                radioButton_reviewerplayer4.Text = configuration.Teamb.Player2.Name;


                // Defence player checkboxes:
                List<Person> defenceplayers = Game.Reviewing_rally.Reception_pos.Defenceplayers;
                checkBox_defenceplayer1.Checked = defenceplayers.Contains(configuration.Teama.Player1);
                checkBox_defenceplayer2.Checked = defenceplayers.Contains(configuration.Teama.Player2);
                checkBox_defenceplayer3.Checked = defenceplayers.Contains(configuration.Teamb.Player1);
                checkBox_defenceplayer4.Checked = defenceplayers.Contains(configuration.Teamb.Player2);

                


                // here we load the video for the first time in the reviewer at frame=0
                String video_name = Game.Reviewing_rally.Start_time.ToString(); 
                try
                {
                    capture_review = null; // delete the old one
                    String videopath = Program.getConfiguration().Mediafolderpath + @"\" + video_name + ".mpg";
                    this.capture_review = new Capture(videopath);
                    double fps = capture_review.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);
                    if (fps > 0)
                    {
                        int interval = (int)Math.Ceiling((1000 / fps)) - 3;
                        timer_review_capture.Interval = interval;
                    }
                    else
                    {
                        fps = 25;
                        timer_review_capture.Interval = 40;
                    }
                }
                catch (NullReferenceException) { Console.WriteLine("unreadable video file"); }
                

                // Count the frames in the video
                double frame_number = 0;
                if (capture_review != null)
                {
                    this.capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
                    while (capture_review.QueryFrame() != null)
                        frame_number++;

                    this.capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
                }
                trackBar_reviewervideo.Maximum = (int)frame_number;
                trackBar_reviewervideo.Value = 0;

                

                // If all the loading is done we display the right tab
                tabControl_main.SelectedIndex = 1;  // switch to review-tab


                // After we loaded all the stuff we can finally play the video:
                button_reviewerplaypause.Text = "Pause";
                timer_review_capture.Start();


                if (calibration != null)
                {
                    if (game.Reviewing_rally.Service_pos.ImageX != 0 && game.Reviewing_rally.Service_pos.ImageY != 0)
                    {
                        editing_position = 0;
                        savePosition(game.Reviewing_rally.Service_pos.ImageX, game.Reviewing_rally.Service_pos.ImageY);
                    }

                    if (game.Reviewing_rally.Defence_pos.ImageX != 0 && game.Reviewing_rally.Defence_pos.ImageY != 0)
                    {
                        editing_position = 5;
                        savePosition(game.Reviewing_rally.Defence_pos.ImageX, game.Reviewing_rally.Defence_pos.ImageY);
                    }
                }



                //selectnextEmptyPosition();
                // Select first index
                listView_reviewertrackedpoints.Items[0].Selected = true;
                listView_reviewertrackedpoints.Focus();
            }
        }


        /*
         * This function updates the Tracked position list with the current 
         * coordinates and timestamps of the tracked positions.
         */ 
        private void updatePositionList()
        {
            // LOAD THE POSITION LIST
            listView_reviewertrackedpoints.Items.Clear();

            ListViewItem item;
            //TimeSpan play_time;
            double play_time;
            String positiontime;
            String positiontext;


            item = new ListViewItem();
            item.Text = "Service";
            positiontext = "(" + Game.Reviewing_rally.Service_pos.WorldX + "," + Game.Reviewing_rally.Service_pos.WorldY + ")";
            item.SubItems.Add(positiontext);     

            play_time = TimeSpan.FromMilliseconds(Game.Reviewing_rally.Service_pos.PositionTime).TotalMilliseconds;
            positiontime = play_time.ToString();
            //positiontime = String.Format("{0:mm\\:ss}", play_time);
            item.SubItems.Add(positiontime);

            listView_reviewertrackedpoints.Items.Add(item);



            item = new ListViewItem();
            item.Text = "Reception";
            positiontext = "(" + Game.Reviewing_rally.Reception_pos.WorldX + "," + Game.Reviewing_rally.Reception_pos.WorldY + ")";
            item.SubItems.Add(positiontext);

            play_time = TimeSpan.FromMilliseconds(Game.Reviewing_rally.Reception_pos.PositionTime).TotalMilliseconds;
            positiontime = play_time.ToString();
            //positiontime = String.Format("{0:mm\\:ss}", play_time);
            item.SubItems.Add(positiontime);

            listView_reviewertrackedpoints.Items.Add(item);




            item = new ListViewItem();
            item.Text = "Setting";
            positiontext = "(" + Game.Reviewing_rally.Set_pos.WorldX + "," + Game.Reviewing_rally.Set_pos.WorldY + ")";
            item.SubItems.Add(positiontext);

            play_time = TimeSpan.FromMilliseconds(Game.Reviewing_rally.Set_pos.PositionTime).TotalMilliseconds;
            positiontime = play_time.ToString();
            //positiontime = String.Format("{0:mm\\:ss}", play_time);
            item.SubItems.Add(positiontime);

            listView_reviewertrackedpoints.Items.Add(item);



            item = new ListViewItem();
            item.Text = "Approach";
            positiontext = "(" + Game.Reviewing_rally.Approach_pos.WorldX + "," + Game.Reviewing_rally.Approach_pos.WorldY + ")";
            item.SubItems.Add(positiontext);

            play_time = TimeSpan.FromMilliseconds(Game.Reviewing_rally.Approach_pos.PositionTime).TotalMilliseconds;
            positiontime = play_time.ToString();
            //positiontime = String.Format("{0:mm\\:ss}", play_time);
            item.SubItems.Add(positiontime);

            listView_reviewertrackedpoints.Items.Add(item);



            item = new ListViewItem();
            item.Text = "Takeoff";
            positiontext = "(" + Game.Reviewing_rally.Takeoff_pos.WorldX + "," + Game.Reviewing_rally.Takeoff_pos.WorldY + ")";
            item.SubItems.Add(positiontext);

            play_time = TimeSpan.FromMilliseconds(Game.Reviewing_rally.Takeoff_pos.PositionTime).TotalMilliseconds;
            positiontime = play_time.ToString();
            //positiontime = String.Format("{0:mm\\:ss}", play_time);
            item.SubItems.Add(positiontime);

            listView_reviewertrackedpoints.Items.Add(item);


            item = new ListViewItem();
            item.Text = "Defence";
            positiontext = "(" + Game.Reviewing_rally.Defence_pos.WorldX + "," + Game.Reviewing_rally.Defence_pos.WorldY + ")";
            item.SubItems.Add(positiontext);

            play_time = TimeSpan.FromMilliseconds(Game.Reviewing_rally.Defence_pos.PositionTime).TotalMilliseconds;
            positiontime = play_time.ToString();
            //positiontime = String.Format("{0:mm\\:ss}", play_time);
            item.SubItems.Add(positiontime);

            listView_reviewertrackedpoints.Items.Add(item);


            listView_reviewertrackedpoints.Refresh();
        }


        
 
        /******************************************************************************************************/



        /************************************* LIST VIEW TRACKED POSITIONS ************************************/
        
       

        private void listView_reviewertrackedpoints_DoubleClick(object sender, EventArgs e)
        {
            int position = listView_reviewertrackedpoints.SelectedIndices[0];
            long position_time=0;
            switch (position)
            {
                case 0:
                    position_time = Game.Reviewing_rally.Service_pos.PositionTime;
                    imagex = Game.Reviewing_rally.Service_pos.ImageX;
                    imagey = Game.Reviewing_rally.Service_pos.ImageY;
                    break;
                case 1:
                    position_time = Game.Reviewing_rally.Reception_pos.PositionTime;
                    imagex = Game.Reviewing_rally.Reception_pos.ImageX;
                    imagey = Game.Reviewing_rally.Reception_pos.ImageY;
                    break;
                case 2:
                    position_time = Game.Reviewing_rally.Set_pos.PositionTime;
                    imagex = Game.Reviewing_rally.Set_pos.ImageX;
                    imagey = Game.Reviewing_rally.Set_pos.ImageY;
                    break;
                case 3:
                    position_time = Game.Reviewing_rally.Approach_pos.PositionTime;
                    imagex = Game.Reviewing_rally.Approach_pos.ImageX;
                    imagey = Game.Reviewing_rally.Approach_pos.ImageY;
                    break;
                case 4:
                    position_time = Game.Reviewing_rally.Takeoff_pos.PositionTime;
                    imagex = Game.Reviewing_rally.Takeoff_pos.ImageX;
                    imagey = Game.Reviewing_rally.Takeoff_pos.ImageY;
                    break;
                case 5:
                    position_time = Game.Reviewing_rally.Defence_pos.PositionTime;
                    imagex = Game.Reviewing_rally.Defence_pos.ImageX;
                    imagey = Game.Reviewing_rally.Defence_pos.ImageY;
                    break;
                default:
                    break;
            }

            if (capture_review != null)
            {
                timer_review_capture.Stop();
                button_reviewerplaypause.Text = "Play";
                
                // Set the video to that time
                this.capture_review.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC, (double)position_time);
                
                // Set the trackbar value to that Frame
                trackBar_reviewervideo.Value = (int) (((position_time / 1000) * fps) % trackBar_reviewervideo.Maximum);
                
                for (int i = 1; i < fps; i++)
                {
                    Image<Bgr, Byte> nextFrame = this.capture_review.QueryFrame();
                    if (nextFrame != null)
                        pictureBox_reviewercamera.Image = nextFrame.ToBitmap();
                }
                
                // Draw the Icon at the positions location
                pictureBox_reviewercamera.Refresh();
            }
        }



        private void button_deletepositions_Click(object sender, EventArgs e)
        {
            Game.Reviewing_rally.Service_pos.ImageX = 0;
            Game.Reviewing_rally.Service_pos.ImageY = 0;
            Game.Reviewing_rally.Service_pos.WorldX = 0;
            Game.Reviewing_rally.Service_pos.WorldY = 0;
            Game.Reviewing_rally.Service_pos.PositionTime = 0;

            Game.Reviewing_rally.Reception_pos.ImageX = 0;
            Game.Reviewing_rally.Reception_pos.ImageY = 0;
            Game.Reviewing_rally.Reception_pos.WorldX = 0;
            Game.Reviewing_rally.Reception_pos.WorldY = 0;
            Game.Reviewing_rally.Reception_pos.PositionTime = 0;

            Game.Reviewing_rally.Set_pos.ImageX = 0;
            Game.Reviewing_rally.Set_pos.ImageY = 0;
            Game.Reviewing_rally.Set_pos.WorldX = 0;
            Game.Reviewing_rally.Set_pos.WorldY = 0;
            Game.Reviewing_rally.Set_pos.PositionTime = 0;

            Game.Reviewing_rally.Approach_pos.ImageX = 0;
            Game.Reviewing_rally.Approach_pos.ImageY = 0;
            Game.Reviewing_rally.Approach_pos.WorldX = 0;
            Game.Reviewing_rally.Approach_pos.WorldY = 0;
            Game.Reviewing_rally.Approach_pos.PositionTime = 0;

            Game.Reviewing_rally.Takeoff_pos.ImageX = 0;
            Game.Reviewing_rally.Takeoff_pos.ImageY = 0;
            Game.Reviewing_rally.Takeoff_pos.WorldX = 0;
            Game.Reviewing_rally.Takeoff_pos.WorldY = 0;
            Game.Reviewing_rally.Takeoff_pos.PositionTime = 0;

            Game.Reviewing_rally.Defence_pos.ImageX = 0;
            Game.Reviewing_rally.Defence_pos.ImageY = 0;
            Game.Reviewing_rally.Defence_pos.WorldX = 0;
            Game.Reviewing_rally.Defence_pos.WorldY = 0;
            Game.Reviewing_rally.Defence_pos.PositionTime = 0;

            updatePositionList(); // display the new tracked positions
            xmlDoc.updateReviewRally(Game.Reviewing_rally);
            pictureBox_reviewercamera.Refresh();
            pictureBox_birdview.Refresh();
            listView_reviewertrackedpoints.Items[0].Selected = true;
            listView_reviewertrackedpoints.Focus();
            
        }

        /******************************************************************************************************/



       



        /******************************* REVIEWER GUI STUFF ********************************************************/
        

        private void Form_Main_Resize(object sender, EventArgs e)
        {
            int x;
            int width;

            // The panel on the left
            width = pictureBox_reviewercamera.Location.X - panel_reviewerleft.Location.X;
            panel_reviewerleft.Size = new Size(width, panel_reviewerleft.Size.Height);

            // The panel on the right
            x = (pictureBox_reviewercamera.Location.X + pictureBox_reviewercamera.Size.Width);
            panel_reviewerright.Location = new Point(x, panel_reviewerright.Location.Y);
            width = (groupBox_reviewerbackground.Location.X + groupBox_reviewerbackground.Size.Width) - x - 5;
            panel_reviewerright.Size = new Size(width, panel_reviewerright.Size.Height);

            // Stretch the list tracked points colunms
            width = listView_reviewertrackedpoints.Size.Width;
            if (listView_reviewertrackedpoints.Columns[0] != null && listView_reviewertrackedpoints.Columns[1] != null)
            {
                listView_reviewertrackedpoints.Columns[0].Width = width / 2 - 2;
                listView_reviewertrackedpoints.Columns[1].Width = width / 2 - 2;
            }

        }



/*******************************************************************************************************************************/
/*************************************************LOADED VIDEO IN LIVE MODE*****************************************************/
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e) {
            if (capture != null)
            {
                using (Image<Bgr, Byte> nextFrame = capture.QueryFrame())
                {
                    if (nextFrame != null)
                    {
                        if (capture_device_index == -1 && new_move)
                            videoWriter.WriteFrame(nextFrame);
                    }
                }
            }
        
        }


        private void timer_live_loadedvideo_Tick(object sender, EventArgs e)
        {
            if (capture != null)
            {
                using (Image<Bgr, Byte> nextFrame = capture.QueryFrame())
                {
                    if (nextFrame != null)
                    {
                        if (capture_device_index == -1 && new_move)
                            videoWriter.WriteFrame(nextFrame);
                    }
                }
            }
        }


        // Load the video
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // here we load the video for the first time in the reviewer at frame=0
            String videopath = "";
            if (openVideoDialog.ShowDialog() == DialogResult.OK)
            {
                videopath = openVideoDialog.FileName;

                timer_review_capture.Stop();
                Application.Idle -= ProcessFrame;

                startLoadedVideo(videopath);
            }
        }


        private void startLoadedVideo(String videopath)
        {
     
            axWindowsMediaPlayer_live.Visible = true;
            axWindowsMediaPlayer_live.URL = @videopath;
            //timer_live_loadedvideo2.Start();
        }






  


     

       





        private void checkBox_calibrationlines_CheckedChanged(object sender, EventArgs e)
        {
            if (show_calibration_lines)
            {
                show_calibration_lines = false;
            }
            else
            {
                show_calibration_lines = true;
            }
            pictureBox_reviewercamera.Refresh();
            listView_reviewertrackedpoints.Focus();
        }

        /*
         * When double click on the bird view: clear all drawed positions
         */ 
        private void pictureBox_birdview_DoubleClick(object sender, EventArgs e)
        {
            pictureBox_birdview.Refresh();
        }


/********************************************* SETUP CALIBRATION CODE *************************************************************/
        private PointF[] corners;// the 4 field corners assoicated with the world coordinates
        private int corner_count;

        private void showNotCalibrated()
        {
            Font myFont = new Font("Arial", 10);
            Graphics g = pictureBox_birdview.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.DrawString("No Calibration!", myFont, Brushes.Red, new Point(2, 2));

        }

        private void button_newcalibration_Click(object sender, EventArgs e)
        {
            calibrating = true;
            corner_count = 0;
            corners = new PointF[4];
            showCalibrationStep(1);
            pictureBox_reviewercamera.Refresh();
        }

        public void showCalibrationStep(int step)
        {

            if (step == 0) // reset all the calibration stuff
            {

                toolStripStatusLabel_calibration.Text = "not calibrated";
                toolStripStatusLabel_calibration.ForeColor = Color.Red;

                label_calibrationstep1.ForeColor = Color.Black;
                label_calibrationstep2.ForeColor = Color.Black;
                label_calibrationstep3.ForeColor = Color.Black;
                label_calibrationstep4.ForeColor = Color.Black;


                label_calibrationstep1.Enabled = false;
                label_calibrationstep2.Enabled = false;
                label_calibrationstep3.Enabled = false;
                label_calibrationstep4.Enabled = false;

                progressBar_calibrationsteps.Visible = false;
                progressBar_calibrationsteps.Value = 0;

                pictureBox_arrowstep1.Visible = false;
                pictureBox_arrowstep2.Visible = false;
                pictureBox_arrowstep3.Visible = false;
                pictureBox_arrowstep4.Visible = false;

                corner_count = 0;
                corners = new PointF[4];


            }

            if (step == 1)
            {

                toolStripStatusLabel_calibration.Text = "not calibrated";
                toolStripStatusLabel_calibration.ForeColor = Color.Red;

                label_calibrationstep1.ForeColor = Color.Black;
                label_calibrationstep2.ForeColor = Color.Black;
                label_calibrationstep3.ForeColor = Color.Black;
                label_calibrationstep4.ForeColor = Color.Black;


                //show calibration steps (only the first)
                label_calibrationstep1.Enabled = true;
                label_calibrationstep2.Enabled = false;
                label_calibrationstep3.Enabled = false;
                label_calibrationstep4.Enabled = false;

                progressBar_calibrationsteps.Visible = true;
                progressBar_calibrationsteps.Value = 0;

                pictureBox_arrowstep1.Visible = true;
                pictureBox_arrowstep2.Visible = false;
                pictureBox_arrowstep3.Visible = false;
                pictureBox_arrowstep4.Visible = false;

            }

            if (step == 2)
            {
                label_calibrationstep1.Enabled = false;
                label_calibrationstep2.Enabled = true;
                label_calibrationstep3.Enabled = false;
                label_calibrationstep4.Enabled = false;

                progressBar_calibrationsteps.Visible = true;

                pictureBox_arrowstep1.Visible = false;
                pictureBox_arrowstep2.Visible = true;
                pictureBox_arrowstep3.Visible = false;
                pictureBox_arrowstep4.Visible = false;

            }

            if (step == 3)
            {
                label_calibrationstep1.Enabled = false;
                label_calibrationstep2.Enabled = false;
                label_calibrationstep3.Enabled = true;
                label_calibrationstep4.Enabled = false;

                progressBar_calibrationsteps.Visible = true;

                pictureBox_arrowstep1.Visible = false;
                pictureBox_arrowstep2.Visible = false;
                pictureBox_arrowstep3.Visible = true;
                pictureBox_arrowstep4.Visible = false;

                pictureBox_reviewercamera.Refresh();

            }


            if (step == 4)
            {
                label_calibrationstep1.Enabled = false;
                label_calibrationstep2.Enabled = false;
                label_calibrationstep3.Enabled = false;
                label_calibrationstep4.Enabled = true;

                progressBar_calibrationsteps.Visible = true;

                pictureBox_arrowstep1.Visible = false;
                pictureBox_arrowstep2.Visible = false;
                pictureBox_arrowstep3.Visible = false;
                pictureBox_arrowstep4.Visible = true;

                pictureBox_reviewercamera.Refresh();
            }

            if (step == 5) //finished calibration
            {
                label_calibrationstep1.Enabled = true;
                label_calibrationstep2.Enabled = true;
                label_calibrationstep3.Enabled = true;
                label_calibrationstep4.Enabled = true;

                label_calibrationstep1.ForeColor = Color.Green;
                label_calibrationstep2.ForeColor = Color.Green;
                label_calibrationstep3.ForeColor = Color.Green;
                label_calibrationstep4.ForeColor = Color.Green;

                progressBar_calibrationsteps.Visible = false;

                pictureBox_arrowstep1.Visible = false;
                pictureBox_arrowstep2.Visible = false;
                pictureBox_arrowstep3.Visible = false;
                pictureBox_arrowstep4.Visible = false;

                draw_cornerlines_incalibrationbox(corners[2], corners[3]);
                draw_cornerlines_incalibrationbox(corners[3], corners[0]);
                toolStripStatusLabel_calibration.Text = "calibrated";
                toolStripStatusLabel_calibration.ForeColor = Color.Green;
                calibrating = false;

                if (corner_count == 4) // create a new calibration on the first time
                    calibration = createNewCalibration(pictureBox_birdview.Size, new Size(640, 480));


                


                if (game.Reviewing_rally.Service_pos.ImageX != 0 && game.Reviewing_rally.Service_pos.ImageY != 0)
                {
                    editing_position = 0;
                    savePosition(game.Reviewing_rally.Service_pos.ImageX, game.Reviewing_rally.Service_pos.ImageY);
                }

                if (game.Reviewing_rally.Defence_pos.ImageX != 0 && game.Reviewing_rally.Defence_pos.ImageY != 0)
                {
                    editing_position = 5;
                    savePosition(game.Reviewing_rally.Defence_pos.ImageX, game.Reviewing_rally.Defence_pos.ImageY);
                }

                listView_reviewertrackedpoints.Focus();
            }

        }



        




        public Calibration createNewCalibration(Size birdview_size, Size videoframe_size)
        {

            Calibration new_calibration = new Calibration(videoframe_size);

            PointF point = corners[0]; // up left 
            new_calibration.addMapping(point.X, point.Y, 0, 0, 0);

            point = corners[1]; // up right
            new_calibration.addMapping(point.X, point.Y, field_width, 0, 0);

            point = corners[2]; // down right
            new_calibration.addMapping(point.X, point.Y, field_width, field_height, 0);

            point = corners[3]; // down left
            new_calibration.addMapping(point.X, point.Y, 0, field_height, 0);

            return new_calibration;
        }






        public void draw_cornerlines_incalibrationbox(PointF point1, PointF point2)
        {
            // draw the lines of the field
            Graphics g = pictureBox_reviewercamera.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen pen = new Pen(Color.Red, 3.0f);
            g.DrawLine(pen, point1.X, point1.Y, point2.X, point2.Y);
        }









/************************************************* REVIEWER PROPERTIES ****************************************************/




        private void button_reviewerkill_Click(object sender, EventArgs e)
        {
            Game.Reviewing_rally.Kill = !Game.Reviewing_rally.Kill;
            if (button_reviewerkill.Text == "KILL")
            { button_reviewerkill.Text = "NO KILL"; }
            else
            { button_reviewerkill.Text = "KILL"; }

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        }


        private void button_reviewersmash_Click(object sender, EventArgs e)
        {
            Game.Reviewing_rally.Smash = !Game.Reviewing_rally.Smash;
            if (button_reviewersmash.Text == "SMASH")
            { button_reviewersmash.Text = "SHOT"; }
            else
            { button_reviewersmash.Text = "SMASH"; }

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        
        }

        private void button_reviewerdrop_Click(object sender, EventArgs e)
        {
            Game.Reviewing_rally.Drop = !Game.Reviewing_rally.Drop;
            if (button_reviewerdrop.Text == "NO DROP")
            { button_reviewerdrop.Text = "DROP"; }
            else
            { button_reviewerdrop.Text = "NO DROP"; }

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        }

        private void button_reviewerbigpoint_Click(object sender, EventArgs e)
        {
            Game.Reviewing_rally.BigPoint = !Game.Reviewing_rally.BigPoint;
            if (button_reviewerbigpoint.Text == "NO BIG POINT")
            { button_reviewerbigpoint.Text = "BIG POINT"; }
            else
            { button_reviewerbigpoint.Text = "NO BIG POINT"; }

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        }

        private Person getReceptionPlayer()
        {
            Person player = configuration.Teama.Player1;

            if (radioButton_reviewerplayer1.Checked == true)
                player = configuration.Teama.Player1;

            if (radioButton_reviewerplayer2.Checked == true)
                player = configuration.Teama.Player2;

            if (radioButton_reviewerplayer3.Checked == true)
                player = configuration.Teamb.Player1;

            if (radioButton_reviewerplayer4.Checked == true)
                player = configuration.Teamb.Player2;

            return player;
        }

        private void radioButton_reviewerplayer1_CheckedChanged(object sender, EventArgs e)
        {
            
            Person player = getReceptionPlayer();
            Game.Reviewing_rally.Reception_pos.Player = player;
            if (player == configuration.Teama.Player1 || player == configuration.Teama.Player2) // brauchen wir eigentlich ned
            { // TeamA is reception team
                if ((player.defaultposition == Person.DefaultPosition.Left && player.Current_position == Rally.PlayerPosition.upperLeft) ||
                    (player.defaultposition == Person.DefaultPosition.Right && player.Current_position == Rally.PlayerPosition.upperRight))
                    Game.Reviewing_rally.Standardposition = true;
                else
                    Game.Reviewing_rally.Standardposition = false;
            }

            if (player == configuration.Teamb.Player1 || player == configuration.Teamb.Player2)
            {// TeamB is reception team
                if ((player.defaultposition == Person.DefaultPosition.Left && player.Current_position == Rally.PlayerPosition.upperLeft) ||
                    (player.defaultposition == Person.DefaultPosition.Right && player.Current_position == Rally.PlayerPosition.upperRight))
                    Game.Reviewing_rally.Standardposition = true;
                else
                    Game.Reviewing_rally.Standardposition = false;
            }

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        }


        private void checkBox_defenceplayer1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = ((CheckBox)sender);
            List<Person> defenceplayers = new List<Person>();
            if (checkBox_defenceplayer1.Checked)
                defenceplayers.Add(configuration.Teama.Player1);

            if (checkBox_defenceplayer2.Checked)
                defenceplayers.Add(configuration.Teama.Player2);

            if (checkBox_defenceplayer3.Checked)
                defenceplayers.Add(configuration.Teamb.Player1);

            if (checkBox_defenceplayer4.Checked)
                defenceplayers.Add(configuration.Teamb.Player2);

            if (checkbox.Checked)
                checkbox.Text = "YES";
            else
                checkbox.Text = "NO";

            Game.Reviewing_rally.Reception_pos.Defenceplayers = defenceplayers;

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        }



        private void button_reviewerfinalized_Click(object sender, EventArgs e)
        {
            Boolean finalized;
            if (button_reviewerfinalized.Text == "Finalize")
            {
                finalized = true;
                button_reviewerfinalized.Text = "Unfinalize";
            }
            else
            {
                finalized = false;
                button_reviewerfinalized.Text = "Finalize";
            }

            Game.Reviewing_rally.Finalized = finalized;

            Bitmap screenshot = capture_stream.createScreenshot(Game.Reviewing_rally.Start_time, finalized);
            if (screenshot != null)
            {
                // get index:
                int index = 0;
                for (int i = 0; i < List_timestamps.Count; i++)
                    if (List_timestamps[i] == Game.Reviewing_rally.Start_time)
                    {
                        index = i;
                        break;
                    }



                imageList_screenshots.Images[index] = screenshot;
                listView_screenshots.LargeImageList = imageList_screenshots;
                listView_screenshots.Refresh();
            }

            xmlDoc.updateReviewRally(Game.Reviewing_rally);
        }


        private void button_deleterally_Click(object sender, EventArgs e)
        {
            timer_review_capture.Stop();

            if (capture_review!=null)
                capture_review.Dispose();

            // get index:
            int screenshotindex = 0;
            for (int i = 0; i < List_timestamps.Count; i++)
                if (List_timestamps[i] == Game.Reviewing_rally.Start_time)
                {
                    xmlDoc.removeNode(Game.Reviewing_rally.Start_time);

                    screenshotindex = i;
                    List_timestamps.RemoveAt(screenshotindex);

                    imageList_screenshots.Images.RemoveAt(screenshotindex);
                    listView_screenshots.LargeImageList = imageList_screenshots;
                    
                    // Update the listview item screenshot indeces
                    listView_screenshots.Items.RemoveAt(screenshotindex);
                    for (int c=screenshotindex; c<listView_screenshots.Items.Count; c++)
                        listView_screenshots.Items[c] = new ListViewItem("", c);

                    listView_screenshots.Refresh();
                    if (listView_screenshots.Items.Count > 0)
                        listView_screenshots.EnsureVisible(listView_screenshots.Items.Count - 1);


                    int indexinset = Game.Sets[Game.Reviewing_rally.CorrespondingSetNr - 1].Rallies.IndexOf(Game.Reviewing_rally);

                    Game.Sets[Game.Reviewing_rally.CorrespondingSetNr - 1].Rallies.RemoveAt(indexinset);


                    tabControl_main.SelectedIndex = 0;

                    break;
                }
            
        }

        private void setRalliesAbsoluteStartTimes()
        {
            // Set the absolute times for all rallies
            long sum_times = 0; // to get the absolute start time of this rally.
            for (int i = 0; i < Game.Sets.Count; i++)
                for (int r = 0; r < Game.Sets[i].Rallies.Count; r++)
                {
                    Game.Sets[i].Rallies[r].Absolute_start_time = sum_times;
                    sum_times += (long)TimeSpan.FromTicks(Game.Sets[i].Rallies[r].Duration_ticks).TotalMilliseconds;
                    Game.Sets[i].Rallies[r].Absolute_end_time = sum_times;

                    // Update position times
                    Game.Sets[i].Rallies[r].Approach_pos.Absolute_PositionTime = Game.Sets[i].Rallies[r].Absolute_start_time + Game.Sets[i].Rallies[r].Approach_pos.PositionTime;
                    Game.Sets[i].Rallies[r].Defence_pos.Absolute_PositionTime = Game.Sets[i].Rallies[r].Absolute_start_time + Game.Sets[i].Rallies[r].Defence_pos.PositionTime;
                    Game.Sets[i].Rallies[r].Reception_pos.Absolute_PositionTime = Game.Sets[i].Rallies[r].Absolute_start_time + Game.Sets[i].Rallies[r].Reception_pos.PositionTime;
                    Game.Sets[i].Rallies[r].Service_pos.Absolute_PositionTime = Game.Sets[i].Rallies[r].Absolute_start_time + Game.Sets[i].Rallies[r].Service_pos.PositionTime;
                    Game.Sets[i].Rallies[r].Set_pos.Absolute_PositionTime = Game.Sets[i].Rallies[r].Absolute_start_time + Game.Sets[i].Rallies[r].Set_pos.PositionTime;
                    Game.Sets[i].Rallies[r].Start_pos.Absolute_PositionTime = Game.Sets[i].Rallies[r].Absolute_start_time + Game.Sets[i].Rallies[r].Start_pos.PositionTime;
                    xmlDoc.updateReviewRally(Game.Sets[i].Rallies[r]);
                }
        }

      

        private void exportVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {

            SaveFileDialog savefiledialog = new SaveFileDialog();
            savefiledialog.FileName = "Game "+configuration.GameID;
            DialogResult result = STAShowSaveDialog(savefiledialog);

            if (result == DialogResult.OK)
            {

                        string videopath = savefiledialog.FileName + ".mpg";

                        // write export xml
                        setRalliesAbsoluteStartTimes();
                        ExportXML exportxml = new ExportXML(savefiledialog.FileName, Game);
                        for (int i = 0; i < Game.Sets.Count; i++)
                            for (int r = 0; r < Game.Sets[i].Rallies.Count; r++)
                                exportxml.addRally(Game.Sets[i].Rallies[r]);



                /*
                        using (ITimeline timeline = new DefaultTimeline())
                        {
                            IGroup group = timeline.AddVideoGroup(32, 640, 480);


                            string firstVideoFilePath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[0].ToString() + ".mpg";
                            var firstVideoClip = group.AddTrack().AddVideo(firstVideoFilePath);


                            for (int i = 1; i < list_timestamps.Count; i++)
                            {
                                string secondVideoFilePath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[i].ToString() + ".mpg";
                                var secondVideoClip = group.AddTrack().AddVideo(secondVideoFilePath, firstVideoClip.Duration);
                                firstVideoClip = secondVideoClip;
                            }

                            using (AviFileRenderer renderer = new AviFileRenderer(timeline, videopath))
                            {

                                renderer.Render();
                                //progressBar_status.Value++;
                            }

                        }
                
                    */

                    
                    ExportVideoThread exportvideothread = new ExportVideoThread(savefiledialog.FileName + ".mpg", list_timestamps);
                    exportvideothread.DoneAppendingRallyVideoEvent += new ExportVideoEventHandler(this.updateProgressbarEventFired);
                    System.Threading.Thread t = new System.Threading.Thread(exportvideothread.write);
                    t.SetApartmentState(System.Threading.ApartmentState.STA);
                    t.Start();
                    

                    
                     
            }
        }



        // This gets called when the export video thread was done appending a rally video
        private delegate void UpdateProgressbarEventHandler(int value);
        private void updateProgressbarEventFired(object sender, ExportVideoProgressEventArgs e)
        {
            updateProgressbar(e.State());
        }


        private void updateProgressbar(int value)
        {
            if (this.progressBar_status.InvokeRequired)
            {
                this.progressBar_status.Invoke(new UpdateProgressbarEventHandler(this.updateProgressbar), value);
            }
            else
            {
                progressBar_status.Visible = true;
                progressBar_status.Maximum = list_timestamps.Count;
                progressBar_status.Value = value+1;
                if (progressBar_status.Value == progressBar_status.Maximum)
                    progressBar_status.Visible = false;
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

        private void button_backtolive_Click(object sender, EventArgs e)
        {
            tabControl_main.SelectedIndex = 0;
        }





        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OpenFileDialog openfiledialog = new OpenFileDialog();
            DialogResult result = STAShowSaveDialog(openfiledialog);
            if (result == DialogResult.OK)
                {
                    if (openfiledialog.CheckPathExists)
                    {
                        String xmllocation = openfiledialog.FileName;
                        xmlDoc.loadXML(xmllocation);
                        xmlDoc.loadRallies(this);
                    }
                }
        }

        

        

        

        




        

       


    }
 
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              