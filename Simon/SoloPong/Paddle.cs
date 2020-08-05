namespace SoloPong
{
    using Meadow.Foundation;

    public class Paddle
    {
        public const int HEIGHT = 3;
        private const int INCREMENT = 30;
        private const int SHRINK_AMOUNT = 3;

        private readonly int displayWidth;
        private readonly Color paddleColor = Color.White;
        private readonly Color backgroundColor;
        private readonly AsyncGraphics asyncGraphics; 
        private readonly int y;

        private int position;

        public Paddle(AsyncGraphics asyncGraphics, int displayWidth, int displayHeight, Color backgroundColor)
        {
            this.asyncGraphics = asyncGraphics;

            this.displayWidth = displayWidth;
            this.backgroundColor = backgroundColor;
            this.y = displayHeight - Paddle.HEIGHT / 2;
        }

        public int Width { get; set; }

        public void Reset()
        {
            this.position = this.displayWidth / 3;
            this.Width = displayWidth / 3;

            // draw the paddle in the starting position
            this.Draw(this.position, this.position + Width, this.paddleColor);
        }

        public int Left
        {
            get { return this.position; }
        }

        public int Right
        {
            get { return this.position + this.Width; }
        }

        public int MaxRight
        {
            get { return this.displayWidth - this.Width; }
        }

        public void Move(int increment)
        {
            // calculate the new position
            int newPosition = this.position + increment;

            if (newPosition > this.MaxRight)
            {
                newPosition = this.MaxRight;
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
                int right = this.position + this.Width;
                int delta = newPosition - this.position;

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
                        this.Draw(oldPosition, newPosition, this.paddleColor);
                        this.Draw(right, right + delta, this.backgroundColor);
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
            int x2 = left + this.Width;
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

        public void Shrink()
        {
            if (this.Width > this.displayWidth / 5)
            {
                int oldRight = this.Right;
                this.Width -= Paddle.SHRINK_AMOUNT;
                this.Draw(oldRight, this.Right, this.backgroundColor);
            }
        }
    }
}
