using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

using Splicer;
using Splicer.Timeline;
using Splicer.Renderer;

namespace BeachScouter
{
    class ExportVideoThread
    {
        private string videopath;
        private List<long> list_timestamps;

        public ExportVideoThread(String path, List<long> list_timestamps)
        {
            this.videopath = path;
            this.list_timestamps = list_timestamps;
        }

        /*
        public void write() {
            int codec = Emgu.CV.CvInvoke.CV_FOURCC('P', 'I', 'M', '1');
            VideoWriter videowriter = new VideoWriter(videopath, codec, 25, 640, 480, true);
            

            for (int i = 0; i < list_timestamps.Count; i++)
            {
                videopath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[i].ToString() + ".mpg";
                try
                {
                    Capture joincapture = new Capture(videopath);
                    Image<Bgr, byte> frame = joincapture.QueryFrame();
                    for (int f=0; f<10; f++)
                        frame = joincapture.QueryFrame();
                    joincapture.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_POS_FRAMES, 0);

                    while (frame != null)
                    {
                        videowriter.WriteFrame(frame);
                        frame = joincapture.QueryFrame();
                    }
                    joincapture.Dispose();
                }
                catch (NullReferenceException) { Console.WriteLine("unreadable video file"); }
            }
            videowriter.Dispose();
        
        }
        */


        public void write()
        {

            if (list_timestamps.Count > 1)
            {
                    string firstVideoFilePath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[0].ToString() + ".mpg";
                    string secondVideoFilePath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[1].ToString() + ".mpg";

                    using (ITimeline timeline = new DefaultTimeline())
                    {
                        IGroup group = timeline.AddVideoGroup(32, 720, 576);

                        var firstVideoClip = group.AddTrack().AddVideo(firstVideoFilePath);
                        var secondVideoClip = group.AddTrack().AddVideo(secondVideoFilePath, firstVideoClip.Duration);

                        using (AviFileRenderer renderer = new AviFileRenderer(timeline, videopath))
                        {
                            renderer.Render();
                        }
                    }
                
            }
        }


        public static DateTime PauseForMilliSeconds(int MilliSecondsToPauseFor)
        {


            System.DateTime ThisMoment = System.DateTime.Now;
            System.TimeSpan duration = new System.TimeSpan(0, 0, 0, 0, MilliSecondsToPauseFor);
            System.DateTime AfterWards = ThisMoment.Add(duration);


            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = System.DateTime.Now;
            }


            return System.DateTime.Now;
        }


    }
}
