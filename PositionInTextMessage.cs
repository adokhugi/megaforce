using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsGame2
{
    class PositionInTextMessage
    {
        private int _row;

        public int Row
        {
            get { return _row; }
            set { _row = value; }
        }

        private int _column;

        public int Column
        {
            get { return _column; }
            set { _column = value; }
        }
    }
}
