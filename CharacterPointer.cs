using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsGame2
{
    class CharacterPointer
    {
        public enum Sides
        {
            Player,
            CPU_Opponents
        }

        private Sides _belongsToSide;

        public Sides BelongsToSide
        {
            get { return _belongsToSide; }
            set { _belongsToSide = value; }
        }

        private int _whichOne;

        public int WhichOne
        {
            get { return _whichOne; }
            set { _whichOne = value; }
        }

        private int _agility;

        public int Agility
        {
            get { return _agility; }
            set { _agility = value; }
        }

        private CharacterPointer _next;

        public CharacterPointer Next
        {
            get { return _next; }
            set { _next = value; }
        }

        public CharacterPointer()
        {
        }

        public CharacterPointer(Sides belongsToSide, int whichOne)
        {
            _belongsToSide = belongsToSide;
            _whichOne = whichOne;
        }

        public CharacterPointer(Sides belongsToSide, int whichOne, int agility)
        {
            _belongsToSide = belongsToSide;
            _whichOne = whichOne;
            _agility = agility;
        }
    }
}
