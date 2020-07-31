namespace SoloPong
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using Meadow.Foundation;
    using Meadow.Foundation.Graphics;

    public class Paddle
    {
        public const int HEIGHT = 3;
        private const int INCREMENT = 30;

        private readonly int displayWidth;
        private readonly int width;
        private readonly int maxRight;
        private readonly Color paddleColor = Color.White;
        private readonly Color backgroundColor;
        private readonly AsyncGraphics asyncGraphics; 
        private readonly int y;

        private int position;

        public Paddle(AsyncGraphics asyncGraphics, int displayWidth, int displayHeight, Color backgroundColor)
        {
            this.asyncGraphics = asyncGraphics;

            this.displayWidth = displayWidth;
            this.width = displayWidth / 3;
            this.backgroundColor = backgroundColor;
            this.maxRight = displayWidth - this.width;
            this.y = displayHeight - Paddle.HEIGHT / 2;
        }

        public void Reset()
        {
            this.position = this.displayWidth / 3;

            // draw the paddle in the starting position
            this.Draw(this.position, this.position + width, this.paddleColor);
        }

        public int Left
        {
            get { return this.position; }
        }

        public int Right
        {
            get { return this.position + this.width; }
        }

        public void Move(int increment)
        {
            // calculate the new position
            int newPosition = this.position + increment;

            if (newPosition > this.maxRight)
            {
                newPosition = this.maxRight;
            }
            else if (newPosition < 0)
            {
                newPosition = 0;
            }
            else
            {
                // do nothing
            }

            if (this.position != newPosition)
            {
                int right = this.position + this.width;
                int delta = newPosition - this.position;
                int drawDelta = delta / 3;

                int oldPosition = this.position;
                this.position = newPosition;

                if (delta > 0)
                {
                    lock (this.asyncGraphics.LockObject)
                    {
                        this.Draw(oldPosition, oldPosition + delta, this.backgroundColor);
                        this.Draw(right, right + delta, this.paddleColor);
                    }
                }
                else if (delta < 0)
                {
                    lock (this.asyncGraphics.LockObject)
                    {
                        this.Draw(right, right + delta, this.backgroundColor);
                        this.Draw(oldPosition, newPosition, this.paddleColor);
                    }
                }
                else
                {
                    // do nothing
                }
            }
        }

        private void Draw(int left, int right, Color color)
        {
            int x2 = left + this.width;
            this.asyncGraphics.DrawLine(left, this.y, right, this.y, color);
        }

        public void MoveRight()
        {
            this.Move(Paddle.INCREMENT);
        }

        public void MoveLeft()
        {
            this.Move(-Paddle.INCREMENT);
        }
    }
}
