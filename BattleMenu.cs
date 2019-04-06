using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class BattleMenu
    {
        public enum States
        {
            StaySelected1   = 0,
            StaySelected2,
            AttackSelected1,
            AttackSelected2,
            MagicSelected1,
            MagicSelected2,
            ItemSelected1,
            ItemSelected2
        }

        public Texture2D[] Texture = new Texture2D[8];

        private States _currentState;

        public States CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public BattleMenu()
        {
            _currentState = States.StaySelected2;
            _position = new Vector2(310f, 480f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, _position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Texture[(int)_currentState], position, Color.White);
        }
    }
}
