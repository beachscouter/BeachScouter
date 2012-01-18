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

    public delegate void ExportVideoEventHandler(object sender, ExportVideoProgressEventArgs e);

    public class ExportVideoThread
    {
        public event ExportVideoEventHandler DoneAppendingRallyVideoEvent;
        private string videopath;
        private List<long> list_timestamps;
        

        public ExportVideoThread(String path, List<long> list_timestamps)
        {
            this.videopath = path;
            this.list_timestamps = list_timestamps;
        }

        
      
        public void write() {
            int codec = Emgu.CV.CvInvoke.CV_FOURCC('P', 'I', 'M', '1');

            int fps = 25;
            if (list_timestamps.Count > 0)
            {
                String tempvideopath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[0].ToString() + ".mpg";
                Capture tempcapture = new Capture(tempvideopath);
                fps = (int)tempcapture.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_FPS);
                tempcapture.Dispose();
            }

            VideoWriter videowriter = new VideoWriter(videopath, codec, fps, 640, 480, true);
            

            for (int i = 0; i < list_timestamps.Count; i++)
            {
                videopath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[i].ToString() + ".mpg";
                try
                {
                    Capture joincapture = new Capture(videopath);
                    Image<Bgr, byte> frame = joincapture.QueryFrame();
                    for (int n = 1; n < 15; n++)
                        joincapture.QueryFrame();

                    while (frame != null)
                    {
                        videowriter.WriteFrame(frame);
                        frame = joincapture.QueryFrame();
                    }
                    joincapture.Dispose();

                    // Notify main frame to update its progressbar
                    ExportVideoProgressEventArgs e = new ExportVideoProgressEventArgs(i);
                    DoneAppendingRallyVideoEvent(this, e);
                }
                catch (NullReferenceException) { Console.WriteLine("unreadable video file"); }
            }
            videowriter.Dispose();
        
        }


        /*
        public void write()
        {
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

                    AviFileRenderer renderer = new AviFileRenderer(timeline, videopath);
                    renderer.Render();
                    renderer.Dispose();

                }
            
        }
        */



    }
}
