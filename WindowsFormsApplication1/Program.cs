using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Emgu.CV;
using System.Drawing;

namespace BeachScouter
{
    static class Program
    {
        private static Configuration configuration;
        private static Capture capture;

        public static Capture Capture
        {
            get { return Program.capture; }
            set { Program.capture = value; }
        }

        public static Configuration getConfiguration() { return configuration; }

 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>


        [STAThread]
        static void Main()
        {
            configuration = new Configuration();
            //capture = new Capture();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //Application.Run(new SetupAssistant());
            InitialForm initialform = new InitialForm();
            initialform.StartPosition = FormStartPosition.Manual;
            initialform.DesktopLocation = new Point(0, 0);
            Application.Run(initialform);
        }
    }
}
