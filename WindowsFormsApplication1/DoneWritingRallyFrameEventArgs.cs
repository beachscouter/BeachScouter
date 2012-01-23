using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class DoneWritingRallyFrameEventArgs : EventArgs
    {
        private int currentframe;
        private int totalframes;

        public DoneWritingRallyFrameEventArgs(int c, int t)
        {
            this.currentframe = c;
            this.totalframes = t;
        }


        public int CurrentFrame()
        {
            return currentframe;
        }

        public int TotalFrames()
        {
            return totalframes;
        }
    }
}
