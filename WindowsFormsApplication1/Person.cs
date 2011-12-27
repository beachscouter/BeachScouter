using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Person
    {
        public enum DefaultPosition { None = 1, Left, Right };
        public enum Role { Trainer = 1, Player };

        private string surname;
        private string firstname;
        private string name;

      
       

        private string nationality;
        private string club;
        private int height;
        private float weight;

        private DefaultPosition defaultPosition;

        
        private Rally.PlayerPosition current_position;

        
        private Role role;

        public Person() { }

        public Person(string surname, string firstname, DefaultPosition dP, Role role)
        {
            this.surname = surname;
            this.firstname = firstname;
            name = firstname + " " + surname;
            this.defaultPosition = dP;
            this.role = role;
        }

        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------


        public string Surname
        {
            get { return surname; }
            set { surname = value; name = firstname + " " + surname; }
        }
        
        public string Firstname
        {
            get { return firstname; }
            set { firstname = value; name = firstname + " " + surname; }
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

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public float Weight
        {
            get { return weight; }
            set { weight = value; }
        }


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

      

        public Rally.PlayerPosition Current_position
        {
            get { return current_position; }
            set { current_position = value; }
        }

        public DefaultPosition defaultposition
        {
            get { return defaultPosition; }
            set { defaultPosition = value; }
        }
    }
}
