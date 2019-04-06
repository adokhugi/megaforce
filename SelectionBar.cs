using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class SelectionBar
    {
        public const int OFFSETX = -5;
        public const int OFFSETY = -4;

        public Texture2D Texture;
        public Vector2 Position;

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawAt(spriteBatch, Position * new Vector2(Map.TILESIZEX, Map.TILESIZEY) + new Vector2(OFFSETX, OFFSETY));
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 selectionBarPosition)
        {
            DrawAt(spriteBatch, selectionBarPosition * new Vector2(Map.TILESIZEX, Map.TILESIZEY) + new Vector2(OFFSETX, OFFSETY));
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 posXY)
        {
            spriteBatch.Draw(Texture, posXY, Color.White);
        }

        public float PositionInMap(Map map)
        {
            return Map.MAPHEADERSIZE + map.Position.Y * map.Size.X + map.Position.X + Position.Y * map.Size.X + Position.X;
        }
    }
}
