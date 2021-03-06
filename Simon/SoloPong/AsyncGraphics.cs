﻿//-----------------------------------------------------------------------
// <copyright file="AsyncGraphics.cs" company="Logikos, Inc">
//     Copyright (c) Lee Nayes, Logikos, Inc. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SoloPong
{
    using System.Threading;
    using Meadow.Foundation;
    using Meadow.Foundation.Graphics;

    /// <summary>
    /// Facade class which wraps the Meadow's graphics library, adding a thread for 
    /// asynchronous screen refresh.
    /// </summary>
    public class AsyncGraphics
    {
        /// <summary>
        /// Object representing the meadow graphics library.
        /// </summary>
        private readonly GraphicsLibrary graphics;

        /// <summary>
        /// Thread used for asynchronous screen refresh.
        /// </summary>
        private readonly Thread showThread;

        /// <summary>
        /// Flag indicating whether the display should be refreshed in the thread's "show" loop.
        /// </summary>
        private bool updateDisplay = false;

        /// <summary>
        /// Initializes a new instance of the AsyncGraphics class.
        /// </summary>
        /// <param name="graphicsLibrary">Reference to a Meadow graphics library object</param>
        public AsyncGraphics(GraphicsLibrary graphicsLibrary)
        {
            this.graphics = graphicsLibrary;

            this.Clear();

            this.graphics.Stroke = Paddle.HEIGHT;
            this.graphics.CurrentFont = new Font12x16();

            this.showThread = new Thread(this.ShowLoop);
            this.showThread.Start();
        }

        /// <summary>
        /// Gets the lock object used for thread interaction associated with
        /// display updates.
        /// </summary>
        public object LockObject
        {
            get { return this.graphics; }
        }

        /// <summary>
        /// Update the screen without waiting for asynchronous screen refresh.
        /// </summary>
        public void ShowDirect()
        {
            lock (this.LockObject)
            {
                this.graphics.Show();
            }
        }

        /// <summary>
        /// Draw text on the screen
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="x">x position at which to draw the text</param>
        /// <param name="y">Y position at which to draw the text</param>
        /// <param name="color">Color to use</param>
        public void DrawText(string text, int x, int y, Color color)
        {
            this.graphics.DrawText(x, y, text, color);
        }

        /// <summary>
        /// Draw a line on the screen
        /// </summary>
        /// <param name="x0">Starting x coordinate</param>
        /// <param name="y0">Starting y coordinate</param>
        /// <param name="x1">Ending x coordinate</param>
        /// <param name="y1">Ending y coordinate</param>
        /// <param name="color">Color to use</param>
        public void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            this.graphics.DrawLine(x0, y0, x1, y1, color);
        }

        /// <summary>
        /// Draw a filled rectangle on the screen
        /// </summary>
        /// <param name="left">Left x coordinate</param>
        /// <param name="top">Top y coordinate</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="color">Color to use</param>
        public void DrawRectangle(int left, int top, int width, int height, Color color)
        {
            this.graphics.DrawRectangle(left, top, width, height, color, filled: true);
        }

        /// <summary>
        /// Draw a filled circle on the screen
        /// </summary>
        /// <param name="x">Upper left x coordinate</param>
        /// <param name="y">Upper left y coordinate</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="color">Color to use</param>
        public void DrawCircle(int x, int y, int radius, Color color)
        {
            this.graphics.DrawCircle(x, y, radius, color, filled: true);
        }

        /// <summary>
        /// Start screen refresh
        /// </summary>
        public void Start()
        {
            this.updateDisplay = true;
        }

        /// <summary>
        /// Stop screen refresh
        /// </summary>
        public void Stop()
        {
            this.updateDisplay = false;
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void Clear()
        {
            this.graphics.Clear(updateDisplay: true);
        }

        /// <summary>
        /// Thread procedure used for asynchronous screen refresh.
        /// </summary>
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
