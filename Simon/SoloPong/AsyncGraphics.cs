﻿namespace SoloPong
{
    using System.Threading;
    using Meadow.Foundation;
    using Meadow.Foundation.Graphics;

    public class AsyncGraphics
    {
        private readonly GraphicsLibrary graphics;
        private readonly Thread showThread;
        private bool updateDisplay = false;

        public AsyncGraphics(GraphicsLibrary graphicsLibrary)
        {
            this.graphics = graphicsLibrary;

            this.Clear();

            this.graphics.Stroke = Paddle.HEIGHT;
            this.graphics.CurrentFont = new Font12x16();

            this.showThread = new Thread(this.ShowLoop);
            this.showThread.Start();
        }

        public object LockObject
        {
            get { return this.graphics; }
        }

        public void ShowDirect()
        {
            lock (this.LockObject)
            {
                this.graphics.Show();
            }
        }

        public void DrawText(string text, int x, int y, Color color)
        {
            this.graphics.DrawText(x, y, text, color);
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            this.graphics.DrawLine(x0, y0, x1, y1, color);
        }

        public void DrawRectangle(int left, int top, int width, int height, Color color)
        {
            this.graphics.DrawRectangle(left, top, width, height, color, filled: true);
        }

        public void DrawCircle(int x, int y, int radius, Color color)
        {
            this.graphics.DrawCircle(x, y, radius, color, true);
        }

        public void Start()
        {
            this.updateDisplay = true;
        }

        public void Stop()
        {
            this.updateDisplay = false;
        }

        public void Clear()
        {
            this.graphics.Clear(updateDisplay: true);
        }

        private void ShowLoop()
        {
            while (true)
            {
                lock (this.LockObject)
                {
                    if (this.updateDisplay)
                    {
                        this.graphics.Show();
                    }
                }

                Thread.Sleep(93);
            }
        }
    }
}
