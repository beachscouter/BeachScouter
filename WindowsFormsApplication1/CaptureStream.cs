using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Threading;



namespace BeachScouter
{
    public class CaptureStream
    {
        private Form_Main parent_form; // the source of the evil
        private int       fps = 25;
        private static List<Capture> devices;
        private int screenshotwidth = 84; 
        private int screenshotheight = 68;

        public CaptureStream()
        {
        }

        public void createCaptures()
        {
            devices = new List<Capture>();
            for (int i = 0; i < 2; i++)
            {
                Capture c = new Capture(i);
                if (c.QueryFrame() != null)
                    devices.Add(c);
            }
        }

        public List<Capture> checkCaptureDevices()
        {
            createCaptures();

            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].QueryFrame() == null)
                    devices.RemoveAt(i);
            }
            return devices;
        }





        /*************************************************************************************************/
        /**************************************** REVIEW STUFF *******************************************/

        /*******************************************************************/
        /******************************** PAUSE ****************************/
        // sometimes you have to take a break
        public void takeABreak(Capture capture, long start_time, double frame_number)
        {
        }




        // returns the frame number of the specified position
        public double timeToFrameNumber(long time_start, long time_current_pos)
        {
            double diff = (double)((time_current_pos - time_start)/1000);
            return diff * fps;
        }



        /*******************************************************************/
        /*************************** SCREENSHOT ****************************/
        public Bitmap createScreenshot(long start_time, Boolean finalized)
        {

            String video_name = start_time.ToString();
            Capture capture_move = null;
            Image<Bgr, Byte> screenshot = new Image<Bgr, Byte>(64, 48);
            Bitmap bmp = new Bitmap(screenshotwidth, screenshotheight);

       
            try
            {
                String videopath = Program.getConfiguration().Mediafolderpath + @"\" + video_name + ".mpg";

                
                capture_move = new Capture(videopath);

                double frame_number = 0;
                capture_move.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);
                while (capture_move.QueryFrame() != null)
                    frame_number++;


                capture_move.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, frame_number / 2);

                
                screenshot = capture_move.QueryFrame();

                

            }
            catch (NullReferenceException) { Console.WriteLine("unreadable video file in capture stream"); }

            if (screenshot != null)
            {
                bmp = screenshot.ToBitmap(screenshotwidth, screenshotheight);
                Graphics gBmp = Graphics.FromImage(bmp);
                Pen Pen;
                if (finalized)
                    Pen = new Pen(Brushes.GreenYellow, 3);
                else
                    Pen = new Pen(Brushes.DimGray, 3);

                Rectangle rect = new Rectangle(0, 0, screenshotwidth-1, screenshotheight-1);
                gBmp.DrawRectangle(Pen, rect);


                // Draw timestamp
               
                String current_time = String.Format("{0:hh\\:mm\\:ss}",  DateTime.Now);

                int stringlength = current_time.Length*5;

                Font drawFont = new Font("Arial", 10, GraphicsUnit.Pixel);
                SolidBrush drawBrush;
                if (finalized)
                    drawBrush = new SolidBrush(Color.GreenYellow);
                else
                    drawBrush = new SolidBrush(Color.DimGray);

                gBmp.FillRectangle(drawBrush, 0, 54, screenshotwidth-1, screenshotheight-1);

                rect = new Rectangle((84 - stringlength) / 2 - 1, 53, screenshotwidth-1, screenshotheight-1);
                drawBrush = new SolidBrush(Color.White);
                gBmp.DrawString(current_time, drawFont, drawBrush, rect);


                return bmp;
            }



            // If we dont have a screenshot because we clicked to fast. We basically dont have a video file
            return null;

        }

        
       
        
    }
}
