using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Set
    {
        private readonly int winningPointSet12 = 21;
        private readonly int winningPointSet3 = 15;
        
        private int setNr;

        private int teamAScore;
        private int teamBScore;

        private List<Rally> rallies;
        private List<Timeout> timeouts;
        
        // constructor
        public Set(int setNr)
        {
            this.setNr = setNr;
            this.teamAScore = 0;
            this.teamBScore = 0;

            this.rallies = new List<Rally>();
            this.timeouts = new List<Timeout>();
        }

        // ---------------------------------------------------------------------
        // methods
        // ---------------------------------------------------------------------
        public bool isSetWon()
        {
            switch (this.setNr)
            { 
                case 1:
                    if (Math.Abs(teamAScore - teamBScore) > 1 && (teamAScore >= winningPointSet12 || teamBScore >= winningPointSet12))
                    { return true; }
                    return false;

                case 2:
                    goto case 1; 

                case 3:
                    if (Math.Abs(teamAScore - teamBScore) > 1 && (teamAScore >= winningPointSet3 || teamBScore >= winningPointSet3))
                    { return true; }
                    return false;

                default:
                    // This code should actually never be reached
                    throw new Exception("It is not possible to have more than three sets in a single game.");
            }
        }


        public Team getSetWinner(Game game)
        {
            if (this.isSetWon())
            {
                if (teamAScore > teamBScore)
                    return game.TeamA;
                else
                    return game.TeamB;
            }
            return null;
        }
        

        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------
        public int SetNr
        {
            get { return setNr; }
            set { setNr = value; }
        }

        public int TeamAScore
        {
            get { return teamAScore; }
            set { teamAScore = value; }
        }

        public int TeamBScore
        {
            get { return teamBScore; }
            set { teamBScore = value; }
        }

        public List<Rally> Rallies
        {
            get { return rallies; }
            set { rallies = value; }
        }

        public List<Timeout> Timeouts
        {
            get { return timeouts; }
            set { timeouts = value; }
        }
    }
}
