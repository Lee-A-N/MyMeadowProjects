namespace SoloPong
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;

    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Audio;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;

    public class MeadowApp : App<F7Micro, MeadowApp>, ISounds
    {
        private const float SILENT = 0;
        private const float QUIET = 3;
        private const float LOUD = 20;

        private St7789 st7789;

        public GraphicsLibrary graphics;
        private AsyncGraphics asyncGraphics;

        private RotaryEncoderWithButton rotaryPaddle;
        private System.Timers.Timer paddleClickDebounceTimer = new System.Timers.Timer(500);
        private bool isPaddleClickDebounceActive = false;

        private PiezoSpeaker speaker;
        private IPwmPort speakerPWM;

        private int displayWidth;
        private int displayHeight;
        private Color backgroundColor;

        private System.Timers.Timer debounceTimer = new System.Timers.Timer(500);
        private bool isDebounceActive = false;

        int directionCounter = 0;

        private Paddle paddle;
        private Ball ball;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);

            this.speakerPWM = Device.CreatePwmPort(Device.Pins.D07, dutyCycle: QUIET);
            this.speaker = new PiezoSpeaker(speakerPWM);

            this.rotaryPaddle = new RotaryEncoderWithButton(Device, Device.Pins.D10, Device.Pins.D09, Device.Pins.D08);
            this.rotaryPaddle.Rotated += RotaryPaddle_Rotated;
            this.rotaryPaddle.Clicked += RotaryPaddle_Clicked;

            this.paddleClickDebounceTimer.AutoReset = false;
            this.paddleClickDebounceTimer.Elapsed += PaddleClickDebounceTimer_Elapsed;

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
            this.asyncGraphics.ShowDirect();

            this.paddle = new Paddle(this.asyncGraphics, this.displayWidth, this.displayWidth, this.backgroundColor);
            this.ball = new Ball(this.asyncGraphics, this.displayWidth, this.displayHeight, this.backgroundColor, this.paddle, this);
        }

        private void PaddleClickDebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.isPaddleClickDebounceActive = false;
        }

        private void RotaryPaddle_Clicked(object sender, EventArgs e)
        {
            if (!this.isPaddleClickDebounceActive)
            {
                this.isPaddleClickDebounceActive = true;
                this.asyncGraphics.Stop();
                this.ball.StopMoving();
                this.LoadScreen();
                this.paddle.Reset();
                this.ball.Reset();
                this.PlayStartSound();
                this.ball.StartMoving();
                this.asyncGraphics.Start();
            }
        }

        private void LoadScreen()
        {
            this.graphics.Stroke = Paddle.HEIGHT;

            this.graphics.DrawRectangle(
                xLeft: 0, 
                yTop: 0,
                width: this.displayWidth,
                height: this.displayHeight, 
                color: this.backgroundColor, 
                filled: true);
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

        void ISounds.PlayBorderHitSound()
        {
            new Thread(() =>
            {
                lock (this.speaker)
                {
                    this.speaker.PlayTone(1500, 30);
                }
            }).Start();
        }

        void ISounds.PlayPaddleHitSound()
        {
            new Thread(() =>
            {
                lock (this.speaker)
                {
                    this.speaker.PlayTone(750, 30);
                }
            }).Start();
        }

        void ISounds.PlayGameOverSound()
        {
            new Thread(() =>
            {
                lock (this.speaker)
                {
                    this.speaker.PlayTone(50, 500);
                }
            }).Start();
        }

        public void PlayStartSound()
        {
            new Thread(() =>
            {
                this.speaker.PlayTone(2000, 2);
                Thread.Sleep(300);
                this.speaker.PlayTone(2000, 2);
            }).Start();
        }

        public void PlayBootSound()
        {
            new Thread(() =>
            {
                this.speaker.PlayTone(4000, 2);
            }).Start();
        }
    }
}