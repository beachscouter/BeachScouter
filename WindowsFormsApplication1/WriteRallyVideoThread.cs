/*
 * THREAD FOTR WRITING/CUTTINGOUT SINGLE RALLY VIDEOS FROM A LOADED VIDEO
 * 
 * 
 * 
 */


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

    public delegate void DoneWritingRallyVideoEventHandler(object sender, DoneWritingRallyVideoEventArgs e);


    class WriteRallyVideoThread
    {
        public event DoneWritingRallyVideoEventHandler donewritingrallyvideo;

        private double start, end;
        private long starttime;
        private VideoWriter videoWriter;
        private String loaded_videopath;


        public WriteRallyVideoThread(double start, double end, String loadedvideopath, VideoWriter writer, long starttime)
        {
            this.start = Math.Floor(start);
            this.end = Math.Ceiling(end);
            this.loaded_videopath = loadedvideopath;
            this.videoWriter = writer;

            this.starttime = starttime; // to notify the main frame that we are done writing the rally with id starttime  so we can create a screenshot.
        }

        public void write()
        {
            Capture tempcapture = new Capture(loaded_videopath);
            int fps = (int)tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);

            Image<Bgr, Byte> frame;
            if (tempcapture != null)
            {

                

                for (int i = 0; i < (start * fps); i++)
                    (tempcapture).QueryFrame();

                int durationframes = (int)((end - start) * fps);

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



            // We are theoretically done with writing the video... so we notify all registered listeners
            DoneWritingRallyVideoEventArgs e = new DoneWritingRallyVideoEventArgs(this.starttime);
            donewritingrallyvideo(this, e);

            
        }

    }
}
