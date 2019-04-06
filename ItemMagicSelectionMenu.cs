using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class ItemMagicSelectionMenu
    {
        public enum States
        {
            TopSelected1 = 0,
            TopSelected2,
            LeftSelected1,
            LeftSelected2,
            RightSelected1,
            RightSelected2,
            BottomSelected1,
            BottomSelected2
        }

        public Texture2D Texture;

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

        public ItemMagicSelectionMenu()
        {
            _currentState = States.TopSelected2;
            _position = new Vector2(340f, 480f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, _position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Texture, position, Color.White);
        }
    }
}
