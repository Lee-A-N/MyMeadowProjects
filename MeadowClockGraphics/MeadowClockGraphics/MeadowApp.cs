using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace MeadowClockGraphics
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        readonly Color WatchBackgroundColor = Color.White;

        St7789 st7789;
        GraphicsLibrary graphics;
        int displayWidth, displayHeight;
        int hour, minute, second, tick;

        public MeadowApp()
        {
            var config = new SpiClockConfiguration(6000,
                SpiClockConfiguration.Mode.Mode3);
            st7789 = new St7789
            (
                device: Device,
                spiBus: Device.CreateSpiBus(
                    Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            this.displayWidth = Convert.ToInt32(this.st7789.Width);
            this.displayHeight = Convert.ToInt32(this.st7789.Height);

            graphics = new GraphicsLibrary(st7789);
            graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;

            DrawClock();
        }

        void DrawClock()
        {
            graphics.Clear(true);

            hour = 8;
            minute = 54;
            DrawWatchFace();

            while (true)
            {
                tick++;
                Thread.Sleep(100);
                UpdateClock(second: tick % 60);
            }
        }

        void DrawWatchFace()
        {
            graphics.Clear();
            int hour = 12;
            int xCenter = displayWidth / 2;
            int yCenter = displayHeight / 2;
            int x, y;
            graphics.DrawRectangle(0, 0, displayWidth, displayHeight, Color.White);
            graphics.DrawRectangle(5, 5, displayWidth - 10, displayHeight - 10, Color.White);
            graphics.CurrentFont = new Font12x20();
            graphics.DrawCircle(xCenter, yCenter, 100, WatchBackgroundColor, true);

            for (int i = 0; i < 60; i++)
            {
                x = (int)(xCenter + 80 * Math.Sin(i * Math.PI / 30));
                y = (int)(yCenter - 80 * Math.Cos(i * Math.PI / 30));

                if (i % 5 == 0)
                {
                    graphics.DrawText(
                        hour > 9 ? x - 10 : x - 5, y - 5, hour.ToString(), Color.Black);

                    if (hour == 12) hour = 1; else hour++;
                }
            }
            graphics.Show();
        }

        void UpdateClock(int second = 0)
        {
            if (second == 0)
            {
                minute++;

                if (minute == 60)
                {
                    minute = 0;
                    hour++;

                    if (hour == 12)
                    {
                        hour = 0;
                    }
                }
            }

            //remove previous hour
            int previousHour = (hour - 1) < -1 ? 11 : (hour - 1);
            this.DrawHourOrMinuteHand(previousHour, 43, 3, 6, WatchBackgroundColor);

            //current hour
            this.DrawHourOrMinuteHand(hour, 43, 3, 6, Color.Black);

            //remove previous minute
            int previousMinute = minute - 1 < -1 ? 59 : (minute - 1);
            this.DrawHourOrMinuteHand(previousMinute, 58, 15, 30, this.WatchBackgroundColor);

            //current minute
            this.DrawHourOrMinuteHand(minute, 58, 15, 30, Color.Black);

            // remove previous second
            int previousSecond = second - 1 < -1 ? 59 : (second - 1);
            this.DrawSecondHand(previousSecond, this.WatchBackgroundColor);

            // draw current second
            this.DrawSecondHand(second, Color.Crimson);

            graphics.Show();
        }

        private int XCenter
        {
            get { return this.displayWidth / 2; }
        }

        private int YCenter
        {
            get { return this.displayHeight / 2; }
        }

        private void DrawSecondHand(int second, Color color)
        {
            this.graphics.Stroke = 1;
            int x = (int)(XCenter + 70 * System.Math.Sin(second * System.Math.PI / 30));
            int y = (int)(YCenter - 70 * System.Math.Cos(second * System.Math.PI / 30));
            graphics.DrawLine(XCenter, YCenter, x, y, color);
        }

        private void DrawHourOrMinuteHand(int value, int length, int offset, int xyDivisor, Color color)
        {
            this.graphics.Stroke = 3;
            int x = (int)(XCenter + length * System.Math.Sin(value * System.Math.PI / xyDivisor));
            int y = (int)(YCenter - length * System.Math.Cos(value * System.Math.PI / xyDivisor));

            graphics.DrawLine(XCenter, YCenter, x, y, color);
        }
    }

    public class MeadowAppText : App<F7Micro, MeadowAppText>
    {
        St7789 st7789;
        GraphicsLibrary graphics;

        public MeadowAppText()
        {
            var config = new SpiClockConfiguration(6000,
                SpiClockConfiguration.Mode.Mode3);
            st7789 = new St7789
            (
                device: Device,
                spiBus: Device.CreateSpiBus(
                    Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            graphics = new GraphicsLibrary(st7789);
            graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;

            DrawTexts();
        }

        void DrawTexts()
        {
            graphics.Clear(true);

            int indent = 20;
            int spacing = 20;
            int y = 5;

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(indent, y, "Meadow F7 SPI ST7789!!", Color.White);

            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(indent, y += spacing, "Red", Color.Red);
            graphics.DrawText(indent, y += spacing, "Purple", Color.Purple);
            graphics.DrawText(indent, y += spacing, "BlueViolet", Color.BlueViolet);
            graphics.DrawText(indent, y += spacing, "Blue", Color.Blue);
            graphics.DrawText(indent, y += spacing, "Cyan", Color.Cyan);
            graphics.DrawText(indent, y += spacing, "LawnGreen", Color.LawnGreen);
            graphics.DrawText(indent, y += spacing, "GreenYellow", Color.GreenYellow);
            graphics.DrawText(indent, y += spacing, "Yellow", Color.Yellow);
            graphics.DrawText(indent, y += spacing, "Orange", Color.Orange);
            graphics.DrawText(indent, y += spacing, "Brown", Color.Brown);
            graphics.Show();

            Thread.Sleep(5000);
        }
    }

    public class MeadowAppShapes : App<F7Micro, MeadowAppShapes>
    {
        private St7789 st7789;
        private GraphicsLibrary graphics;

        private int displayWidth;

        private int displayHeight;

        public MeadowAppShapes()
        {
            var config = new SpiClockConfiguration(6000,
                SpiClockConfiguration.Mode.Mode3);
            st7789 = new St7789
            (
                device: Device,
                spiBus: Device.CreateSpiBus(
                    Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config),
                chipSelectPin: null,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            this.displayWidth = Convert.ToInt32(this.st7789.Width);
            this.displayHeight = Convert.ToInt32(this.st7789.Height);

            this.graphics = new GraphicsLibrary(st7789);
            this.graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;

            this.DrawShapes();
        }

        void DrawShapes()
        {
            while (true)
            {
                Random rand = new Random();

                graphics.Clear(true);

                int radius = 100;
                int originX = this.displayWidth / 2;
                int originY = this.displayHeight / 2;

                for (int i = 1; i < 5; i++)
                {
                    graphics.DrawCircle
                    (
                        centerX: originX,
                        centerY: originY,
                        radius: radius,
                        color: Color.FromRgb(
                            rand.Next(255), rand.Next(255), rand.Next(255)),
                        filled: true
                    );

                    graphics.Show();
                    radius -= 30;
                }

                int sideLength = 30;

                for (int i = 1; i < 5; i++)
                {
                    graphics.Stroke = 4;

                    graphics.DrawRectangle
                    (
                        xLeft: (displayWidth - sideLength) / 2,
                        yTop: (displayHeight - sideLength) / 2,
                        width: sideLength,
                        height: sideLength,
                        color: Color.FromRgb(
                            rand.Next(255), rand.Next(255), rand.Next(255))
                    );

                    graphics.Show();
                    sideLength += 60;
                }

                // horizontal
                graphics.DrawLine(0, displayHeight / 2, displayWidth, displayHeight / 2,
                    Color.FromRgb(rand.Next(255), rand.Next(255), rand.Next(255)));

                graphics.Show();

                //vertical
                graphics.DrawLine(displayWidth / 2, 0, displayWidth / 2, displayHeight,
                    Color.FromRgb(rand.Next(255), rand.Next(255), rand.Next(255)));

                graphics.Show();

                // diagonal lt_rb
                graphics.DrawLine(0, 0, displayWidth, displayHeight,
                    Color.FromRgb(rand.Next(255), rand.Next(255), rand.Next(255)));

                graphics.Show();

                // diagonal lb_rt
                graphics.DrawLine(0, displayHeight, displayWidth, 0,
                    Color.FromRgb(rand.Next(255), rand.Next(255), rand.Next(255)));

                graphics.Show();

                Thread.Sleep(5000);
            }
        }
    }
}