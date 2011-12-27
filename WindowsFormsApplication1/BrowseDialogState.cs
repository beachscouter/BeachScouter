using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BeachScouter
{
    public class BrowseDialogState
    {
        public DialogResult result;
        public FolderBrowserDialog dialog;


        public void ThreadProcShowDialog()
        {
            result = dialog.ShowDialog();
        }
    }
}
