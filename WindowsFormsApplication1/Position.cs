using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Position
    {
        // real world coordinates of the current position
        private int worldx;
        private int worldy;
        private int worldz = 0;
        
 
        //image coordinates of the world coordinates.so we dont need to reverse calibrate again
        private int imagex;
        private int imagey;

        // current time in the video stream
        private long positiontime; // withing the rally (relative) in miliseconds
        private long absolute_positiontime; // within the joined video (in miliseconds) for the beachviewer
        private DateTime instant;

        // player which played the ball
        private Person player;
        private List<Person> defenceplayers = new List<Person>();



        // new Position created via click? If so, it can be cused to validate coordinates
        private bool clickCreated;

        public Position() 
        { 
            this.player = new Person();
            this.clickCreated = false;
        }

 
        public Position(int wx, int wy, int wz, int x, int y, long positiontime, Person player)
        {
            this.worldx = wx;
            this.worldy = wy;
            this.worldz = wz;

            this.imagex = x;
            this.imagey = y;

            this.instant = DateTime.Now;

            this.positiontime = positiontime;
            
            this.player = player;

            this.clickCreated = true;
        }
        

        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------

        public int WorldX
        {
            get { return worldx; }
            set { worldx = value; }
        }
        
        public int WorldY
        {
            get { return worldy; }
            set { worldy = value; }
        }

        public int WorldZ
        {
            get { return worldz; }
            set { worldz = value; }
        }


        public int ImageY
        {
            get { return imagey; }
            set { imagey = value; }
        }

        public int ImageX
        {
            get { return imagex; }
            set { imagex = value; }
        }

        public long PositionTime
        {
            get { return positiontime; }
            set { positiontime = value; }
        }

        public long Absolute_PositionTime
        {
            get { return absolute_positiontime; }
            set { absolute_positiontime = value; }
        }

        public DateTime Instant
        {
            get { return instant; }
            set { instant = value; }
        }

        public Person Player
        {
            get { return player; }
            set { player = value; }
        }

        public List<Person> Defenceplayers
        {
            get { return defenceplayers; }
            set { defenceplayers = value; }
        }

        public bool ClickCreated
        {
            get { return clickCreated; }
            set { clickCreated = value; }
        }
    }
}
