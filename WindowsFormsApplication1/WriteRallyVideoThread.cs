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
    class WriteRallyVideoThread
    {
        private List<Image<Bgr, Byte>> rallyframes;
        private VideoWriter videoWriter;

        public WriteRallyVideoThread(List<Image<Bgr, Byte>> rallyframes, VideoWriter writer)
        {
            this.rallyframes = rallyframes;
            this.videoWriter = writer;
        }


        public void write()
        {
            for (int i = 0; i < rallyframes.Count; i++)
            {
                Image<Bgr, Byte> frame = rallyframes[i];
                if (frame != null)
                    videoWriter.WriteFrame(frame);

            }
        }

    }
}
