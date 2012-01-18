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
    public class SaveDialogState
    {
        public DialogResult result;
        public FileDialog dialog;


        public void ThreadProcShowDialog()
        {
            result = dialog.ShowDialog();
        }
    }
}
