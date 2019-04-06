using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Box
    {
        public static Texture2D Texture_TopLeft;
        public static Texture2D Texture_TopMiddle;
        public static Texture2D Texture_TopRight;
        public static Texture2D Texture_MiddleLeft;
        public static Texture2D Texture_MiddleMiddle;
        public static Texture2D Texture_MiddleRight;
        public static Texture2D Texture_BottomLeft;
        public static Texture2D Texture_BottomMiddle;
        public static Texture2D Texture_BottomRight;

        public Texture2D OverrideTexture_TopMiddle;
        public Texture2D OverrideTexture_MiddleLeft;
        public Texture2D OverrideTexture_MiddleMiddle;
        public Texture2D OverrideTexture_MiddleRight;
        public Texture2D OverrideTexture_BottomMiddle;

        private Vector2 _size;

        public Vector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Box()
        {
        }

        public Box(Vector2 size)
        {
            _size = size;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, Position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            Vector2 tempPosition = position;

            spriteBatch.Draw(Texture_TopLeft, tempPosition, Color.White);
            tempPosition.X += Texture_TopLeft.Width;
            for (int i = 0; i < _size.X - Texture_TopLeft.Width - Texture_TopRight.Width; i++)
            {
                if (OverrideTexture_TopMiddle != null)
                    spriteBatch.Draw(OverrideTexture_TopMiddle, tempPosition, Color.White);
                else
                    spriteBatch.Draw(Texture_TopMiddle, tempPosition, Color.White);
                tempPosition.X++;
            }
            spriteBatch.Draw(Texture_TopRight, tempPosition, Color.White);
            tempPosition.Y += Texture_TopLeft.Height;

            for (int i = 0; i < _size.Y - Texture_TopLeft.Height - Texture_BottomRight.Height; i++)
            {
                tempPosition.X = position.X;
                if (OverrideTexture_MiddleLeft != null)
                    spriteBatch.Draw(OverrideTexture_MiddleLeft, tempPosition, Color.White);
                else
                    spriteBatch.Draw(Texture_MiddleLeft, tempPosition, Color.White);
                tempPosition.X += Texture_MiddleLeft.Width;
                for (int j = 0; j < _size.X - Texture_MiddleLeft.Width - Texture_MiddleRight.Width; j++)
                {
                    if (OverrideTexture_MiddleMiddle != null)
                        spriteBatch.Draw(OverrideTexture_MiddleMiddle, tempPosition, Color.White);
                    else
                        spriteBatch.Draw(Texture_MiddleMiddle, tempPosition, Color.White);
                    tempPosition.X++;
                }
                if (OverrideTexture_MiddleRight != null)
                    spriteBatch.Draw(OverrideTexture_MiddleRight, tempPosition, Color.White);
                else
                    spriteBatch.Draw(Texture_MiddleRight, tempPosition, Color.White);
                tempPosition.Y += Texture_MiddleLeft.Height;
            }

            tempPosition.X = position.X;
            spriteBatch.Draw(Texture_BottomLeft, tempPosition, Color.White);
            tempPosition.X += Texture_BottomLeft.Width;
            for (int i = 0; i < _size.X - Texture_BottomLeft.Width - Texture_BottomRight.Width; i++)
            {
                if (OverrideTexture_BottomMiddle != null)
                    spriteBatch.Draw(OverrideTexture_BottomMiddle, tempPosition, Color.White);
                else
                    spriteBatch.Draw(Texture_BottomMiddle, tempPosition, Color.White);
                tempPosition.X++;
            }
            spriteBatch.Draw(Texture_BottomRight, tempPosition, Color.White);
        }
    }
}
