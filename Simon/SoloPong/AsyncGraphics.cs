namespace SoloPong
{
    using System.Threading;
    using Meadow.Foundation;
    using Meadow.Foundation.Graphics;

    public class AsyncGraphics
    {
        private GraphicsLibrary graphics;
        private Thread showThread;
        private bool updateDisplay = false;

        public AsyncGraphics(GraphicsLibrary graphicsLibrary)
        {
            this.graphics = graphicsLibrary;

            this.showThread = new Thread(this.ShowLoop);
            this.showThread.Start();
        }

        public object LockObject
        {
            get { return this.graphics; }
        }

        public void ShowDirect()
        {
            this.graphics.Show();
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            lock (graphics)
            {
                this.graphics.DrawLine(x0, y0, x1, y1, color);
            }
        }

        public void DrawRectangle(int x0, int y0, int width, int height, Color color)
        {
            lock (graphics)
            {
                this.graphics.DrawRectangle(x0, y0, width, height, color, true);
            }
        }

        public void DrawCircle(int x, int y, int radius, Color color)
        {
            lock (graphics)
            {
                this.graphics.DrawCircle(x, y, radius, color, true);
            }
        }

        public void Start()
        {
            this.updateDisplay = true;
        }

        public void Stop()
        {
            this.updateDisplay = false;
        }

        private void ShowLoop()
        {
            while (true)
            {
                if (this.updateDisplay)
                {
                    lock (this.graphics)
                    {
                        this.graphics.Show();
                    }
                }

                Thread.Sleep(50);
            }
        }
    }
}
