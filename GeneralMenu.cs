using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class GeneralMenu
    {
        public enum States
        {
            QuitSelected1   = 0,
            QuitSelected2,
            MemberSelected1,
            MemberSelected2,
            MapSelected1,
            MapSelected2,
            SpeedSelected1,
            SpeedSelected2
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

        public GeneralMenu()
        {
            _currentState = States.MemberSelected2;
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
