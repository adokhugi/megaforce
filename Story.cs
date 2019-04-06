using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame2
{
    class Story
    {
        public const int MAXSTORYSIZE = 25000;

        public int pointer;

        private byte[] story = new byte[MAXSTORYSIZE];

        public Story(String Filename)
        {
            story = File.ReadAllBytes(Filename);
            pointer = 0;
        }

        public string GetNextCharacter()
        {
            int tempPtr = pointer;
            while (tempPtr < story.Length && story[tempPtr] != ':' && story[tempPtr] != '\n' && story[tempPtr] != '\r')
                tempPtr++;
            if (tempPtr >= story.Length || story[tempPtr] == '\n' || story[tempPtr] == '\r')
                return null;
            else
            {
                int i;
                string str = "";
                for (i = pointer; i < tempPtr; i++)
                    str += (char) story[i];
                return str;
            }
        }

        public string GetNextLine()
        {
            int pointerOld = pointer;
            while (pointer < story.Length && story[pointer] != '\n' && story[pointer] != '\r')
                pointer++;
            if (pointer == pointerOld || pointer > story.Length)
                return null;
            else
            {
                int i;
                string str = "";
                for (i = pointerOld; i < pointer; i++)
                    str += (char) story[i];
                while (pointer < story.Length && (story[pointer] == ' ' || story[pointer] == '\n' || story[pointer] == '\r'))
                    pointer++;
                return str;
            }
        }
    }
}
