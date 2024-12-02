using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Map
    {
        public const int TILESIZEX = 60;
        public const int TILESIZEY = 48;
        // 2024.12.02 introduced constants
        public const int MAXX = 1 + Game1.PREFERREDBACKBUFFERWIDTH / TILESIZEX;
        public const int MAXY = 1 + Game1.PREFERREDBACKBUFFERHEIGHT / TILESIZEY;
        public const int MAXMAPSIZE = 2500;
        public const int MAPHEADERSIZE = 2;
        public const int MAXPATHLENGTH = 50;
        public const int OPACITY_FULL = 255;
        public const int OPACITY_LESS = 200;
        public const int MAXMAPFOOTERSIZE = 100;
        public const int ENDOFENEMYBLOCKTOKEN = 255;

        public enum DisplayStates
        {
            Error,
            Forest_Default,
            Grassland_Default,
            Hill_Default,
            Mountain_Default,
            Path_Default,
            Sea_Default,
            Sea_Default_Blinks,
            Wall_Default
        }

        public enum Directions
        {
            Left,
            Right,
            Up,
            Down
        }

        public static Texture2D[] Texture = new Texture2D[256];
        public Vector2 Size;
        public Vector2 Position;
        public Vector2 Offset = new Vector2(0, 0);
        public Directions[] currentPath = new Directions[MAXPATHLENGTH];
        public Directions[] bestPath = new Directions[MAXPATHLENGTH];
        
        private int bestPath_numberSteps;

        private byte[] map = new byte[MAXMAPSIZE + MAPHEADERSIZE + MAXMAPFOOTERSIZE];
        private DisplayStates[] mapDisplay = new DisplayStates[MAXMAPSIZE + MAPHEADERSIZE];
        private bool[] mapViable = new bool[MAXMAPSIZE + MAPHEADERSIZE];
        private bool[] mapMarked = new bool[MAXMAPSIZE + MAPHEADERSIZE];

        private bool _blinkStatus;

        public bool BlinkStatus
        {
            get { return _blinkStatus; }
            set { _blinkStatus = value; }
        }

        public Map(String Filename)
        {
            map = File.ReadAllBytes(Filename);
            Size = new Vector2(map[0], map[1]);
            Position = new Vector2(map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y], map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 1]);
            CreateMapDisplay();
            EmptyMapViable();
            EmptyMapMarked();
        }

        public void CreateMapDisplay()
        {
            for (int i = MAPHEADERSIZE; i < MAPHEADERSIZE + Size.X * Size.Y; i++)
            {
                switch ((char)map[i])
                {
                    case 'F':
                        mapDisplay[i] = DisplayStates.Forest_Default;
                        break;

                    case 'G':
                        mapDisplay[i] = DisplayStates.Grassland_Default;
                        break;

                    case 'H':
                        mapDisplay[i] = DisplayStates.Hill_Default;
                        break;

                    case 'M':
                        mapDisplay[i] = DisplayStates.Mountain_Default;
                        break;

                    case 'P':
                        mapDisplay[i] = DisplayStates.Path_Default;
                        break;

                    case 'S':
                        mapDisplay[i] = DisplayStates.Sea_Default;
                        break;

                    case 'W':
                        mapDisplay[i] = DisplayStates.Wall_Default;
                        break;
                }
            }
        }

        public byte DiagonallyAboveLeft(int pos)
        {
            if (pos - Size.X - 1 >= MAPHEADERSIZE && (pos - MAPHEADERSIZE) % Size.X != 0)
                return map[pos - (int)Size.X - 1];
            else
                return 0; // nothing
        }

        public DisplayStates DiagonallyAboveLeftDisplay(int pos)
        {
            if (pos - Size.X - 1 >= MAPHEADERSIZE && (pos - MAPHEADERSIZE) % Size.X != 0)
                return mapDisplay[pos - (int)Size.X - 1];
            else
                return DisplayStates.Error;
        }

        public byte DiagonallyAboveRight(int pos)
        {
            if (pos - Size.X + 1 >= MAPHEADERSIZE && (pos + 1 - MAPHEADERSIZE) % Size.X != 0)
                return map[pos - (int)Size.X + 1];
            else
                return 0; // nothing
        }

        public DisplayStates DiagonallyAboveRightDisplay(int pos)
        {
            if (pos - Size.X + 1 >= MAPHEADERSIZE && (pos + 1 - MAPHEADERSIZE) % Size.X != 0)
                return mapDisplay[pos - (int)Size.X + 1];
            else
                return DisplayStates.Error;
        }

        public byte DiagonallyLeftBelow(int pos)
        {
            if (pos + Size.X - 1 < MAPHEADERSIZE + Size.X * Size.Y && (pos - 1 - MAPHEADERSIZE) % Size.X != 0)
                return map[pos + (int)Size.X - 1];
            else
                return 0; // nothing
        }

        public byte DiagonallyRightBelow(int pos)
        {
            if (pos + Size.X + 1 < MAPHEADERSIZE + Size.X * Size.Y && (pos + 1 - MAPHEADERSIZE) % Size.X != 0)
                return map[pos + (int)Size.X + 1];
            else
                return 0; // nothing
        }

        public byte Above(int pos)
        {
            if (pos - Size.X >= MAPHEADERSIZE)
                return map[pos - (int)Size.X];
            else
                return 0; // nothing
        }

        public DisplayStates AboveDisplay(int pos)
        {
            if (pos - Size.X >= MAPHEADERSIZE)
                return mapDisplay[pos - (int)Size.X];
            else
                return DisplayStates.Error;
        }

        public byte Left(int pos)
        {
            if (pos - 1 >= MAPHEADERSIZE && (pos - MAPHEADERSIZE) % Size.X != 0)
                return map[pos - 1];
            else
                return 0; // nothing
        }

        public DisplayStates LeftDisplay(int pos)
        {
            if (pos - 1 >= MAPHEADERSIZE && pos % Size.X != 0)
                return mapDisplay[pos - 1];
            else
                return DisplayStates.Error;
        }

        public byte Right(int pos)
        {
            if (pos + 1 < MAPHEADERSIZE + Size.X * Size.Y && (pos + 1 - MAPHEADERSIZE) % Size.X != 0)
                return map[pos + 1];
            else
                return 0; // nothing
        }

        public byte Below(int pos)
        {
            if (pos + Size.X < MAPHEADERSIZE + Size.X * Size.Y)
                return map[pos + (int)Size.X];
            else
                return 0; // nothing
        }

        public void EmptyMapViable()
        {
            for (int i = 0; i < MAXMAPSIZE + MAPHEADERSIZE; i++)
                mapViable[i] = false;
        }

        // 2023.04.03 Fixed bug
        public void CalcViable(float position, float move, bool flying, Party otherParty)
        {
            mapViable[(int)position] = true;

            if (move >= 1)
            {
                move--;

                if ((position - MAPHEADERSIZE) - Size.X > 0)
                    CalcViable_Helper(position - Size.X, move, flying, otherParty);
                if ((position - MAPHEADERSIZE) + Size.X < Size.Y * Size.X)
                    CalcViable_Helper(position + Size.X, move, flying, otherParty);
                if ((position - MAPHEADERSIZE) % Size.X != 0)
                    CalcViable_Helper(position - 1, move, flying, otherParty);
                if (position % Size.X != 0)
                    CalcViable_Helper(position + 1, move, flying, otherParty);
            }
        }

        private void CalcViable_Helper(float newPosition, float move, bool flying, Party otherParty)
        {
            int i;
            bool collision = false;

            if (newPosition >= MAPHEADERSIZE && newPosition < MAPHEADERSIZE + Size.X * Size.Y)
            {
                for (i = 0; !collision && i < otherParty.NumPartyMembers; i++)
                    if (otherParty.Members[i].Position == newPosition && otherParty.Members[i].Alive == true)
                        collision = true;

                if (!collision)
                    switch ((char)map[(int)newPosition])
                    {
                        case 'F':
                            if (!flying) move *= 0.7f;
                            CalcViable(newPosition, move, flying, otherParty);
                            break;

                        case 'G':
                            if (!flying) move *= 0.85f;
                            CalcViable(newPosition, move, flying, otherParty);
                            break;

                        case 'H':
                            if (!flying) move *= 0.7f;
                            CalcViable(newPosition, move, flying, otherParty);
                            break;

                        case 'P':
                            CalcViable(newPosition, move, flying, otherParty);
                            break;

                        case 'S':
                            if (flying)
                                CalcViable(newPosition, move, flying, otherParty);
                            break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, Position, false, true);
        }

        public void Draw(SpriteBatch spriteBatch, bool displayMarked)
        {
            Draw(spriteBatch, Position, displayMarked, true);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 topLeft)
        {
            Draw(spriteBatch, topLeft, false, true);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 topLeft, bool displayMarked)
        {
            Draw(spriteBatch, topLeft, displayMarked, true);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 topLeft, bool displayMarked, bool displayViable)
        {
            Draw(spriteBatch, topLeft, displayMarked, displayViable, OPACITY_FULL);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 topLeft, bool displayMarked, bool displayViable, byte opacity1)
        {
            float pointer;
            Vector2 tempOffset = Offset;
            Vector2 frame = new Vector2();
            if (Size.X < MAXX) frame.X = Size.X; else frame.X = MAXX;
            if (Size.Y < MAXY) frame.Y = Size.Y; else frame.Y = MAXY;
            if (tempOffset.X > 0)
            {
                tempOffset.X -= TILESIZEX;
                topLeft.X--;
            }
            if (tempOffset.Y > 0)
            {
                tempOffset.Y -= TILESIZEY;
                topLeft.Y--;
            }
            if (tempOffset.X < 0)
            {
                frame.X++;
                if (tempOffset.X < -TILESIZEX)
                    frame.X++;
            }
            if (tempOffset.Y < 0)
            {
                frame.Y++;
                if (tempOffset.Y < -TILESIZEY)
                    frame.Y++;
            }
            pointer = MAPHEADERSIZE + topLeft.Y * Size.X + topLeft.X;
            for (int i = 0; i < frame.Y; i++)
            {
                for (int j = 0; j < frame.X; j++)
                {
                    byte opacity = opacity1;
                    if (displayViable)
                    {
                        if (_blinkStatus && mapViable[(int)pointer]) 
                            opacity -= OPACITY_FULL - OPACITY_LESS; 
                    }
                    if (displayMarked)
                    {
                        if (_blinkStatus && mapMarked[(int)pointer])
                            opacity -= OPACITY_FULL - OPACITY_LESS;
                    }
                    // 2014.12.02 fix to enable higher resolutions than the default 800x600
                    var texture = Texture[(int)mapDisplay[(int)pointer]];
                    if (texture != null)
                    {
                        spriteBatch.Draw(texture, new Vector2(tempOffset.X + j * TILESIZEX, tempOffset.Y + i * TILESIZEY), new Color(opacity, opacity, opacity, opacity));
                    }
                    pointer++;
                }
                pointer += Size.X - frame.X;
            }
        }

        public Boolean InBoundaries(Vector2 newPos)
        {
            if (newPos.X < 0)
                return false;

            if (newPos.Y < 0)
                return false;

            if (newPos.X + MAXX >= Size.X)
                return false;

            if (newPos.Y + MAXY >= Size.Y)
                return false;

            return true;
        }

        public Boolean AtLeftBorder(Vector2 position)
        {
            if (position.X <= 0)
                return true;

            return false;
        }

        public Boolean AtRightBorder(Vector2 position)
        {
            if (position.X >= MAXX - 1)
                return true;

            return false;
        }

        public Boolean AtTopBorder(Vector2 position)
        {
            if (position.Y <= 0)
                return true;

            return false;
        }

        public Boolean AtBottomBorder(Vector2 position)
        {
            if (position.Y >= MAXY - 1)
                return true;

            return false;
        }

        public Vector2 CalcPosition(float position)
        {
            return CalcPosition(position, Position);
        }

        public Vector2 CalcPosition(float position, Vector2 topLeft)
        {
            Vector2 returnPos;
            float row = (int)((position - MAPHEADERSIZE) / Size.X);

            returnPos.Y = row - topLeft.Y;
            returnPos.X = position - MAPHEADERSIZE - row * Size.X - topLeft.X;

            return returnPos;
        }

        public bool IsViable(float position)
        {
            return mapViable[(int) position];
        }

        public bool IsVisible(float position, Vector2 topLeft)
        {
            Vector2 pos = CalcPosition(position, topLeft);

            if (pos.X < 0)
                return false;

            if (pos.Y < 0)
                return false;

            if (pos.X >= MAXX)
                return false;

            if (pos.Y >= MAXY)
                return false;

            return true;
        }

        public bool IsVisibleMinusBorder(float position, Vector2 topLeft)
        {
            Vector2 pos = CalcPosition(position, topLeft);

            if (pos.X < 3)
                return false;

            if (pos.Y < 3)
                return false;

            if (pos.X > MAXX - 3)
                return false;

            if (pos.Y > MAXY - 3)
                return false;

            return true;
        }

        public bool IsVisibleMinusTopBorder(float position, Vector2 topLeft)
        {
            Vector2 pos = CalcPosition(position, topLeft);

            if (pos.X < 0)
                return false;

            if (pos.Y < 3)
                return false;

            if (pos.X >= MAXX)
                return false;

            if (pos.Y >= MAXY)
                return false;

            return true;
        }

        public bool IsOccupied(float position, CharacterPointer characterPointer, Party party1, Party party2)
        {
            for (int i = 0; i < party1.NumPartyMembers; i++)
                if (characterPointer.BelongsToSide != CharacterPointer.Sides.Player || i != characterPointer.WhichOne)
                    if (party1.Members[i].Alive && position == party1.Members[i].Position)
                        return true;

            for (int i = 0; i < party2.NumPartyMembers; i++)
                if (characterPointer.BelongsToSide != CharacterPointer.Sides.CPU_Opponents || i != characterPointer.WhichOne)
                    if (party2.Members[i].Alive && position == party2.Members[i].Position)
                        return true;

            return false;
        }

        public byte At(float position)
        {
            return map[(int) position];
        }

        public bool AnyCharacterLocatedAt(float position, Party party)
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (position == party.Members[i].Position && party.Members[i].HitPoints > 0)
                    return true;

            return false;
        }

        public int CharacterLocatedAt(float position, Party party)
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (position == party.Members[i].Position && party.Members[i].HitPoints > 0)
                    return i;

            return -1; // none found
        }

        public void EmptyMapMarked()
        {
            for (int i = 0; i < MAXMAPSIZE + MAPHEADERSIZE; i++)
                mapMarked[i] = false;
        }

        public void MarkFieldsWithDistance(float position, int distance)
        {
            MarkFieldsWithDistance(position, distance, false, false, false, false);
        }

        public void MarkFieldsWithDistance(float position, int distance, bool leftConsumed, bool rightConsumed, bool upConsumed, bool downConsumed)
        {
            if (distance == 0)
            {
                mapMarked[(int)position] = true;
            }
            else if (distance == 1)
            {
                if (!rightConsumed && (position - MAPHEADERSIZE) % Size.X != 0 && position - 1 >= MAPHEADERSIZE)
                    mapMarked[(int)position - 1] = true;

                if (!leftConsumed && (position + 1 - MAPHEADERSIZE) % Size.X != 0 && position + 1 <= 1 + Size.X * Size.Y)
                    mapMarked[(int)position + 1] = true;

                if (!downConsumed && position - Size.X >= MAPHEADERSIZE)
                    mapMarked[(int)(position - Size.X)] = true;

                if (!upConsumed && position + Size.X < MAPHEADERSIZE + Size.X * Size.Y)
                    mapMarked[(int)(position + Size.X)] = true;
            }
            else
            {
                if (!rightConsumed && (position - MAPHEADERSIZE) % Size.X != 0 && position - 1 >= MAPHEADERSIZE)
                    MarkFieldsWithDistance(position - 1, distance - 1, true, rightConsumed, upConsumed, downConsumed);

                if (!leftConsumed && (position + 1 - MAPHEADERSIZE) % Size.X != 0 && position + 1 <= 1 + Size.X * Size.Y)
                    MarkFieldsWithDistance(position + 1, distance - 1, leftConsumed, true, upConsumed, downConsumed);

                if (!downConsumed && position - Size.X >= MAPHEADERSIZE)
                    MarkFieldsWithDistance(position - Size.X, distance - 1, leftConsumed, rightConsumed, true, downConsumed);

                if (!upConsumed && position + Size.X < MAPHEADERSIZE + Size.X * Size.Y)
                    MarkFieldsWithDistance(position + Size.X, distance - 1, leftConsumed, rightConsumed, upConsumed, true);
            }
        }

        public bool AnyCharacterLocatedInMarkedFields(Party party)
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (party.Members[i].Alive && mapMarked[(int) party.Members[i].Position])
                    return true;

            return false;
        }

        public int GetNextCharacterPositionLocatedInMarkedFields(Party party, int skip)
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (party.Members[i].Alive && mapMarked[(int)party.Members[i].Position])
                {
                    if (skip == 0)
                        return (int)party.Members[i].Position;
                    else
                        skip--;
                }

            return -1; // not found
        }

        public int GetNextCharacterNumberLocatedInMarkedFields(Party party, int skip)
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (party.Members[i].Alive && mapMarked[(int)party.Members[i].Position])
                {
                    if (skip == 0)
                        return i;
                    else
                        skip--;
                }

            return -1; // not found
        }

        public int GetCharacterNumberLocatedInGivenPosition(Party party, int position)
        {
            for (int i = 0; i < party.NumPartyMembers; i++)
                if (party.Members[i].Alive && (int)party.Members[i].Position == position)
                    return i;

            return -1; // not found
        }

        public void CalcBestPath(int currentPosition, int destinationPosition, int maxStepsToGo)
        {
            bestPath_numberSteps = -1;
            CalcBestPath_Helper(0, currentPosition, destinationPosition, maxStepsToGo);
        }

        public void CalcBestPath_Helper(int currentPositionNumber, int currentPosition, int destinationPosition, int maxStepsToGo)
        {
            int i;

            if (currentPosition == destinationPosition)
            {
                if (bestPath_numberSteps == -1 || bestPath_numberSteps > currentPositionNumber)
                {
                    bestPath_numberSteps = currentPositionNumber;
                    for (i = 0; i <= currentPositionNumber; i++)
                        bestPath[i] = currentPath[i];
                }
            }
            else if (maxStepsToGo > 0)
            {
                if (currentPosition - Size.X >= MAPHEADERSIZE && mapViable[(int)(currentPosition - Size.X)])
                {
                    currentPath[currentPositionNumber] = Directions.Up;
                    CalcBestPath_Helper(currentPositionNumber + 1, (int)(currentPosition - Size.X), destinationPosition, maxStepsToGo - 1);
                }
                if ((currentPosition - MAPHEADERSIZE) % Size.X != 0 && currentPosition - 1 >= MAPHEADERSIZE && mapViable[currentPosition - 1])
                {
                    currentPath[currentPositionNumber] = Directions.Left;
                    CalcBestPath_Helper(currentPositionNumber + 1, currentPosition - 1, destinationPosition, maxStepsToGo - 1);
                }
                if ((currentPosition + 1 - MAPHEADERSIZE) % Size.X != 0 && currentPosition + 1 <= 1 + Size.X * Size.Y && mapViable[currentPosition + 1])
                {
                    currentPath[currentPositionNumber] = Directions.Right;
                    CalcBestPath_Helper(currentPositionNumber + 1, currentPosition + 1, destinationPosition, maxStepsToGo - 1);
                }
                if (currentPosition + Size.X < MAPHEADERSIZE + Size.X * Size.Y && mapViable[(int)(currentPosition + Size.X)])
                {
                    currentPath[currentPositionNumber] = Directions.Down;
                    CalcBestPath_Helper(currentPositionNumber + 1, (int)(currentPosition + Size.X), destinationPosition, maxStepsToGo - 1);
                }
            }
        }

        public int GetBestPathNumberSteps()
        {
            return bestPath_numberSteps;
        }

        public Vector2 CenterMapPosition (int characterPosition)
        {
            int x = (characterPosition - MAPHEADERSIZE) % (int)Size.X;
            int y = (characterPosition - MAPHEADERSIZE) / (int)Size.X;
            int mapPosition_x = x - MAXX / 2;
            int mapPosition_y = y - MAXY / 2;

            if (mapPosition_x < 0)
                mapPosition_x = 0;
            if (mapPosition_y < 0)
                mapPosition_y = 0;
            while (mapPosition_x + MAXX >= Size.X)  // *** >= oder >?
                mapPosition_x--;
            while (mapPosition_y + MAXY >= Size.Y)  // *** >= oder >?
                mapPosition_y--;

            return new Vector2(mapPosition_x, mapPosition_y); 
        }

        public Vector2 CalcClosestMapPositionSoThatVisible(float targetPosition)
        {
            Vector2 targetVector;
            Vector2 returnPosition = Position;
            float row = (int)((targetPosition - MAPHEADERSIZE) / Size.X);

            targetVector.Y = row;
            targetVector.X = targetPosition - MAPHEADERSIZE - row * Size.X;

            if (returnPosition.X > targetVector.X)
                returnPosition.X = targetVector.X;
            else if (returnPosition.X < targetVector.X - MAXX + 1)
                returnPosition.X = targetVector.X - MAXX + 1;

            if (returnPosition.Y > targetVector.Y)
                returnPosition.Y = targetVector.Y;
            else if (returnPosition.Y < targetVector.Y - MAXY + 1)
                returnPosition.Y = targetVector.Y - MAXY + 1;

            return returnPosition;
        }

        public bool MarkAttackableOpponents(Party allies, Party opponents, int minAttackRange, int maxAttackRange)
        {
            bool opponentInRange = false;

            for (int i = MAPHEADERSIZE; i < MAPHEADERSIZE + (int)(Size.X * Size.Y); i++)
            {
                if (IsViable(i))
                {
                    bool collision = false;
                    for (int j = 0; !collision && j < allies.NumPartyMembers; j++)
                        if (j != allies.MemberOnTurn && allies.Members[j].Position == i && allies.Members[j].Alive)
                            collision = true;

                    if (!collision)
                        for (int j = minAttackRange; j <= maxAttackRange; j++)
                        {
                            MarkFieldsWithDistance(i, j);
                            if (AnyCharacterLocatedInMarkedFields(opponents))
                                opponentInRange = true;
                        }
                }
            }

            return opponentInRange;
        }

        public bool IsMarked(float position)
        {
            return mapMarked[(int)position];
        }

        public int CalcPositionWhereToMove(Party enemies, Party party, int charNumber, int minAttackRange, int maxAttackRange)
        {
            int backUpPosition = -1;
            int bestPath_numberSteps = -1;
            int skip;

            for (int i = MAPHEADERSIZE; i < MAPHEADERSIZE + (int)(Size.X * Size.Y); i++)
            {
                if (IsViable(i))
                {
                    bool collision = false;
                    for (int j = 0; !collision && j < enemies.NumPartyMembers; j++)
                        if (j != enemies.MemberOnTurn && enemies.Members[j].Position == i && enemies.Members[j].Alive)
                            collision = true;

                    if (!collision)
                    {
                        EmptyMapMarked();
                        for (int j = minAttackRange; j <= maxAttackRange; j++)
                        {
                            MarkFieldsWithDistance(i, j);
                            if (AnyCharacterLocatedInMarkedFields(party))
                            {
                                skip = 0;
                                while (charNumber != GetNextCharacterNumberLocatedInMarkedFields(party, skip) && GetNextCharacterPositionLocatedInMarkedFields(party, skip) != -1)
                                    skip++;
                                if (charNumber == GetNextCharacterNumberLocatedInMarkedFields(party, skip))
                                {
                                    CalcBestPath((int)enemies.Members[enemies.MemberOnTurn].Position, i, enemies.Members[enemies.MemberOnTurn].MovePoints);
                                    if (bestPath_numberSteps == -1
                                        || bestPath_numberSteps > GetBestPathNumberSteps())
                                    {
                                        backUpPosition = i;
                                        bestPath_numberSteps = GetBestPathNumberSteps();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 2024.12.02 this should prevent occasional bugs where the enemy moves erratically and the game crashes
            return backUpPosition != -1 ? backUpPosition : (int)enemies.Members[enemies.MemberOnTurn].Position;
        }

        public void MarkViable()
        {
            for (int i = MAPHEADERSIZE; i < MAXMAPSIZE + MAPHEADERSIZE; i++)
                if (mapViable[i])
                    mapMarked[i] = true;
        }

        public void CalcViableFromMarked(int move, bool flying, Party otherParty)
        {
            for (int i = MAPHEADERSIZE; i < MAXMAPSIZE + MAPHEADERSIZE; i++)
                if (mapMarked[i])
                    CalcViable(i, move, flying, otherParty);
        }

        public int FollowBestPath(int currentPosition, Party currentParty)
        {
            return FollowBestPath(0, currentPosition, currentParty);
        }

        public int FollowBestPath(int currentPositionNumber, int currentPosition, Party currentParty)
        {
            int newPosition = currentPosition;

            switch (bestPath[currentPositionNumber])
            {
                case Directions.Up:
                    newPosition -= (int)Size.X;
                    break;

                case Directions.Left:
                    newPosition--;
                    break;

                case Directions.Right:
                    newPosition++;
                    break;

                case Directions.Down:
                    newPosition += (int)Size.X;
                    break;
            }

            if (IsViable(newPosition) && !IsOccupiedByParty(newPosition, currentParty))
                return FollowBestPath(currentPositionNumber + 1, newPosition, currentParty);
            else
                return currentPosition;
        }

        public bool IsOccupiedByParty(int newPosition, Party currentParty)
        {
            for (int i = 0; i < currentParty.NumPartyMembers; i++)
                if (currentParty.Members[i].Position == newPosition)
                    return true;

            return false;
        }

        public bool MarkHealableAllies(Party allies, int minRange, int maxRange)
        {
            return MarkAttackableOpponents(allies, allies, minRange, maxRange);
        }

        public bool MustSurviveCharacterInProximity(Party allies, int minRange, int maxRange)
        {
            int skip;

            for (int i = MAPHEADERSIZE; i < MAPHEADERSIZE + (int)(Size.X * Size.Y); i++)
            {
                if (IsViable(i))
                {
                    bool collision = false;
                    for (int j = 0; !collision && j < allies.NumPartyMembers; j++)
                        if (j != allies.MemberOnTurn && allies.Members[j].Position == i && allies.Members[j].Alive)
                            collision = true;

                    if (!collision)
                    {
                        EmptyMapMarked();
                        for (int j = minRange; j <= maxRange; j++)
                        {
                            MarkFieldsWithDistance(i, j);
                            if (AnyCharacterLocatedInMarkedFields(allies))
                            {
                                skip = 0;
                                while (GetNextCharacterPositionLocatedInMarkedFields(allies, skip) != -1
                                       && !allies.Members[GetNextCharacterNumberLocatedInMarkedFields(allies, skip)].MustSurvive)
                                    skip++;
                                if (GetNextCharacterPositionLocatedInMarkedFields(allies, skip) != -1
                                     && allies.Members[GetNextCharacterNumberLocatedInMarkedFields(allies, skip)].MustSurvive)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public int GetStartingPositionOfPlayerPartyMember(int number)
        {
            return MAPHEADERSIZE + (int)Size.X * (map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 2 * number + 2 + 1]) + map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 2 * number + 2];
        }

        public int GetNumberOfEnemies()
        {
            int returnValue = 0;
            int i = MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 2 + 2 * Party.MAXPARTYMEMBERS_PLAYER;
            while (map[i] != ENDOFENEMYBLOCKTOKEN)
            {
                i += 3;
                returnValue++;
            }
            return returnValue;
        }

        public int GetEnemyId(int number)
        {
            return map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 2 + 2 * Party.MAXPARTYMEMBERS_PLAYER + 3 * number];
        }

        public int GetStartingPositionOfEnemyPartyMember(int number)
        {
            return MAPHEADERSIZE + (int)Size.X * (map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 2 + 2 * Party.MAXPARTYMEMBERS_PLAYER + 3 * number + 2]) + map[MAPHEADERSIZE + (int)Size.X * (int)Size.Y + 2 + 2 * Party.MAXPARTYMEMBERS_PLAYER + 3 * number + 1];
        }
    }
}
