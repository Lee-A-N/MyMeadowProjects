namespace SoloPong
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;
    using Meadow.Foundation;
    using Meadow.Foundation.Graphics;

    public class Ball
    {
        private const int MOVE_INTERVAL = 200;

        private readonly Color backgroundColor;
        private readonly int maxX;
        private readonly int maxY;
        private readonly AsyncGraphics asyncGraphics;
        private readonly System.Timers.Timer moveTimer = new System.Timers.Timer(Ball.MOVE_INTERVAL);
        private readonly Paddle paddle;
        private readonly Random random = new Random();
        private readonly ISounds speaker;

        private int xPosition = 5;
        private int yPosition = 5;
        private int xIncrement;
        private int yIncrement;

        private int width = 10;
        private int height = 10;
        private int displayWidth;

        private Color color = Color.Red;

        public Ball(AsyncGraphics asyncGraphics, int displayWidth, int displayHeight, Color backgroundColor, Paddle paddle, ISounds soundGenerator)
        {
            this.asyncGraphics = asyncGraphics;
            this.speaker = soundGenerator;

            this.backgroundColor = backgroundColor;
            this.paddle = paddle;

            this.displayWidth = displayWidth;
            this.maxX = displayWidth - this.width;
            this.maxY = displayHeight - this.height - Paddle.HEIGHT;
 
            this.moveTimer.AutoReset = true;
            this.moveTimer.Elapsed += MoveTimer_Elapsed;
        }

        public void Reset()
        {
            this.xPosition = this.random.Next(5, this.displayWidth - 5 - this.width);
            this.yPosition = 5;

            this.xIncrement = 7;
            this.yIncrement = 13;

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

        private void MoveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.checkForCollision())
            {
                int oldX = this.xPosition;
                int oldY = this.yPosition;

                this.xPosition += xIncrement;

                if (this.xPosition > this.maxX)
                {
                    this.xPosition = maxX;
                }

                if (this.xPosition < 0)
                {
                    this.xPosition = 0;
                }

                this.yPosition += yIncrement;

                if (this.yPosition > this.maxY)
                {
                    this.yPosition = maxY;
                }

                if (this.yPosition < 0)
                {
                    this.yPosition = 0;
                }

                lock (this.asyncGraphics.LockObject)
                {
                    this.Draw(oldX, oldY, this.backgroundColor);
                    this.Draw(this.xPosition, this.yPosition, this.color);
                }
            }
        }

        private bool checkForCollision()
        {
            bool isCollisionDetected = false;

            if (this.xPosition >= this.maxX || this.xPosition <= 0)
            {
                this.speaker.PlayBorderHitSound();
                this.xIncrement = -this.xIncrement;
            }

            if (this.yPosition <= 0)
            {
                this.speaker.PlayBorderHitSound();
                this.yIncrement = -this.yIncrement;
            }

            if (this.yPosition >= this.maxY)
            {
                if (this.xPosition >= this.paddle.Left && this.xPosition < this.paddle.Right)
                {
                    this.speaker.PlayPaddleHitSound();
                    this.yIncrement = -this.yIncrement;
                    this.paddle.Shrink();
                }
                else
                {
                    // Missed the paddle.  Time to explode...
                    this.asyncGraphics.Stop();
                    this.StopMoving();
                    this.speaker.PlayGameOverSound();
                    this.Explode();

                    isCollisionDetected = true;
                }
            }

            return isCollisionDetected;
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
