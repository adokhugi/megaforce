using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Party
    {
        public const int MAXPARTYMEMBERS = 50;
        public const int MAXPARTYMEMBERS_PLAYER = 12;

        private int _numPartyMembers;

        public int NumPartyMembers
        {
            get { return _numPartyMembers; }
            set { _numPartyMembers = value; }
        }

        public Character[] Members = new Character[MAXPARTYMEMBERS];

        private int _memberOnTurn;

        public int MemberOnTurn
        {
            get { return _memberOnTurn; }
            set { _memberOnTurn = value; }
        }

        private CharacterPointer.Sides _side;

        public CharacterPointer.Sides Side
        {
            get { return _side; }
            set { _side = value; }
        }

        public Party(int numPartyMembers, CharacterPointer.Sides side)
        {
            _numPartyMembers = numPartyMembers;
            _side = side;
        }

        public void Draw(SpriteBatch spriteBatch, Map map)
        {
            if (map != null)
                DrawAt(spriteBatch, map, map.Position, false, new Vector2(0, 0));
        }

        public void Draw(SpriteBatch spriteBatch, Map map, Vector2 oldMapPosition)
        {
            if (map != null)
                DrawAt(spriteBatch, map, oldMapPosition, false, new Vector2(0, 0));
        }

        public void Draw(SpriteBatch spriteBatch, Map map, Vector2 oldMapPosition, bool excludeMemberOnTurn)
        {
            if (map != null)
                DrawAt(spriteBatch, map, oldMapPosition, excludeMemberOnTurn, new Vector2(0, 0));
        }

        public void Draw(SpriteBatch spriteBatch, Map map, Vector2 oldMapPosition, bool excludeMemberOnTurn, byte opacity1)
        {
            if (map != null)
                DrawAt(spriteBatch, map, oldMapPosition, excludeMemberOnTurn, new Vector2(0, 0), opacity1);
        }

        public void DrawAt(SpriteBatch spriteBatch, Map map, Vector2 oldMapPosition, bool excludeMemberOnTurn, Vector2 translateVector)
        {
            if (map != null)
                DrawAt(spriteBatch, map, oldMapPosition, excludeMemberOnTurn, translateVector, 255);
        }

        public void DrawAt(SpriteBatch spriteBatch, Map map, Vector2 oldMapPosition, bool excludeMemberOnTurn, Vector2 translateVector, byte opacity1)
        {
            for (int i = 0; i < NumPartyMembers; i++)
            {
                if (Members[i].Alive)
                    if (!excludeMemberOnTurn || !(MemberOnTurn == i))
                        Members[i].DrawAt(spriteBatch, map.CalcPosition(Members[i].Position, oldMapPosition) * new Vector2(Map.TILESIZEX, Map.TILESIZEY) + translateVector + new Vector2(Character.OFFSETX, Character.OFFSETY), true, opacity1);
            }
        }

        public bool IsDefeated()
        {
            bool somebodyIsAlive = false;

            for (int i = 0; i < _numPartyMembers; i++)
            {
                if (Members[i].HitPoints > 0)
                    somebodyIsAlive = true;
                else if (Members[i].MustSurvive)
                    return true;
            }

            return !somebodyIsAlive;
        }

        public bool MustSurviveIsDefeated()
        {
            for (int i = 0; i < _numPartyMembers; i++)
                if (Members[i].HitPoints == 0 && Members[i].MustSurvive)
                    return true;

            return false;
        }

        public void Regenerate()
        {
            for (int i = 0; i < _numPartyMembers; i++)
            {
                if (Members[i].MustSurvive 
                    || Members[i].CharClass == "PHNK" 
                    || Members[i].CharClass == "PHNX")
                    Members[i].Alive = true;

                if (Members[i].Alive)
                    Members[i].Regenerate();
            }
        }

        public void RegenerateFully()
        {
            for (int i = 0; i < _numPartyMembers; i++)
            {
                Members[i].Alive = true;
                Members[i].Regenerate();
            }
        }

        public int GetNumberOfExhaustedLeader()
        {
            for (int i = 0; i < _numPartyMembers; i++)
                if (Members[i].MustSurvive && Members[i].HitPoints == 0)
                    return i;

            return -1;
        }

        public void UnBoost()
        {
            for (int i = 0; i < _numPartyMembers; i++)
                Members[i].UnBoost();
        }

        public void UnAttackBoost()
        {
            for (int i = 0; i < _numPartyMembers; i++)
                Members[i].UnAttackBoost();
        }

        public void Join(Character newMember)
        {
            Members[_numPartyMembers] = newMember;
            _numPartyMembers++;
        }
    }
}
