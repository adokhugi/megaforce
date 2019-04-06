using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class StatsBar
    {
        public const int TEXTUREMIDDLE_NUMBERLAYERS = 6;

        public Vector2 Position;

        public Texture2D TextureLeft;
        public Texture2D[] TextureMiddle = new Texture2D[TEXTUREMIDDLE_NUMBERLAYERS];
        public Texture2D TextureRight;

        public int[] Width = new int[TEXTUREMIDDLE_NUMBERLAYERS];

        private int _totalWidth;

        public int TotalWidth
        {
            get { return _totalWidth; }
            set { _totalWidth = value; }
        }

        public void CalculateWidth(int hitPoints, int maxHitPoints)
        {
            float scaleFactor = 2.5f;
            
            _totalWidth = 0;

            for (int i = TEXTUREMIDDLE_NUMBERLAYERS - 1; i >= 1; i--)
            {
                if (hitPoints > (i - 1) * 100)
                {
                    Width[i] = (int)((hitPoints - (i - 1) * 100) * scaleFactor) - _totalWidth;
                    _totalWidth += Width[i];
                    hitPoints = (i - 1) * 100;
                }
                else
                    Width[i] = 0;
            }

            int temp = 100;
            if (maxHitPoints < 100)
                temp = maxHitPoints;
            Width[0] = (int)(scaleFactor * temp) - _totalWidth;

            _totalWidth += Width[0];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, Position);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 posXY)
        {
            if (_totalWidth > 0)
            {
                spriteBatch.Draw(TextureLeft, posXY, Color.White);
                posXY.X += TextureLeft.Width;
                for (int i = TEXTUREMIDDLE_NUMBERLAYERS - 1; i >= 0; i--)
                    if (Width[i] > 0)
                    {
                        spriteBatch.Draw(TextureMiddle[i], new Rectangle((int)posXY.X, (int)posXY.Y, Width[i], TextureMiddle[i].Height), Color.White);
                        posXY.X += Width[i];
                    }
                spriteBatch.Draw(TextureRight, posXY, Color.White);
            }
        }
    }
}
