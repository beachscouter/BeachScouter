/*
 * THREAD FOTR WRITING/CUTTINGOUT SINGLE RALLY VIDEOS FROM A LOADED VIDEO
 * 
 * OR
 * 
 * FROM A GIVEN BUFFER OF FRAMES
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
using System.Threading;
namespace BeachScouter
{

    public delegate void DoneWritingRallyVideoEventHandler(object sender, DoneWritingRallyVideoEventArgs e);
    public delegate void DoneWritingRallyFrameEventHandler(object sender, DoneWritingRallyFrameEventArgs e);

    class WriteRallyVideoThread
    {
        public event DoneWritingRallyVideoEventHandler donewritingrallyvideo;
        public event DoneWritingRallyFrameEventHandler donewritingrallyframe;

        private double start, end;
        private long starttime;
        private VideoWriter videoWriter;
        private String loaded_videopath;
        List<Image<Bgr, Byte>> buffer;

        // For cutting from a loaded video
        public WriteRallyVideoThread(double start, double end, String loadedvideopath, VideoWriter writer, long starttime)
        {
            this.start = Math.Floor(start);
            this.end = Math.Ceiling(end);
            this.loaded_videopath = loadedvideopath;
            this.videoWriter = writer;

            this.starttime = starttime; // to notify the main frame that we are done writing the rally with id starttime  so we can create a screenshot.
        }



        // for writing from a given buffer
        public WriteRallyVideoThread(List<Image<Bgr, Byte>> buffer, VideoWriter writer, long starttime)
        {
            this.videoWriter = writer;
            this.buffer = buffer;
            this.starttime = starttime;
        }

        public void write()
        {
            Capture tempcapture = new Capture(loaded_videopath);
            int fps = (int)tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);

            if (tempcapture != null)
            {

                int durationframes = (int)((end - start) * fps);
                int progressbar_max = (int)(durationframes + (start * fps));

                /*
                Console.WriteLine("start frame should be: " + (start * fps));
                Emgu.CV.CvInvoke.cvSetCaptureProperty(tempcapture, CAP_PROP.CV_CAP_PROP_POS_FRAMES, (start * fps));
                Console.WriteLine("current frame pos (opencv): " + Emgu.CV.CvInvoke.cvGetCaptureProperty(tempcapture, CAP_PROP.CV_CAP_PROP_POS_FRAMES));

                tempcapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, (start * fps));
                Console.WriteLine("current frame pos (emgucv): " + tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES));
                */

                int duration = (int)(end - start);
                String outputpath = Program.getConfiguration().Mediafolderpath + @"\" + starttime.ToString() + ".mpg";
                String argument = "-sameq -ss " + start + " -t " + duration + " -i " + loaded_videopath + " " + outputpath;

                String exepath = Environment.CurrentDirectory + @"\" + "ffmpeg.exe";
                Process myProcess = new Process();

                try
                {
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.UseShellExecute = false;
                    myProcess.StartInfo.FileName = exepath;
                    myProcess.StartInfo.Arguments = argument;
                    myProcess.EnableRaisingEvents = true;
                    myProcess.Exited += new EventHandler(processFinished);
                    myProcess.Start();
                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine(ex.NativeErrorCode);
                }


            }
                /*
                DoneWritingRallyFrameEventArgs eframe;
                
                for (int i = 0; i < (start * fps); i++)
                {
                    (tempcapture).QueryFrame();
                    eframe = new DoneWritingRallyFrameEventArgs(i, progressbar_max);
                    donewritingrallyframe(this, eframe);
                }
                
                

                int count = 0;
                while (count < durationframes)
                {
                    frame = (tempcapture).QueryFrame();
                    videoWriter.WriteFrame(frame);
                    count++;
                    eframe = new DoneWritingRallyFrameEventArgs((count+(int)(start * fps)), progressbar_max);
                    donewritingrallyframe(this, eframe);
                }
            }


            tempcapture.Dispose();
            videoWriter.Dispose();



            // We are theoretically done with writing the video... so we notify all registered listeners
            DoneWritingRallyVideoEventArgs e = new DoneWritingRallyVideoEventArgs(this.starttime);
            donewritingrallyvideo(this, e);

            */
        }



        public void processFinished(object sender, EventArgs evt)
        {
            DoneWritingRallyVideoEventArgs e = new DoneWritingRallyVideoEventArgs(this.starttime);
            donewritingrallyvideo(this, e);
        }


        public void writeFromBuffer()
        {
            for (int i = 0; i < buffer.Count; i++)
                videoWriter.WriteFrame(buffer[i]);
            buffer.Clear();
            videoWriter.Dispose();

            // We are theoretically done with writing the video... so we notify all registered listeners
            DoneWritingRallyVideoEventArgs e = new DoneWritingRallyVideoEventArgs(this.starttime);
            donewritingrallyvideo(this, e);

        }


    }
}
