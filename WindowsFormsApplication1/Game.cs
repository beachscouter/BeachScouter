using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Game
    {
        private string location;
        private string result;

        private DateTime startDate;
        private DateTime endDate;

        private Team teamA;
        private Team teamB;

        private List<Set> sets;
        private Rally current_rally;
        private Rally reviewing_rally; // The rally that is currently being edited in the Reviewer

      

        private Team winner;



        // constructor
        public Game(Team teamA, Team teamB)
        {
            this.location = "";
            this.teamA = teamA;
            this.teamB = teamB;
            this.winner = null;

            this.sets = new List<Set>();

            this.startDate = DateTime.Now;
        }

        // ---------------------------------------------------------------------
        // methods
        // ---------------------------------------------------------------------
        public Team getWinner()
        {
            switch (sets.Count)
            { 
                case 1:
                    return null;

                case 2:
                    int winningStrikeTeamA = 0;
                    int winningStrikeTeamB = 0;
                    foreach (Set s in sets)
                    {
                        if (s.isSetWon())
                        {
                            if (s.TeamAScore > s.TeamBScore)
                            { winningStrikeTeamA += 1; }
                            else
                            { winningStrikeTeamB += 1; }
                        }
                    }
                    if (winningStrikeTeamA == 2)
                    { return this.teamA; }

                    if (winningStrikeTeamB == 2)
                    { return this.teamB; }

                    return null;
                
                case 3:
                    goto case 2;

                default:
                    // This code should actually never be reached
                    throw new Exception("DEBUG: It is not possible to have more than three sets in a single game.");
            }
        }





        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------
        public string Location
        {
            get { return location; }
            set { location = value; }
        }

        public string Result
        {
            get { return result; }
            set { result = value; }
        }

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

        public Team TeamA
        {
            get { return teamA; }
            set { teamA = value; }
        }

        public Team TeamB
        {
            get { return teamB; }
            set { teamB = value; }
        }

        public Team Winner
        {
            get { return winner; }
            set { winner = value; }
        }

        public List<Set> Sets
        {
            get { return sets; }
            set { sets = value; }
        }


        public Rally Current_rally
        {
            get { return current_rally; }
            set { current_rally = value; }
        }

        public Rally Reviewing_rally
        {
            get { return reviewing_rally; }
            set { reviewing_rally = value; }
        }





    }
}
