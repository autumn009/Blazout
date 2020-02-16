using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public void FillScreen(char ch)
        {
            for (int y = 0; y < vvramHeight; y++)
            {
                for (int x = 0; x < vvramWidth; x++)
                {
                    vvram[x, y] = ch;
                }
            }
        }
    }

    public class GameMain
    {
        public VVRAM Vvram = new VVRAM();
        public const char WakuChar = '■';
        public const char BlockChar = '□';
        public const char SpaceChar = '　';
        public const char BallChar = '●';
        public int paddleX = 12;
        public int paddleWidth = 5;
        public const int timerInterval = 300;
        public int ballX = 0;
        public int ballY = 0;

        public void DrawPaddle(int xbegin, int width)
        {
            int y = VVRAM.vvramHeight - 1;
            for (int x = 1; x < VVRAM.vvramHeight-1; x++)
            {
                Vvram.SetChar(x, y, SpaceChar);
            }
            for (int i = 0; i < width; i++)
            {
                Vvram.SetChar(i+xbegin, y, WakuChar);
            }
        }
        public void DrawPaddle()
        {
            DrawPaddle(paddleX, paddleWidth);
        }
        public void DrawBall()
        {
            Vvram.SetChar(ballX, ballY, BallChar);
        }
        public void ResetBall()
        {
            ballX = paddleX + paddleWidth / 2;
            ballY = VVRAM.vvramHeight - 2;
        }
        public void MoveBall(int dx, int dy)
        {
            Vvram.SetChar(ballX, ballY, SpaceChar);
            ballX += dx;
            ballY += dy;
            Vvram.SetChar(ballX, ballY, BallChar);
        }

        public void TimerProc(Task task)
        {
            DrawPaddle();

            _ = Task.Delay(timerInterval).ContinueWith(TimerProc);
        }

        public bool buttonLActive = false;
        public bool buttonRActive = false;

        private const string style0 = "width: 20em; height:5em; background-color:gray";
        private const string style1 = "width: 20em; height:5em; background-color:red";

        private string getStyle(bool sw) => sw ? style1 : style0;

        public string lstyle => getStyle(buttonLActive);
        public string rstyle => getStyle(buttonRActive);

        public GameMain()
        {
            Vvram.FillScreen(SpaceChar);
            // draw waku line
            for (int y = 0; y < VVRAM.vvramHeight; y++)
            {
                Vvram.SetChar(0, y, WakuChar);
                Vvram.SetChar(VVRAM.vvramWidth - 1, y, WakuChar);
            }
            for (int x = 0; x < VVRAM.vvramWidth; x++)
            {
                Vvram.SetChar(x, 0, WakuChar);
            }
            DrawPaddle();
            ResetBall();
            DrawBall();

            _ = Task.Delay(timerInterval).ContinueWith(TimerProc);
        }
    }
}
