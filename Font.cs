using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Font
    {
        public Texture2D[] Characters = new Texture2D[256];

        private Vector2 _size;

        public Vector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private Vector2 _position;

	    public Vector2 Position
	    {
		    get { return _position;}
		    set { _position = value;}
	    }

        public float[] WidthOfCharacter = new float[256];

        public Font(Vector2 size)
        {
            _size = size;
        }

        public void CalcWidthOfCharacters()
        {
            for (int i = 0; i < 256; i++)
                if (Characters[i] != null)
                    WidthOfCharacter[i] = Characters[i].Width + 1;
                else
                    WidthOfCharacter[i] = 0;                   
        }

        public void Print(SpriteBatch spriteBatch, string text)
        {
            PrintAt(spriteBatch, text, Position, Color.White, false);
        }

        public void Print(SpriteBatch spriteBatch, string text, bool proportional)
        {
            PrintAt(spriteBatch, text, Position, Color.White, proportional);
        }

        public void Print(SpriteBatch spriteBatch, string text, Color color)
        {
            PrintAt(spriteBatch, text, Position, color, false);
        }

        public void Print(SpriteBatch spriteBatch, string text, Color color, bool proportional)
        {
            PrintAt(spriteBatch, text, Position, color, proportional);
        }

        public void PrintAt(SpriteBatch spriteBatch, string text, Vector2 position)
        {
            PrintAt(spriteBatch, text, position, Color.White, false);
        }
        
        public void PrintAt(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            PrintAt(spriteBatch, text, position, color, false);
        }

        public void PrintAt(SpriteBatch spriteBatch, string text, Vector2 position, Color color, bool proportional)
        {
            for (int i = 0; i < text.Length; i++)
            {
                spriteBatch.Draw(Characters[text.ToCharArray()[i]], position, color);
                if (proportional)
                    position.X += WidthOfCharacter[text.ToCharArray()[i]];
                else
                    position.X += _size.X;
            }
        }

        public void Println(SpriteBatch spriteBatch, string text)
        {
            PrintAt(spriteBatch, text, Position, Color.White, false);
            _position.Y += _size.Y;
        }

        public void Println(SpriteBatch spriteBatch, string text, bool proportional)
        {
            PrintAt(spriteBatch, text, Position, Color.White, proportional);
            _position.Y += _size.Y;
        }

        public void Println(SpriteBatch spriteBatch, string text, Color color)
        {
            PrintAt(spriteBatch, text, Position, color, false);
            _position.Y += _size.Y;
        }

        public void PrintlnAt(SpriteBatch spriteBatch, string text, Vector2 position)
        {
            PrintAt(spriteBatch, text, position, Color.White, false);
            _position.Y += _size.Y;
        }

        public void PrintlnAt(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
        {
            PrintAt(spriteBatch, text, position, color, false);
            _position.Y += _size.Y;
        }

        public void PrintlnAt(SpriteBatch spriteBatch, string text, Vector2 position, Color color, bool proportional)
        {
            PrintAt(spriteBatch, text, position, color, proportional);
            _position.Y += _size.Y;
        }
    }
}
