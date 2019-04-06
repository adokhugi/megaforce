using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class ItemMenu
    {
        public enum States
        {
            DropSelected1 = 0,
            DropSelected2,
            UseSelected1,
            UseSelected2,
            GiveSelected1,
            GiveSelected2,
            EquipSelected1,
            EquipSelected2
        }

        public Texture2D[] Texture = new Texture2D[8];

        private States _currentState;

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public States CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }

        public ItemMenu()
        {
            _currentState = States.UseSelected2;
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
