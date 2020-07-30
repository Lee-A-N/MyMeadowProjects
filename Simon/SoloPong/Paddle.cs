namespace SoloPong
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Meadow.Foundation;
    using Meadow.Foundation.Graphics;

    public class Paddle
    {
        private const int INCREMENT = 30;

        private int width;
        //private int displayWidth;
        //private int displayHeight;
        private int top;
        private int position;
        private Color paddleColor = Color.White;
        private Color backgroundColor;
        private int maxRight;

        private bool isMoveInProgress = false;

        private AsyncGraphics asyncGraphics; 

        public Paddle(AsyncGraphics asyncGraphics, int displayWidth, int displayHeight, Color backgroundColor)
        {
            this.asyncGraphics = asyncGraphics;

            this.width = displayWidth / 3;
            this.top = displayHeight - 5;
            this.position = displayWidth / 3;
            this.backgroundColor = backgroundColor;
            this.maxRight = displayWidth - this.width;

            // draw the paddle in the starting position
            this.Draw(this.position, this.position + width, this.paddleColor);
            this.asyncGraphics.ShowDirect();
        }


        private object lockObject = new object();

        public void Move(int increment)
        {
            if (!isMoveInProgress)
            {
                this.isMoveInProgress = true;

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

                //Console.WriteLine($"New position is {this.position}");

                if (this.position != newPosition)
                {
                    int right = this.position + this.width;
                    int delta = newPosition - this.position;
                    int drawDelta = delta / 3;

                    int oldPosition = this.position;
                    this.position = newPosition;

                    if (delta > 0)
                    {
                        this.Draw(oldPosition, oldPosition + delta, this.backgroundColor);
                        this.Draw(right, right + delta, this.paddleColor);
                        //Task.Run(() => this.Graphics.Show());
                    }
                    else if (delta < 0)
                    {
                        this.Draw(right, right + delta, this.backgroundColor);
                        this.Draw(oldPosition, newPosition, this.paddleColor);
                        //Task.Run(() => this.Graphics.Show());
                    }
                    else
                    {
                        // do nothing
                    }
                }
            }

            this.isMoveInProgress = false;
        }

        private void Draw(int left, int right, Color color)
        {
            int x2 = left + this.width;
            this.asyncGraphics.DrawLine(left, this.top, right, this.top, color);
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
