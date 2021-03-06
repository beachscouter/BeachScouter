﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace BeachScouter
{
    public class Configuration
    {


        // For the camera settings
        private Calibration calibration;
        PointF[] calibration_corners; // so we can redraw the field lines
        private int camera_index;

        

        // For the team settings
        private Team teama;
        private Team teamb;
        private Boolean teamAup;
        private int teama_score;
        private int teamb_score;

        

        // For the advanced setting
        private String mediafolderpath;
        private String xmlOutputFile;
        private String outputDir;
        private String loadXmlFile;
        private String location, tournamentType, freeText, gameID;
        
        public Configuration()
        {
            // defaults
            calibration = null;
            calibration_corners = new PointF[4];

            camera_index = 0;


            teama = new Team("Team1", new Person("", "", Person.DefaultPosition.Left, Person.Role.Player), new Person("", "", Person.DefaultPosition.Right, Person.Role.Player));
            teamb = new Team("Team2", new Person("", "", Person.DefaultPosition.Left, Person.Role.Player), new Person("", "", Person.DefaultPosition.Right, Person.Role.Player));
            
            teamAup = false;
            teama.Player1.Current_position = Rally.PlayerPosition.lowerLeft;
            teama.Player2.Current_position = Rally.PlayerPosition.lowerRight;
            teamb.Player1.Current_position = Rally.PlayerPosition.upperLeft;
            teamb.Player2.Current_position = Rally.PlayerPosition.upperRight;

            location = "";
            tournamentType = "";
            freeText = "";
            outputDir = "";
            gameID = "";

            loadXmlFile = "Please choose a XML file";
        }

        /******************** Set game ID ********************************/
        public void setGameIDAndCreateOutputFilesDir(String p, String gameID)
        {
            // set gameID -> first thing to do!
            this.gameID = gameID;
            
            String path = p;

            // FolderNameNext -> if folder exists create "foobar_1" and so on
            if (path == "")
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string newGameDir = System.IO.Path.Combine(path, gameID);

            bool folderExists = System.IO.Directory.Exists(newGameDir);
            int cnt = 0;
            string newDir = "";

            while (folderExists)
            {
                cnt += 1;
                newDir = gameID + "_" + cnt.ToString();
                newGameDir = System.IO.Path.Combine(path, newDir);
                if (!System.IO.Directory.Exists(newGameDir))
                {
                    folderExists = false;
                }
            }
            
            System.IO.Directory.CreateDirectory(newGameDir);
            this.outputDir = newGameDir;
            this.xmlOutputFile = newGameDir + @"\" + gameID + ".xml";
            this.mediafolderpath = newGameDir;
        }
        /*****************************************************************/

        /******************** Camera Configurations **********************/

        public PointF[] Calibration_corners
        {
            get { return calibration_corners; }
            set { calibration_corners = value; }
        }

        public Calibration Calibration
        {
            get { return calibration; }
            set { calibration = value; }
        }


        public int Camera_index
        {
            get { return camera_index; }
            set { camera_index = value; }
        }

        /*****************************************************************/



        /******************** TEAM CONFIGURATIONS ***********************/

        public Team Teama
        {
            get { return teama; }
            set { teama = value; }
        }


        public Team Teamb
        {
            get { return teamb; }
            set { teamb = value; }
        }


        public Boolean TeamAup
        {
            get { return teamAup; }
            set { teamAup = value; }
        }

        /*****************************************************************/


        /******************** ADVANCED CONFIGURATIONS ***********************/
        public String Mediafolderpath
        {
            get { return mediafolderpath; }
            set { mediafolderpath = value; }
        }

        public String XmlOutputFile
        {
            get { return xmlOutputFile; }
            set { xmlOutputFile = value; }
        }

        public String OutputDir
        {
            get { return outputDir; }
            set { outputDir = value; }
        }

        public String LoadXmlFile
        {
            get { return loadXmlFile; }
            set { loadXmlFile = value; }
        }

        public String Location
        {
            get { return location; }
            set { location = value; }
        }

        public String TournamentType
        {
            get { return tournamentType; }
            set { tournamentType = value; }
        }

        public String GameID
        {
            get { return gameID; }
            set { gameID = value; }
        }

        public String FreeText
        {
            get { return freeText; }
            set { freeText = value; }
        }

        public int Teama_score
        {
            get { return teama_score; }
            set { teama_score = value; }
        }


        public int Teamb_score
        {
            get { return teamb_score; }
            set { teamb_score = value; }
        }
        /*******************************************************************/
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        