using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class LevelUpMessage
    {
        public enum Messages
        {
            None,
            NewMagicSpell,
            MagicSpellLevelIncreased
        }

        private Messages _message;

        public Messages Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private int _number;

        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public LevelUpMessage (Messages message, int number)
        {
            _message = message;
            _number = number;
        }
    }
}
