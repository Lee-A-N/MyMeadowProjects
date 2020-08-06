namespace SoloPong
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Timers;
    using System.Xml;
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Audio;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private const int KNOB_ROTATION_DEBOUNCE_INTERVAL = 100;

        private readonly St7789 st7789;

        private readonly GraphicsLibrary graphics;
        private readonly AsyncGraphics asyncGraphics;

        private readonly RotaryEncoderWithButton rotaryPaddle;

        private readonly ISounds soundGenerator;

        private readonly int displayWidth;
        private readonly int displayHeight;
        private readonly Color backgroundColor;

        private readonly System.Timers.Timer debounceTimer = new System.Timers.Timer(MeadowApp.KNOB_ROTATION_DEBOUNCE_INTERVAL);

        private readonly ScoreKeeper scoreKeeper;

        private readonly Paddle paddle;
        private readonly Ball ball;
        private readonly Banner instructionBanner;
        private readonly Banner scoreBanner;

        private bool isDebounceActive = false;

        int directionCounter = 0;
        private int rotaryPaddleClickCount = 0;

        public MeadowApp()
        {
            MeadowApp.DebugWriteLine("Initializing...");

            this.soundGenerator = new SoundGenerator(
                Device.CreateDigitalInputPort(Device.Pins.D03),
                Device.CreateDigitalInputPort(Device.Pins.D04),
                Device.CreatePwmPort(Device.Pins.D07));

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);

            this.rotaryPaddle = new RotaryEncoderWithButton(Device, Device.Pins.D10, Device.Pins.D09, Device.Pins.D08, debounceDuration: 100);
            this.rotaryPaddle.Rotated += RotaryPaddle_Rotated;

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

            this.graphics = new GraphicsLibrary(this.st7789)
            {
                Rotation = GraphicsLibrary.RotationType._270Degrees
            };
            this.graphics.Clear(updateDisplay: true);
            this.asyncGraphics = new AsyncGraphics(this.graphics);

            this.debounceTimer.AutoReset = false;
            this.debounceTimer.Elapsed += DebounceTimer_Elapsed;

            this.backgroundColor = Color.Blue;

            this.scoreBanner = new Banner(this.displayWidth, this.asyncGraphics, fontHeight: 16, this.backgroundColor, color: Color.Yellow, top: 0)
            {
                Text = Banner.SCORE_TEXT
            };

            this.instructionBanner = new Banner(
                displayWidth: this.displayWidth, 
                graphics: this.asyncGraphics, 
                fontHeight: 16, 
                backgroundColor: this.backgroundColor, 
                color: Color.White, 
                top: Banner.HEIGHT * 2);
            this.ShowInstructionBanner(Banner.START_TEXT);

            this.paddle = new Paddle(this.asyncGraphics, this.displayWidth, this.displayWidth, this.backgroundColor);

            this.scoreKeeper = new ScoreKeeper();
            this.scoreKeeper.ScoreChanged += this.scoreBanner.OnScoreChanged;

            this.ball = new Ball(
                asyncGraphics: this.asyncGraphics, 
                displayWidth: this.displayWidth, 
                displayHeight: this.displayHeight, 
                backgroundColor: this.backgroundColor, 
                paddle: this.paddle, 
                soundGenerator: this.soundGenerator, 
                minimumY: Banner.HEIGHT + 1,
                scoreKeeper: this.scoreKeeper);

            this.ball.ExplosionOccurred += this.OnExplosionOccurred;

            this.rotaryPaddle.Clicked += RotaryPaddle_Clicked;

            this.soundGenerator.PlayConstructionCompleteSound();
        }

        public static void DebugWriteLine(string s)
        {
            //Console.WriteLine(s);
        }

        private void RotaryPaddle_Clicked(object sender, EventArgs e)
        {
            ++this.rotaryPaddleClickCount;

            if (this.rotaryPaddleClickCount <= 1)
            {
                soundGenerator.PlayStartSound();
                MeadowApp.DebugWriteLine("Processing knob click");
                this.graphics.Clear(true);
                this.asyncGraphics.Stop();
                this.ball.StopMoving();
                this.LoadScreen(eraseInstructionBanner : true, string.Empty, showScoreBanner: true);
                this.paddle.Reset();
                this.ball.Reset();
                this.scoreKeeper.Reset();
                this.soundGenerator.PlayStartSound();
                this.ball.StartMoving();
                this.asyncGraphics.Start();
            }

            this.rotaryPaddleClickCount = 0;
        }

        public void ShowInstructionBanner(string text)
        {
            this.instructionBanner.Text = text;
            this.instructionBanner.Draw();
            this.asyncGraphics.ShowDirect();
        }

        private void LoadScreen(bool eraseInstructionBanner, string instructionBannerText, bool showScoreBanner)
        {
            this.graphics.DrawRectangle(
                xLeft: 0, 
                yTop: 0,
                width: this.displayWidth,
                height: this.displayHeight, 
                color: this.backgroundColor, 
                filled: true);

            if (eraseInstructionBanner)
            {
                this.instructionBanner.Hide();
            }
            else
            {
                this.ShowInstructionBanner(instructionBannerText);
            }

            if (showScoreBanner)
            {
                this.scoreBanner.Draw();
            }

            this.asyncGraphics.ShowDirect();
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
                MeadowApp.DebugWriteLine($"RotaryPaddle_Rotated exception: {ex}");
            }
        }

        private void DebounceTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.isDebounceActive = false;
        }

        private void OnExplosionOccurred(object sender, Ball.GameOverArgs args)
        {
            this.ShowInstructionBanner(Banner.RESTART_TEXT);
        }
    }
}