using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Spell
    {
        public static Texture2D TextureSpellSelected;
        public static Texture2D TextureEgress;
        public static Texture2D TextureBlaze;
        public static Texture2D TextureMuddle;
        public static Texture2D TextureDispel;
        public static Texture2D TextureDesoul;
        public static Texture2D TextureBolt;
        public static Texture2D TextureKaton;
        public static Texture2D TextureSlow;
        public static Texture2D TextureBoost;
        public static Texture2D TextureDetox;
        public static Texture2D TextureHeal;
        public static Texture2D TextureAura;
        public static Texture2D TextureBlast;

        public Texture2D Texture;

        public enum Types
        {
            Attack,
            Heal,
            Other
        }

        public enum AreaEffectTypes
        {
            Default,
            Divide
        }

        private Types _type;

        public Types Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public enum Elements
        {
            Fire,
            Ice,
            Thunder,
            None
        }

        private Elements _element;

        public Elements Element
        {
            get { return _element; }
            set { _element = value; }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _level;

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        private AreaEffectTypes _areaEffectType;

        public AreaEffectTypes AreaEffectType
        {
            get { return _areaEffectType; }
            set { _areaEffectType = value; }
        }

        public int[] EffectPoints = new int[4];

        public int[] MagicPoints = new int[4];

        public int[] MinRange = new int[4];

        public int[] MaxRange = new int[4];

        public int[] Area = new int[4];

        public static int CurrentState;

        public Spell(string name, int level)
        {
            _name = name;
            CurrentState = 0;
            _level = level;

            // *** all spells are to be added here
            switch (name)
            {
                case "EGRESS":
                    Texture = TextureEgress;
                    _type = Types.Other;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 0;
                    MagicPoints[0] = 8;
                    MinRange[0] = 0;
                    MaxRange[0] = 0;
                    Area[0] = 1;
                    break;

                case "BOOST":
                    Texture = TextureBoost;
                    _type = Types.Heal;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 0;
                    MagicPoints[0] = 2;     // *** temporarily, must be checked
                    MinRange[0] = 0;
                    MaxRange[0] = 1;        // *** temporarily, must be checked
                    Area[0] = 2;
                    EffectPoints[1] = 0;
                    MagicPoints[1] = 5;     // *** temporarily, must be checked
                    MinRange[1] = 0;
                    MaxRange[1] = 3;        // *** temporarily, must be checked
                    Area[1] = 3;
                    // ***
                    break;

                case "DETOX":
                    // *** magic points etc. are missing
                    Texture = TextureDetox;
                    _type = Types.Heal;
                    _areaEffectType = AreaEffectTypes.Default;
                    break;

                case "HEAL":
                    Texture = TextureHeal;
                    _type = Types.Heal;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 18;
                    MagicPoints[0] = 3;
                    MinRange[0] = 0;
                    MaxRange[0] = 1;
                    Area[0] = 1;
                    EffectPoints[1] = 18;
                    MagicPoints[1] = 5;
                    MinRange[1] = 0;
                    MaxRange[1] = 2;
                    Area[1] = 1;
                    EffectPoints[2] = 37;
                    MagicPoints[2] = 10;
                    MinRange[2] = 0;
                    MaxRange[2] = 3;
                    Area[2] = 1;
                    EffectPoints[3] = 500;
                    MagicPoints[3] = 20;
                    MinRange[3] = 0;
                    MaxRange[3] = 1;
                    Area[3] = 1;
                    break;

                case "AURA":
                    Texture = TextureAura;
                    _type = Types.Heal;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 18;
                    MagicPoints[0] = 7;
                    MinRange[0] = 0;
                    MaxRange[0] = 3;
                    Area[0] = 2;
                    EffectPoints[1] = 18;
                    MagicPoints[1] = 11;
                    MinRange[1] = 0;
                    MaxRange[1] = 3;
                    Area[1] = 3;
                    EffectPoints[2] = 37;
                    MagicPoints[2] = 15;
                    MinRange[2] = 0;
                    MaxRange[2] = 3;
                    Area[2] = 3;
                    EffectPoints[3] = 500;
                    MagicPoints[3] = 20;
                    MinRange[3] = 0;
                    MaxRange[3] = 0;
                    Area[3] = 1;
                    break;

                case "SLOW":
                    // *** magic points etc. are missing
                    Texture = TextureSlow;
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    break;

                case "BLAST":
                    Texture = TextureBlast;
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    // *** magic points etc. are missing
                    break;

                case "BLAZE":
                    Texture = TextureBlaze;
                    _type = Types.Attack;
                    _element = Elements.Fire;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 6; // *** temporarily; value is to be fixed
                    MagicPoints[0] = 2;
                    MinRange[0] = 1;
                    MaxRange[0] = 2;
                    Area[0] = 1;
                    EffectPoints[1] = 10; // *** temporarily; value is to be fixed
                    MagicPoints[1] = 6;
                    MinRange[1] = 1;
                    MaxRange[1] = 2;
                    Area[1] = 2;
                    EffectPoints[2] = 20; // *** temporarily; value is to be fixed
                    MagicPoints[2] = 10;
                    MinRange[2] = 1;
                    MaxRange[2] = 2;
                    Area[2] = 2;
                    EffectPoints[3] = 50; // *** temporarily; value is to be fixed
                    MagicPoints[3] = 10;
                    MinRange[3] = 1;
                    MaxRange[3] = 2;
                    Area[3] = 1;
                    break;

                case "MUDDLE":
                    Texture = TextureMuddle;
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    // ***
                    break;

                case "DISPEL":
                    Texture = TextureDispel;
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    // ***
                    break;

                case "DESOUL":
                    Texture = TextureDesoul;
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    // ***
                    break;

                case "BOLT":
                    // *** magic points etc. are missing
                    Texture = TextureBolt;
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 15; // *** temporarily; value is to be fixed
                    MagicPoints[0] = 8;
                    MinRange[0] = 1;
                    MaxRange[0] = 2;
                    Area[0] = 2;
                    EffectPoints[1] = 18; // *** temporarily; value is to be fixed
                    MagicPoints[1] = 15;
                    MinRange[1] = 1;
                    MaxRange[1] = 3;
                    Area[1] = 3;
                    break;

                case "KATON":
                    Texture = TextureKaton;
                    _type = Types.Attack;
                    _element = Elements.Fire;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 15; // *** temporarily; value is to be fixed
                    MagicPoints[0] = 6;
                    MinRange[0] = 1;
                    MaxRange[0] = 2;
                    Area[0] = 2;
                    // ***
                    break;

                case "RAIJIN":
                    // *** texture, magic points etc. are missing
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Default;
                    EffectPoints[0] = 20; // *** temporarily; value is to be fixed
                    MagicPoints[0] = 15;
                    MinRange[0] = 1;
                    MaxRange[0] = 3;
                    Area[0] = 3;
                    // ***
                    break;

                case "DAO":
                    // *** texture, magic points etc. are missing
                    _type = Types.Attack;
                    _areaEffectType = AreaEffectTypes.Divide;
                    EffectPoints[0] = 20; // *** temporarily; value is to be fixed
                    MagicPoints[0] = 8;
                    MinRange[0] = 1;
                    MaxRange[0] = 2;
                    Area[0] = 2;
                    // ***
                    break;

                case "APOLLO":
                    // *** texture, magic points etc. are missing
                    _type = Types.Attack;
                    _element = Elements.Fire;
                    _areaEffectType = AreaEffectTypes.Divide;
                    EffectPoints[0] = 30; // *** temporarily; value is to be fixed
                    MagicPoints[0] = 10;
                    MinRange[0] = 1;
                    MaxRange[0] = 2;
                    Area[0] = 2;
                    // ***
                    break;

                default:
                    // *** temporarily
                    _type = Types.Attack;
                    _element = Elements.None;
                    _areaEffectType = AreaEffectTypes.Default;
                    MinRange[0] = 1;
                    MaxRange[0] = 2;
                    break;
            }
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position)
        {
            DrawAt(spriteBatch, position, false);
        }

        public void DrawAt(SpriteBatch spriteBatch, Vector2 position, bool selected)
        {
            spriteBatch.Draw(Texture, position, Color.White);

            if (selected && CurrentState == 1)
                spriteBatch.Draw(TextureSpellSelected, position, Color.White);
        }
    }
}
