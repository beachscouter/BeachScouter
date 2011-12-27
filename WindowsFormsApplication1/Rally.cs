/* This class represents a Rally (a.k.a Spielzug, Move, etc etc etc etc)
 * It contains Information abouth wether the rally was a:
 *  - KILL 
 *  - DROP 
 *  - SMASH or BIGPOINT
 * 
 * It also contains various informations about:
 *  - Wich team was UP and which team was DOWN at the time
 *  - Positions of serve, receive, pass, jump and defense
 *  - The time the rally started and ended
 *  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeachScouter
{
    public class Rally
    {
        public enum PlayerPosition { upperLeft = 1, upperRight, lowerLeft, lowerRight };

        private PlayerPosition teamAPlayer1_pos;
        private PlayerPosition teamAPlayer2_pos;
        private PlayerPosition teamBPlayer1_pos;
        private PlayerPosition teamBPlayer2_pos;
        private Boolean standardposition;

        

        private Position service_pos;
        private Position reception_pos;
        private Position set_pos;
        private Position start_pos;
        private Position takeoff_pos;
        private Position defence_pos;
        private Position approach_pos;


         

        private long start_time; // in ticks of time now
        private long endRally_time; // in ticks of time now
        private long absolute_start_time; // the start of the rally in the joined video in miliseconds
        private long absolute_end_time; // the end (int ticks) time in the joined video
        private long duration_ticks; // in ticks

        private int correspondingSetNr;

        // vileicht kann man die auch als enum definieren:
        private bool finalized;
        private bool bigPoint;
        private bool kill;
        
        // If not smash -> shot
        private bool smash;
        private bool drop;
        private bool defaultRallyDevelopment;
        private bool timeout;

        public Rally(long start_time, int setNr)
        {

            this.start_time = start_time;


            // These new position objects are in "click"-order
            service_pos = new Position();
            reception_pos = new Position();
            set_pos = new Position();
            start_pos = new Position();
            takeoff_pos = new Position();
            defence_pos = new Position();
            approach_pos = new Position();

            // Set the default values
            kill = true;
            drop = false;
            smash = true;
            bigPoint = false;
            finalized = false;
            defaultRallyDevelopment = true;
            timeout = false;
            correspondingSetNr = setNr;
        }

        // This constructor is called in case we start the rally uppon button click "Start Move". That is, we wont have a serve position yet
        public Rally(PlayerPosition tAp1,  PlayerPosition tAp2, PlayerPosition tBp1, PlayerPosition tBp2, long start_time, int setNr)
        {   

            this.teamAPlayer1_pos = tAp1;
            this.teamAPlayer2_pos = tAp2;
            this.teamBPlayer1_pos = tBp1;
            this.teamBPlayer2_pos = tBp2;




            this.start_time = start_time;


            // These new position objects are in "click"-order
            service_pos = new Position();
            reception_pos = new Position();
            set_pos = new Position();
            start_pos = new Position();
            takeoff_pos = new Position();
            defence_pos = new Position();
            approach_pos = new Position();

            // Set the default values
            kill = true;
            drop = false;
            smash = true;
            bigPoint = false;
            finalized = false;
            defaultRallyDevelopment = true;
            timeout = false;
            correspondingSetNr = setNr;
        }


        // ---------------------------------------------------------------------
        // methods
        // ---------------------------------------------------------------------
        
        // The team played the ball in the next to last click event gets either a +1 or 0
        public void setNewScore(Game game, bool teamAup)
        {
            Tuple<Position, Position> last2Positions = getLast2Positions();

            // trivial case, ball was outside the field or not even service succeeded
            if (last2Positions == null)
            {
                kill = false;
                return;
            }
            if (last2Positions.Item2.WorldX < 0 || last2Positions.Item2.WorldY < 0 || last2Positions.Item2.WorldY > 16 || last2Positions.Item2.WorldX > 8)
            {
                // no point, attack failed
                kill = false;
                return;
            }
            
            // find attacking team
            Team madeAttack;
            if (teamAup)
            {
                if (last2Positions.Item1.WorldY <= 8)
                    madeAttack = game.TeamA;
                else
	                madeAttack = game.TeamB;
            }
            else
            {
                if (last2Positions.Item1.WorldY <= 8)
                    madeAttack = game.TeamB;
                else
                    madeAttack = game.TeamA;
            }

            // increment attacking team score
            if (teamAup)
            {
                if (last2Positions.Item2.WorldY > 8 && madeAttack == game.TeamA)
                {
                    game.Sets[game.Sets.Count - 1].TeamAScore += 1;
                    kill = true;
                    return;
                }
                if (last2Positions.Item2.WorldY <= 8 && madeAttack == game.TeamB)
                {
                    game.Sets[game.Sets.Count - 1].TeamBScore += 1;
                    kill = true;
                    return;
                }
            }

            else
            {
                if (last2Positions.Item2.WorldY <= 8 && madeAttack == game.TeamA)
                {
                    game.Sets[game.Sets.Count - 1].TeamAScore += 1;
                    kill = true;
                    return;
                }
                if (last2Positions.Item2.WorldY > 8 && madeAttack == game.TeamB)
                {
                    game.Sets[game.Sets.Count - 1].TeamBScore += 1;
                    kill = true;
                    return;
                }
            }
            // in any other case, there is no change of score for either team.
            kill = false;
            return;
        }


        private Tuple<Position, Position> getLast2Positions()
        {
            if (approach_pos.ClickCreated)
                return Tuple.Create(defence_pos, approach_pos);
            if (defence_pos.ClickCreated)
                return Tuple.Create(takeoff_pos,defence_pos);

            // start position is only created in reviewer (manually)
            if (takeoff_pos.ClickCreated)
            {
                if (start_pos.ClickCreated)
                    return Tuple.Create(start_pos, takeoff_pos);
                else
                    return Tuple.Create(set_pos, takeoff_pos);
            }
            if (start_pos.ClickCreated)
                return Tuple.Create(set_pos, start_pos);
            if (set_pos.ClickCreated)
                return Tuple.Create(reception_pos,set_pos);
            if (reception_pos.ClickCreated)
                return Tuple.Create(service_pos,reception_pos);
            return null;
        }

        // ---------------------------------------------------------------------
        // getter and setter
        // ---------------------------------------------------------------------

        public Rally.PlayerPosition TeamAPlayer1_pos
        {
            get { return teamAPlayer1_pos; }
            set { teamAPlayer2_pos = value; }
        }

        public Rally.PlayerPosition TeamAPlayer2_pos
        {
            get { return teamAPlayer2_pos; }
            set { teamAPlayer2_pos = value; }
        }

        public Rally.PlayerPosition TeamBPlayer1_pos
        {
            get { return teamBPlayer1_pos; }
            set { teamBPlayer1_pos = value; }
        }

        public Rally.PlayerPosition TeamBPlayer2_pos
        {
            get { return teamBPlayer2_pos; }
            set { teamBPlayer2_pos = value; }
        }

    

        public Position Service_pos
        {
            get { return service_pos; }
            set { service_pos = value; }
        }

        public Position Reception_pos
        {
            get { return reception_pos; }
            set { reception_pos = value; }
        }

        public Position Set_pos
        {
            get { return set_pos; }
            set { set_pos = value; }
        }

        public Position Start_pos
        {
            get { return start_pos; }
            set { start_pos = value; }
        }

        public Position Takeoff_pos
        {
            get { return takeoff_pos; }
            set { takeoff_pos = value; }
        }

        public Position Defence_pos
        {
            get { return defence_pos; }
            set { defence_pos = value; }
        }

        public Position Approach_pos
        {
            get { return approach_pos; }
            set { approach_pos = value; }
        }

        public long Start_time
        {
            get { return start_time; }
            set { start_time = value; }
        }

        public long Duration_ticks
        {
            get { return duration_ticks; }
            set { duration_ticks = value; }
        }

        public long Absolute_start_time
        {
            get { return absolute_start_time; }
            set { absolute_start_time = value; }
        }

        public long Absolute_end_time
        {
            get { return absolute_end_time; }
            set { absolute_end_time = value; }
        }



        public long EndRally_time
        {
            get { return endRally_time; }
            set { endRally_time = value; }
        }

        public bool Finalized
        {
            get { return finalized; }
            set { finalized = value; }
        }

        public bool BigPoint
        {
            get { return bigPoint; }
            set { bigPoint = value; }
        }
        
        public bool Kill
        {
            get { return kill; }
            set { kill = value; }
        }
        
        public bool Smash
        {
            get { return smash; }
            set { smash = value; }
        }
        
        public bool Drop
        {
            get { return drop; }
            set { drop = value; }
        }

        public bool DefaultRallyDevelopment
        {
            get { return defaultRallyDevelopment; }
            set { defaultRallyDevelopment = value; }
        }

        public int CorrespondingSetNr
        {
            get { return correspondingSetNr; }
            set { correspondingSetNr = value; }
        }

        public Boolean Standardposition
        {
            get { return standardposition; }
            set { standardposition = value; }
        }

        public bool Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

    }
}

