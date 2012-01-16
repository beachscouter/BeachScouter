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
            this.start = Math.Floor(start);
            this.end = Math.Ceiling(end);
            this.videoWriter = writer;
            this.loaded_videopath = loadedvideopath;
        }

        public void write()
        {
            Capture tempcapture = new Capture(loaded_videopath);

            Image<Bgr, Byte> frame;
            if (tempcapture != null)
            {

                double fps2 = tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);

                for (int i = 0; i < (start * fps2); i++)
                    (tempcapture).QueryFrame();

                int durationframes = (int)((end - start) * fps2);

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
