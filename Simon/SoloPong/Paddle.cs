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

        private MeadowApp parent;

        private int width;
        private int top;
        private int position;
        private Color paddleColor = Color.White;

        private bool isMoveInProgress = false;

        public Paddle(MeadowApp parent)
        {
            this.parent = parent;
            this.width = parent.DisplayWidth / 3;
            this.top = parent.DisplayHeight - 5;
            this.position = parent.DisplayWidth / 3;

            // draw the paddle in the starting position
            this.Draw(this.position, this.position + width, this.paddleColor);
            this.Graphics.Show();
        }

        private GraphicsLibrary Graphics 
        { 
            get { return this.parent.Graphics; } 
        }

        private Color BackgroundColor
        {
            get { return this.parent.Background;  }
        }

        private int MaxRight
        {
            get { return this.parent.DisplayWidth - this.width;  }
        }

        private object lockObject = new object();

        public void Move(int increment)
        {
            if (!isMoveInProgress)
            {
                this.isMoveInProgress = true;

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
                        //for (int ii = 0; ii < 3; ++ii)
                        //{
                        //    this.Draw(this.position + ii * drawDelta, this.position + (ii + 1) * drawDelta, this.BackgroundColor);
                        //    this.Draw(right + ii * drawDelta, right + (ii + 1) * drawDelta, this.paddleColor);
                        //    this.Graphics.Show();
                        //}

                        this.Draw(oldPosition, oldPosition + delta, this.BackgroundColor);
                        this.Draw(right, right + delta, this.paddleColor);

                        Task.Run(() => this.Graphics.Show());
                    }
                    else if (delta < 0)
                    {
                        this.Draw(right, right + delta, this.BackgroundColor);
                        this.Draw(oldPosition, newPosition, this.paddleColor);
                        Task.Run(() => this.Graphics.Show());

                        //for (int ii = 0; ii < 3; ++ii)
                        //{
                        //    this.Draw(right + ii * drawDelta, right + (ii + 1) * drawDelta, this.BackgroundColor);
                        //    this.Draw(this.position + ii * drawDelta, this.position + (ii + 1) * drawDelta, this.paddleColor);
                        //    this.Graphics.Show();
                        //}
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
            this.Graphics.DrawLine(left, this.top, right, this.top, color);
        }

        public void MoveRight()
        {
            //Console.WriteLine("Move right");
            //Task.Run(() => this.Move(Paddle.INCREMENT));
            this.Move(Paddle.INCREMENT);
        }

        public void MoveLeft()
        {
            //Console.WriteLine("Move left");
            //Task.Run(() => this.Move(-Paddle.INCREMENT));
            this.Move(-Paddle.INCREMENT);
        }
    }
}
