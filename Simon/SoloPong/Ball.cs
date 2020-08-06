namespace SoloPong
{
    using System;
    using System.Threading;
    using System.Timers;
    using Meadow.Foundation;

    public class Ball
    {
        public class GameOverArgs : EventArgs
        {
        }

        public delegate void NotifyGameOver(object sender, GameOverArgs args);
        public event NotifyGameOver ExplosionOccurred;

        private const int MOVE_INTERVAL = 200;

        private readonly AsyncGraphics asyncGraphics;
        private readonly System.Timers.Timer moveTimer = new System.Timers.Timer(Ball.MOVE_INTERVAL);
        private readonly Paddle paddle;
        private readonly Random random = new Random();
        private readonly ISounds soundGenerator;
        private readonly ScoreKeeper scoreKeeper;

        private readonly Color backgroundColor;
        private readonly int maxX;
        private readonly int maxY;
        private readonly int minY;
        private readonly int width = 10;
        private readonly int height = 10;
        private readonly int displayWidth;

        private readonly Color color = Color.Red;

        private int xPosition = 5;
        private int yPosition;
        private int xIncrement;
        private int yIncrement;

        public Ball(
            AsyncGraphics asyncGraphics, 
            int displayWidth, 
            int displayHeight, 
            Color backgroundColor, 
            Paddle paddle, 
            ISounds soundGenerator, 
            int minimumY,
            ScoreKeeper scoreKeeper)
        {
            this.asyncGraphics = asyncGraphics;
            this.soundGenerator = soundGenerator;

            this.scoreKeeper = scoreKeeper;

            this.backgroundColor = backgroundColor;
            this.paddle = paddle;

            this.displayWidth = displayWidth;
            this.maxX = displayWidth - this.width;
            this.maxY = displayHeight - this.height - Paddle.HEIGHT;
            this.minY = minimumY;
            this.yPosition = this.minY + 5;
 
            this.moveTimer.AutoReset = true;
            this.moveTimer.Elapsed += MoveTimer_Elapsed;
        }

        public void Reset()
        {
            this.xPosition = this.random.Next(5, this.displayWidth - 5 - this.width);
            this.yPosition = this.minY + 5;

            int xDirectionRandom = this.random.Next(0, 1);
            int directionMultiplier = xDirectionRandom > 0 ? -1 : 1;

            int incrementRandom = this.random.Next(6, 10);

            this.xIncrement = incrementRandom * directionMultiplier;
            this.yIncrement = 20 - incrementRandom;

            // draw the ball in the starting position
            this.Draw(this.xPosition, this.yPosition, this.color);
        }

        public void StartMoving()
        {
            this.moveTimer.Start();
        }

        public void StopMoving()
        {
            this.moveTimer.Stop();
        }

        private bool isMoveComplete = true;

        private void MoveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                MeadowApp.DebugWriteLine("Move timer elapsed");

                if (this.isMoveComplete)
                {
                    this.isMoveComplete = false;

                    if (!this.CheckForCollision())
                    {
                        int oldX = this.xPosition;
                        int oldY = this.yPosition;

                        MeadowApp.DebugWriteLine($"Move timer elapsed: {oldX},{oldY} {xIncrement},{yIncrement}");

                        this.xPosition += this.xIncrement;

                        if (this.xPosition > this.maxX)
                        {
                            this.xPosition = maxX;
                        }

                        MeadowApp.DebugWriteLine($"new x = {this.xPosition}");

                        if (this.xPosition < 0)
                        {
                            this.xPosition = 0;
                        }

                        this.yPosition += yIncrement;

                        MeadowApp.DebugWriteLine($"new y = {this.yPosition}");

                        if (this.yPosition > this.maxY)
                        {
                            this.yPosition = this.maxY;
                        }

                        if (this.yPosition < this.minY)
                        {
                            this.yPosition = this.minY;
                        }

                        MeadowApp.DebugWriteLine($"new x,y = {this.xPosition},{this.yPosition}");

                        if (!(this.xPosition == oldX && this.yPosition == oldY))
                        {
                            lock (this.asyncGraphics.LockObject)
                            {
                                if (this.moveTimer.Enabled)
                                {
                                    this.Draw(oldX, oldY, this.backgroundColor);
                                    this.Draw(this.xPosition, this.yPosition, this.color);
                                }
                            }
                        }
                    }

                    this.isMoveComplete = true;
                }

                MeadowApp.DebugWriteLine("leaving move timer elapsed");
            }
            catch (Exception ex)
            {
                MeadowApp.DebugWriteLine($"Exception in MoveTimer_Elapsed: {ex}");
                this.isMoveComplete = true;
            }
        }

        private void ChangeXIncrement(int extraX)
        {
            this.xIncrement += this.random.Next(-1, 1);
            this.xIncrement += extraX;

            if (this.xIncrement < 0)
            {
                if (this.xIncrement > -7)
                {
                    this.xIncrement = -7;
                }
            }
            else
            {
                if (this.xIncrement < 7)
                {
                    this.xIncrement = 7;
                }
            }
        }

        private void ChangeYIncrement(int extraY)
        {
            this.yIncrement += this.random.Next(-1, 1) + extraY;

            if (this.yIncrement < 0)
            {
                if (this.yIncrement > -7)
                {
                    this.yIncrement = -7;
                }
            }
            else
            {
                if (this.yIncrement < 7)
                {
                    this.yIncrement = 7;
                }
            }
        }

        private void GetVelocityChangeAdjustments(int ballCenterX, out int extraX, out int extraY)
        {
            float oneThirdPaddleWidth = this.paddle.Width / 3;

            if (xIncrement > 0)
            {
                // traveling right
                if (ballCenterX < this.paddle.Left + oneThirdPaddleWidth)
                {
                    extraX = -2;    // a bit less horizontal velocity
                    extraY = -2;    // a bit more vertical velocity
                }
                else if (ballCenterX > this.paddle.Left + oneThirdPaddleWidth * 2)
                {
                    extraX = 2;    // a bit more horizontal velocity
                    extraY = 2;    // a bit less vertical velocity
                }
                else
                {
                    extraX = 0;
                    extraY = 0;
                }
            }
            else
            {
                //traveling left
                if (ballCenterX < this.paddle.Left + oneThirdPaddleWidth)
                {
                    extraX = -2;   // a bit more horizontal velocity
                    extraY = 2;    // a bit less vertical velocity
                }
                else if (ballCenterX > this.paddle.Left + oneThirdPaddleWidth * 2)
                {
                    extraX = 2;    // a bit less horizontal velocity
                    extraY = -2;    // a bit more vertical velocity
                }
                else
                {
                    extraX = 0;
                    extraY = 0;
                }
            }
        }

        private bool CheckForCollision()
        {
            bool isPaddleMissed = false;
            bool isBorderHit = false;

            int ballCenterX = this.xPosition + this.width / 2;

            MeadowApp.DebugWriteLine($"checkForCollision: {ballCenterX},{this.yPosition}");

            if (this.yPosition >= this.maxY)
            {
                if (ballCenterX >= this.paddle.Left && ballCenterX < this.paddle.Right)
                {
                    // Paddle hit!
                    this.soundGenerator.PlayPaddleHitSound();
                    this.scoreKeeper.Increment();

                    this.GetVelocityChangeAdjustments(ballCenterX, out int extraX, out int extraY);

                    this.yIncrement = -this.yIncrement;

                    // Apply the x and y increments
                    this.ChangeXIncrement(extraX);
                    this.ChangeYIncrement(extraY);

                    // Make the paddle smaller
                    this.paddle.Shrink();
                }
                else
                {
                    // Missed the paddle.  Time to explode...
                    this.asyncGraphics.Stop();
                    this.StopMoving();
                    this.soundGenerator.PlayGameOverSound();
                    this.Explode();

                    isPaddleMissed = true;
                }
            }
            else
            {
                if (this.xPosition >= this.maxX || this.xPosition <= 0)
                {
                    MeadowApp.DebugWriteLine("x border hit");
                    isBorderHit = true;
                    this.xIncrement = -this.xIncrement;
                    this.ChangeYIncrement(0);
                }

                if (this.yPosition <= this.minY)
                {
                    MeadowApp.DebugWriteLine("y border hit");
                    isBorderHit = true;
                    this.yIncrement = -this.yIncrement;
                    this.ChangeXIncrement(0);
                }

                if (isBorderHit)
                {
                    this.soundGenerator.PlayBorderHitSound();
                }
            }

            MeadowApp.DebugWriteLine($"leaving checkForCollision: {isPaddleMissed}");
            return isPaddleMissed;
        }

        private void Explode()
        {
            int x = this.xPosition + this.width / 2;
            int y = this.yPosition - Paddle.HEIGHT;
            int radius = 4;
            int blackRadius = 0;

            for (int ii = 0; ii < 4; ++ii)
            {
                this.asyncGraphics.DrawCircle(x, y, radius, Color.Yellow);
                this.asyncGraphics.DrawCircle(x, y, (int)(radius * 0.7), Color.Orange);
                this.asyncGraphics.DrawCircle(x, y, (int)(radius * 0.4), Color.Red);

                blackRadius = (int)(radius * 0.2);
                this.asyncGraphics.DrawCircle(x, y, blackRadius, Color.Black);

                this.asyncGraphics.ShowDirect();
                Thread.Sleep(100);

                radius *= 2;
            }

            ExplosionOccurred?.Invoke(this, null);
        }

        private void Draw(int x, int y, Color color)
        {
            this.asyncGraphics.DrawCircle(
                x + width / 2,
                y + height / 2,
                this.width / 2,
                color);
        }
    }
}
