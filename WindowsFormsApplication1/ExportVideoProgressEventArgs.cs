/*
 * 
 * To proberly update a progress bar
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class ExportVideoProgressEventArgs : EventArgs
    {
        private int rallynum;

        public ExportVideoProgressEventArgs(int number)
        {
            this.rallynum = number;
        }

        public int State()
        {
            return this.rallynum;
        }


    }
}
