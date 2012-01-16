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
namespace BeachScouter
{
    class WriteVideoThread
    {
        private double start, end, durationframes;
        private double startmsec, endmsec;
        private VideoWriter videoWriter;
        private String loaded_videopath;
        public Bitmap screenshot;
        private int screenshotwidth = 84;
        private int screenshotheight = 68;

        public WriteVideoThread(double start, double end, VideoWriter writer, String loadedvideopath)
        {
            Console.WriteLine("WindowsMediaSucks: " + start + " - " + end);
            this.start = Math.Floor(start);
            this.end = Math.Ceiling(end) ;
            Console.WriteLine("Increased: " + this.start + " - " + this.end);
            //this.durationframes = (this.end - this.start) * 25;

            this.startmsec = this.start * 1000;
            this.endmsec = this.end * 1000;
            Console.WriteLine("StartMsec: " + startmsec);
            Console.WriteLine("Duration: " + (this.end - this.start));
            Console.WriteLine("Duration frames: " + ((this.end - this.start) * 30));

            this.videoWriter = writer;
            this.loaded_videopath = loadedvideopath;
        }

        public void write()
        {
            Capture tempcapture = new Capture(loaded_videopath);

            // Because this all sucks and we dont get anyfeedback on wether/when emgucv/csharp has loaded the video
            // we have to manually "wait" (not sleep, because sleep stops everything including opening the video)
            // by reading a couple of frames first. ITS BECAUSE OF CODE LIKE THIS AND BECAUSE OF NO PROPER FRAMESWORKS THAT WINDOWS SUCK!!11!!
            for (int i=0; i<100; i++)
                (tempcapture).QueryFrame();

            Image<Bgr, Byte> frame;
            if (tempcapture != null)
            {
                //tempcapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC, start);
                
                double fps = tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);
                tempcapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, ((this.start) * fps));
                int durationframes = (int)((this.end - this.start) * fps); // since c# sucks i have to do it manually just like any other crap
                //for (int framecount = 0; framecount <= durationframes; framecount++)
                while(tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_MSEC)< endmsec)
                {
                    frame = (tempcapture).QueryFrame();
                    videoWriter.WriteFrame(frame);
                    CvInvoke.cvWaitKey(100);
                }
            }
            

            tempcapture.Dispose();
            videoWriter.Dispose();
        }


        private Bitmap createScreenshot(Image<Bgr, Byte> frame)
        {
            Bitmap bmp = new Bitmap(screenshotwidth, screenshotheight);


            if (frame != null)
            {
                bmp = frame.ToBitmap(screenshotwidth, screenshotheight);
                Graphics gBmp = Graphics.FromImage(bmp);
                Pen Pen = new Pen(Brushes.DimGray, 3);

                Rectangle rect = new Rectangle(0, 0, screenshotwidth - 1, screenshotheight - 1);
                gBmp.DrawRectangle(Pen, rect);


                // Draw timestamp

                String current_time = String.Format("{0:hh\\:mm\\:ss}", DateTime.Now);

                int stringlength = current_time.Length * 5;

                Font drawFont = new Font("Arial", 10, GraphicsUnit.Pixel);
                SolidBrush drawBrush = new SolidBrush(Color.DimGray);

                gBmp.FillRectangle(drawBrush, 0, 54, screenshotwidth - 1, screenshotheight - 1);

                rect = new Rectangle((84 - stringlength) / 2 - 1, 53, screenshotwidth - 1, screenshotheight - 1);
                drawBrush = new SolidBrush(Color.White);
                gBmp.DrawString(current_time, drawFont, drawBrush, rect);


                return bmp;
            }

            // If we dont have a screenshot because we clicked to fast. We basically dont have a video file
            return null;

        }



    }
}
