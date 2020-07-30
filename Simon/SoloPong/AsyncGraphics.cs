namespace SoloPong
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;

    public class AsyncGraphics
    {
        private GraphicsLibrary graphics;
        private Thread showThread;
        private bool keepRunning = true;

        public AsyncGraphics(GraphicsLibrary graphicsLibrary)
        {
            this.graphics = graphicsLibrary;
            this.showThread = new Thread(this.ShowLoop);
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

        public void Start()
        {
            this.showThread.Start();
        }

        public void Stop()
        {
            this.keepRunning = false;
            this.showThread.Join();
        }

        private void ShowLoop()
        {
            while (this.keepRunning)
            {
                lock (this.graphics)
                {
                    if (keepRunning)
                    {
                        this.graphics.Show();
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
