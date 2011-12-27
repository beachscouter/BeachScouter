using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Team
    {
        private string name;
        private string nationality;
        private string club;

        private Person trainer;
        private Person player1;
        private Person player2;




        public Team(string name, Person player1, Person player2) 
        {
            this.name = name;
            this.player1 = player1;
            this.player2 = player2;
        }

        // ---------------------------------------------------------------------
        // methods
        // ---------------------------------------------------------------------
        


        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Nationality
        {
            get { return nationality; }
            set { nationality = value; }
        }

        public string Club
        {
            get { return club; }
            set { club = value; }
        }

        public Person Trainer
        {
            get { return trainer; }
            set { trainer = value; }
        }

        public Person Player1
        {
            get { return player1; }
            set { player1 = value; }
        }

        public Person Player2
        {
            get { return player2; }
            set { player2 = value; }
        }


    }
}

