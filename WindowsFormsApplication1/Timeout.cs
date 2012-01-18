using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Timeout
    {
        private DateTime startTimeout_time;
        private DateTime endTimeout_time;

        private int currentScore_TeamA;
        private int currentScore_TeamB;

        private string requestedByTeam;

        public Timeout(int scoreTeamA, int scoreTeamB, string teamName)
        {
            this.startTimeout_time = DateTime.Now;
            this.currentScore_TeamA = scoreTeamA;
            this.currentScore_TeamB = scoreTeamB;
            this.requestedByTeam = teamName;
        }

        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------

        public DateTime StartTimeout_time
        {
            get { return startTimeout_time; }
            set { startTimeout_time = value; }
        }

        public DateTime EndTimeout_time
        {
            get { return endTimeout_time; }
            set { endTimeout_time = value; }
        }

        public int CurrentScore_TeamA
        {
            get { return currentScore_TeamA; }
            set { currentScore_TeamA = value; }
        }

        public int CurrentScore_TeamB
        {
            get { return currentScore_TeamB; }
            set { currentScore_TeamB = value; }
        }

        public string RequestedByTeam
        {
            get { return requestedByTeam; }
            set { requestedByTeam = value; }
        }
    }
}
