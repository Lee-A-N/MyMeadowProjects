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

    public class MeadowApp : App<F7Micro, MeadowApp>, ISounds
    {
        private enum SoundMode
        {
            Silent,
            Soft,
            Normal
        }

        private const int KNOB_ROTATION_DEBOUNCE_INTERVAL = 100;

        private readonly St7789 st7789;

        private readonly GraphicsLibrary graphics;
        private readonly AsyncGraphics asyncGraphics;

        private readonly RotaryEncoderWithButton rotaryPaddle;

        private readonly PiezoSpeaker speaker;
        private readonly IPwmPort speakerPWM;

        private readonly IDigitalInputPort volumeIn1;
        private readonly IDigitalInputPort volumeIn2;

        private readonly int displayWidth;
        private readonly int displayHeight;
        private readonly Color backgroundColor;

        private readonly System.Timers.Timer debounceTimer = new System.Timers.Timer(MeadowApp.KNOB_ROTATION_DEBOUNCE_INTERVAL);
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

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);

            this.rotaryPaddle = new RotaryEncoderWithButton(Device, Device.Pins.D10, Device.Pins.D09, Device.Pins.D08, debounceDuration: 100);
            this.rotaryPaddle.Rotated += RotaryPaddle_Rotated;

            this.volumeIn1 = Device.CreateDigitalInputPort(Device.Pins.D03);
            this.volumeIn2 = Device.CreateDigitalInputPort(Device.Pins.D04);

            this.speakerPWM = Device.CreatePwmPort(Device.Pins.D07, frequency: 5, dutyCycle: this.SoundDutyCycle);
            this.speaker = new PiezoSpeaker(speakerPWM);
            this.PlayInitialSound();

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
            this.graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;
            this.graphics.Clear(updateDisplay: true);
            this.asyncGraphics = new AsyncGraphics(this.graphics);

            this.debounceTimer.AutoReset = false;
            this.debounceTimer.Elapsed += DebounceTimer_Elapsed;

            this.backgroundColor = Color.Blue;

            this.scoreBanner = new Banner(this.displayWidth, this.asyncGraphics, fontHeight: 16, this.backgroundColor, color: Color.Yellow, top: 0);
            this.scoreBanner.Text = Banner.SCORE_TEXT;
            this.instructionBanner = new Banner(
                displayWidth: this.displayWidth, 
                graphics: this.asyncGraphics, 
                fontHeight: 16, 
                backgroundColor: this.backgroundColor, 
                color: Color.White, 
                top: Banner.HEIGHT * 2);
            this.ShowInstructionBanner(Banner.START_TEXT);

            this.paddle = new Paddle(this.asyncGraphics, this.displayWidth, this.displayWidth, this.backgroundColor);
            this.ball = new Ball(this.asyncGraphics, this.displayWidth, this.displayHeight, this.backgroundColor, this.paddle, this, minimumY: Banner.HEIGHT + 1);
            this.ball.ExplosionOccurred += this.OnExplosionOccurred;
            this.ball.ScoreChanged += this.scoreBanner.OnScoreChanged;

            this.rotaryPaddle.Clicked += RotaryPaddle_Clicked;
            this.PlayBootSound();
        }

        public static void DebugWriteLine(string s)
        {
            Console.WriteLine(s);
        }

        private void RotaryPaddle_Clicked(object sender, EventArgs e)
        {
            ++this.rotaryPaddleClickCount;

            if (this.rotaryPaddleClickCount <= 1)
            {
                this.PlayStartSound();
                MeadowApp.DebugWriteLine("Processing knob click");
                this.graphics.Clear(true);
                this.asyncGraphics.Stop();
                this.ball.StopMoving();
                this.LoadScreen(eraseInstructionBanner : true, string.Empty, showScoreBanner: true);
                this.paddle.Reset();
                this.ball.Reset();
                this.ball.Score = 0;
                this.PlayStartSound();
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

        void ISounds.PlayBorderHitSound()
        {
            this.PlaySound(1500, 10);
        }

        void ISounds.PlayPaddleHitSound()
        {
            this.PlaySound(1300, 10);
        }

        void ISounds.PlayGameOverSound()
        {
            try
            {
                // A thread is needed to get the sound to play
                new Thread(() =>
                {
                    this.PlaySound(50, 500);
                }).Start();
            }
            catch (Exception)
            {
                // Sometimes thread creation fails. Make sure it doesn't crash the game.
                MeadowApp.DebugWriteLine($"Exception in PlayGameOverSound");
            }
        }

        public void PlayStartSound()
        {
            this.PlaySound(2000, 2);
            Thread.Sleep(10);
            this.PlaySound(2000, 2);
        }

        public void PlayInitialSound()
        {
            new Thread(() =>
            {
                this.PlaySound(1.1f, 1);
            }).Start();
        }

        public void PlayBootSound()
        {
            new Thread(() =>
            {
                this.PlaySound(1000, 1);
            }).Start();
        }

        private SoundMode SoundLevel
        {
            get
            {
                if (this.volumeIn1.State == true)
                {
                    return SoundMode.Normal;
                }
                else if (this.volumeIn2.State == true)
                {
                    return SoundMode.Silent;
                }
                else
                {
                    return SoundMode.Soft;
                }
            }
        }

        private int SoundDutyCycle
        {
            get
            {
                switch (this.SoundLevel)
                {
                    case SoundMode.Silent:
                        return 0;

                    case SoundMode.Soft:
                        return 1;

                    default:
                        return 20;
                }
            }
        }

        private int GetSoundDuration(int normalDuration)
        {
            switch (this.SoundLevel)
            {
                case SoundMode.Silent:
                    return 0;

                case SoundMode.Soft:
                    return normalDuration == 0 ? 0 : 1;

                default:
                    return normalDuration;
            }
        }

        private void PlaySound(float frequency, int duration)
        {
            this.speakerPWM.DutyCycle = this.SoundDutyCycle;

            if (this.SoundDutyCycle > 0)
            {
                this.speaker.PlayTone(frequency, this.GetSoundDuration(duration));
            }
        }

        private void OnExplosionOccurred(object sender, Ball.GameOverArgs args)
        {
            this.ShowInstructionBanner(Banner.RESTART_TEXT);
        }
    }
}