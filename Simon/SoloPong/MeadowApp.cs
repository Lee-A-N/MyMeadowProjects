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

        public GraphicsLibrary Graphics { get; set; }

        private Paddle paddle;

        private RotaryEncoderWithButton rotaryPaddle;

        private System.Timers.Timer debounceTimer = new System.Timers.Timer(500);
        private bool isDebounceActive = false;

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
            this.DisplayWidth = Convert.ToInt32(this.st7789.Width);
            this.DisplayHeight = Convert.ToInt32(this.st7789.Height);

            this.Graphics = new GraphicsLibrary(this.st7789);
            this.Graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;

            this.Graphics.Clear();
            this.Graphics.Show();

            this.debounceTimer.AutoReset = false;
            this.debounceTimer.Elapsed += DebounceTimer_Elapsed;

            this.Background = Color.Blue;

            this.LoadScreen();

            this.paddle = new Paddle(this);

            this.rotaryPaddle = new RotaryEncoderWithButton(Device, Device.Pins.D10, Device.Pins.D09, Device.Pins.D08);
            this.rotaryPaddle.Rotated += RotaryPaddle_Rotated;
        }

        public Color Background { get; set; }

        public int DisplayWidth { get; set; }

        public int DisplayHeight { get; set; }

        private void LoadScreen()
        {

            this.Graphics.Stroke = 5;

            this.Graphics.DrawRectangle(xLeft: 0, yTop: 0, width: this.DisplayWidth, height: this.DisplayHeight, color: this.Background, filled: true);

            this.Graphics.Show();
        }

        int directionCounter = 0;

        private void RotaryPaddle_Rotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryTurnedEventArgs e)
        {
            try
            {
                if (!this.isDebounceActive)
                {
                    this.isDebounceActive = true;
                    this.debounceTimer.Start();

                    //if (!this.isDebounceActive)
                    //{
                    //    this.isDebounceActive = true;

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

                    //this.isDebounceActive = false;
                    //}
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