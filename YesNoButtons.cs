using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class YesNoButtons
    {
        public enum States
        {
            YesSelected1 = 0,
            YesSelected2,
            NoSelected1,
            NoSelected2
        }

        public Texture2D[] TextureYes = new Texture2D[2];
        public Texture2D[] TextureNo = new Texture2D[2];

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private States _currentState;

        public States CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        public YesNoButtons()
        {
            _currentState = States.YesSelected2;
        }

        public YesNoButtons(Vector2 position)
        {
            _currentState = States.YesSelected2;
            _position = position;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, Position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            if (_currentState == States.YesSelected2)
                spriteBatch.Draw(TextureYes[1], position, Color.White);
            else
                spriteBatch.Draw(TextureYes[0], position, Color.White);

            if (_currentState == States.NoSelected2)
                spriteBatch.Draw(TextureNo[1], position + new Vector2(100, 0), Color.White);
            else
                spriteBatch.Draw(TextureNo[0], position + new Vector2(100, 0), Color.White);
        }
    }
}
