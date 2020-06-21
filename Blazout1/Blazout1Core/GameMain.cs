using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Blazout1Core
{
    public class PARAMS
    {
        public static string VvramWidth { get; set; } = VVRAM.vvramDefaultWidth.ToString();
        public static string VvramHeight { get; set; } = VVRAM.vvramDefaultHeight.ToString();
    }

    public class VVRAM
    {
        public const int vvramDefaultWidth = 40;
        public const int vvramDefaultHeight = 25;
        public static int vvramWidth
        {
            get
            {
                if (int.TryParse(PARAMS.VvramWidth, out int x))
                    return x;
                else
                    return vvramDefaultWidth;
            }
        }
        public static int vvramHeight
        {
            get
            {
                if (int.TryParse(PARAMS.VvramHeight, out int y))
                    return y;
                else
                    return vvramDefaultHeight;
            }

        }

        private char[,] vvram = new char[vvramWidth, vvramHeight];
        public char GetChar(int x, int y)
        {
            if (x < 0 || x >= vvramWidth || y < 0 || y >= vvramHeight) return GameMain.SpaceChar;
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

        internal void SetString(int x, int y, string s)
        {
            foreach (var c in s)
            {
                SetChar(x++, y, c);
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
        public const int timerInterval = 100;
        public int ballX = 0;
        public int ballY = 0;
        public int ballDX = -1;
        public int ballDY = -1;
        public object UI = null;
        public int Score = 0;
        public int LeftBlocks = 0;
        public bool IsCheat = false;

        private void mainUpdate()
        {
            dynamic dui = UI;
            dui?.Update();
        }

        private int turnRightX()
        {
            if (ballDX == -1 && ballDY == -1) return 1;
            if (ballDX == 1 && ballDY == -1) return 1;
            if (ballDX == 1 && ballDY == 1) return -1;
            return -1;
        }
        private int turnRightY()
        {
            if (ballDX == -1 && ballDY == -1) return -1;
            if (ballDX == 1 && ballDY == -1) return 1;
            if (ballDX == 1 && ballDY == 1) return 1;
            return -1;
        }
        private int turnLeftX()
        {
            if (ballDX == -1 && ballDY == -1) return -1;
            if (ballDX == 1 && ballDY == -1) return -1;
            if (ballDX == 1 && ballDY == 1) return 1;
            return 1;
        }
        private int turnLeftY()
        {
            if (ballDX == -1 && ballDY == -1) return 1;
            if (ballDX == 1 && ballDY == -1) return -1;
            if (ballDX == 1 && ballDY == 1) return -1;
            return 1;
        }

        public void DrawPaddle(int xbegin, int width)
        {
            int y = VVRAM.vvramHeight - 1;
            for (int x = 1; x < VVRAM.vvramWidth - 1; x++)
            {
                if (IsCheat)
                    Vvram.SetChar(x, y, WakuChar);
                else
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
            ballDX = -1;
            ballDY = -1;
        }
        public void MoveBall(int dx, int dy)
        {
            Vvram.SetChar(ballX, ballY, SpaceChar);
            // input delta drift
            int ddx = 0;
            if (ballY == VVRAM.vvramHeight - 2)
            {
                if (buttonLActive) ddx = -1;
                if (buttonRActive) ddx = 1;
            }
            ballX += dx;
            if (ballX+ddx >= 1 && ballX+ddx < VVRAM.vvramWidth - 1) ballX += ddx;
            ballY += dy;
            Vvram.SetChar(ballX, ballY, BallChar);
        }

        public void TimerProc(Task task)
        {
            // paddle move
            int dx = 0;
            if (buttonLActive && paddleX >= 2) dx = -1;
            if (buttonRActive && paddleX <= VVRAM.vvramWidth - 2 - paddleWidth) dx = 1;
            paddleX += dx;
            DrawPaddle();

            // ball move
            int nextX = ballX + ballDX;
            int nextY = ballY + ballDY;
            if (Vvram.GetChar(nextX, nextY) == SpaceChar)
            {
                MoveBall(ballDX, ballDY);
            }
            else
            {
                // reflect ball
                int X1 = turnRightX();
                int Y1 = turnRightY();
                int X2 = turnLeftX();
                int Y2 = turnLeftY();

                bool v1 = Vvram.GetChar(ballX + X1, ballY + Y1) == SpaceChar;
                bool v2 = Vvram.GetChar(ballX + X2, ballY + Y2) == SpaceChar;
                if (v1 ^ v2)
                {
                    if (v1)
                    {
                        ballDX = X1;
                        ballDY = Y1;
                    }
                    else
                    {
                        ballDX = X2;
                        ballDY = Y2;
                    }
                }
                else
                {
                    // reflect return
                    ballDX = -ballDX;
                    ballDY = -ballDY;
                }
                MoveBall(ballDX, ballDY);
                if (Vvram.GetChar(nextX, nextY) == BlockChar)
                {
                    Vvram.SetChar(nextX, nextY, SpaceChar);
                    switch (nextY)
                    {
                        case 3:
                        case 4:
                            Score += 7;
                            break;
                        case 5:
                        case 6:
                            Score += 5;
                            break;
                        case 7:
                        case 8:
                            Score += 3;
                            break;
                        default:
                            Score += 1;
                            break;
                    }
                    LeftBlocks--;
                }
            }

            // game over check
            if ( ballY >= VVRAM.vvramHeight-1)
            {
                isPlaying = false;
                Vvram.SetString(12, 12, "　　　　　　　　　　　");
                Vvram.SetString(12, 13, "　ＧＡＭＥ　ＯＶＥＲ　");
                Vvram.SetString(12, 14, "　　　　　　　　　　　");
                // update screen
                mainUpdate();
            }
            else if (LeftBlocks == 0)
            {
                isPlaying = false;
                Vvram.SetString(12, 12, "　　　　　　　　　　　");
                Vvram.SetString(12, 13, "　　ＹＯＵ　ＷＩＮ　　");
                Vvram.SetString(12, 14, "　　　　　　　　　　　");
                // update screen
                mainUpdate();
            }
            else
            {
                // update screen
                mainUpdate();

                _ = Task.Delay(timerInterval).ContinueWith(TimerProc);
            }
        }

        public bool buttonLActive = false;
        public bool buttonRActive = false;

        private const string style0 = "width: 20em; height:5em; background-color:gray";
        private const string style1 = "width: 20em; height:5em; background-color:red";

        private string getStyle(bool sw) => sw ? style1 : style0;

        public string lstyle => getStyle(buttonLActive);
        public string rstyle => getStyle(buttonRActive);

        private bool isPlaying = false;

        public string PlayAgainVisibility => isPlaying ? "visibility:hidden" : "visibility:visible";


        public GameMain()
        {
            isPlaying = true;
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

            for (int y = 3; y < 3+8; y++)
            {
                for (int x = 1; x < VVRAM.vvramWidth-1; x++)
                {
                    Vvram.SetChar(x, y, BlockChar);
                    LeftBlocks++;
                }
            }

            DrawPaddle();
            ResetBall();
            DrawBall();

            mainUpdate();
            _ = Task.Delay(timerInterval).ContinueWith(TimerProc);
        }
    }
}
