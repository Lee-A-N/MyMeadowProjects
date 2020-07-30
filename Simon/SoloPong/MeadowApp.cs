namespace SoloPong
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private St7789 st7789;

        public GraphicsLibrary graphics;
        private AsyncGraphics asyncGraphics;

        private Paddle paddle;

        private RotaryEncoderWithButton rotaryPaddle;

        private int displayWidth;
        private int displayHeight;

        private System.Timers.Timer debounceTimer = new System.Timers.Timer(500);
        private bool isDebounceActive = false;
        public Color backgroundColor;

        int directionCounter = 0;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);

            this.st7789 = new St7789(
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

            this.graphics = new GraphicsLibrary(this.st7789);
            this.graphics.Clear();
            this.graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;
            this.graphics.Show();
            this.asyncGraphics = new AsyncGraphics(this.graphics);

            this.debounceTimer.AutoReset = false;
            this.debounceTimer.Elapsed += DebounceTimer_Elapsed;

            this.backgroundColor = Color.Blue;

            this.LoadScreen();

            this.paddle = new Paddle(this.asyncGraphics, this.displayWidth, this.displayWidth, this.backgroundColor);

            this.rotaryPaddle = new RotaryEncoderWithButton(Device, Device.Pins.D10, Device.Pins.D09, Device.Pins.D08);
            this.rotaryPaddle.Rotated += RotaryPaddle_Rotated;

            this.asyncGraphics.Start();
        }

        private void LoadScreen()
        {
            this.graphics.Stroke = 5;
            this.graphics.DrawRectangle(
                xLeft: 0, 
                yTop: 0,
                width: this.displayWidth,
                height: this.displayHeight, 
                color: this.backgroundColor, 
                filled: true);
            this.graphics.Show();
        }

        private void RotaryPaddle_Rotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryTurnedEventArgs e)
        {
            try
            {
                if (!this.isDebounceActive)
                {
                    this.isDebounceActive = true;
                    this.debounceTimer.Start();

                    if (e.Direction == RotationDirection.Clockwise)
                    {
                        ++directionCounter;

                        if (this.directionCounter > 0)
                        {
                            this.paddle.MoveLeft();
                            this.directionCounter = 1;
                        }
                    }
                    else
                    {
                        --directionCounter;

                        if (this.directionCounter < 0)
                        {
                            this.paddle.MoveRight();
                            this.directionCounter = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RotaryPaddle_Rotated exception: {ex}");
            }
        }

        private void DebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.isDebounceActive = false;
        }
    }
}