using System;
using System.Collections.Generic;
using System.Text;

namespace Blazout1Core
{
    public class VVRAM
    {
        public const int vvramWidth = 40;
        public const int vvramHeight = 25;

        private char[,] vvram = new char[vvramWidth, vvramHeight];
        public char GetChar(int x, int y)
        {
            return vvram[x, y];
        }
        public void SetChar(int x, int y, char ch)
        {
            vvram[x, y] = ch;
        }

        public VVRAM()
        {
            for (int y = 0; y < vvramHeight; y++)
            {
                for (int x = 0; x < vvramWidth; x++)
                {
                    vvram[x, y] = '　';
                }
            }
        }
    }

    public class GameMain
    {
        public VVRAM Vvram = new VVRAM();




    }
}
