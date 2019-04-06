using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Redbar
    {
        public Texture2D Texture;

        public static int CurrentState = 0;

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Redbar()
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, _position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            if (CurrentState == 1)
                spriteBatch.Draw(Texture, position, Color.White);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position, int width, int height)
        {
            if (CurrentState == 1)
                spriteBatch.Draw(Texture, new Rectangle((int)position.X, (int)position.Y, width, height), Color.White);
        }
    }
}
