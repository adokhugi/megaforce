using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Arrowforward
    {
        public Texture2D Texture;

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private bool _visible;

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public Arrowforward()
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, Position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            if (_visible)
                spriteBatch.Draw(Texture, position, Color.White);
        }
    }
}
