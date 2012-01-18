using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class DoneWritingRallyVideoEventArgs : EventArgs
    {
        private long videoid;

        public DoneWritingRallyVideoEventArgs(long id)
        {
            this.videoid = id;
        }

        public long videoID()
        {
            return this.videoid;
        }



    }
}
