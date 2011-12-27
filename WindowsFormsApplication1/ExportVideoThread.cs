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

        public void write() {
            int codec = Emgu.CV.CvInvoke.CV_FOURCC('P', 'I', 'M', '1');
            VideoWriter videowriter = new VideoWriter(videopath, codec, 25, 640, 480, true);


            for (int i = 0; i < list_timestamps.Count; i++)
            {
                videopath = Program.getConfiguration().Mediafolderpath + @"\" + list_timestamps[i].ToString() + ".avi";
                try
                {
                    Capture joincapture = new Capture(videopath);
                    Image<Bgr, byte> frame = joincapture.QueryFrame();
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


    }
}
