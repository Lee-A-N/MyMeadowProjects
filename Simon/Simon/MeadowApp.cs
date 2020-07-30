namespace Simon
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation.Audio;
    using Meadow.Foundation.Leds;
    using Meadow.Foundation.Sensors.Buttons;
    using Meadow.Hardware;
    using System;
    using System.Threading;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private const int ANIMATION_DELAY = 200;

        private float[] notes = new float[] { 261.63f, 329.63f, 392, 523.25f };

        private Led[] leds = new Led[4];

        private PushButton[] pushButtons = new PushButton[4];

        private PiezoSpeaker speaker;

        private bool isAnimating = false;

       private SimonGame game = new SimonGame();

        public MeadowApp()
        {
            this.leds[0] = new Led(Device.CreateDigitalOutputPort(Device.Pins.D10));
            this.leds[1] = new Led(Device.CreateDigitalOutputPort(Device.Pins.D09));
            this.leds[2] = new Led(Device.CreateDigitalOutputPort(Device.Pins.D08));
            this.leds[3] = new Led(Device.CreateDigitalOutputPort(Device.Pins.D07));

            this.pushButtons[0] = new PushButton(
                Device.CreateDigitalInputPort(
                    Device.Pins.D01,
                    InterruptMode.EdgeBoth,
                    ResistorMode.Disabled
            ));
            this.pushButtons[0].Clicked += ButtonRedClicked;

            this.pushButtons[1] = new PushButton(
                Device.CreateDigitalInputPort(
                    Device.Pins.D02,
                    InterruptMode.EdgeBoth,
                    ResistorMode.Disabled
            ));
            this.pushButtons[1].Clicked += ButtonGreenClicked;

            this.pushButtons[2] = new PushButton(
                Device.CreateDigitalInputPort(
                    Device.Pins.D03,
                    InterruptMode.EdgeBoth,
                    ResistorMode.Disabled
            ));
            this.pushButtons[2].Clicked += ButtonBlueClicked;

            this.pushButtons[3] = new PushButton(
                Device.CreateDigitalInputPort(
                    Device.Pins.D04,
                    InterruptMode.EdgeBoth,
                    ResistorMode.Disabled
            ));
            this.pushButtons[3].Clicked += ButtonYellowClicked;

            this.speaker = new PiezoSpeaker(Device.CreatePwmPort(Device.Pins.D11));

            Console.WriteLine("Welcome to Simon");
            this.SetAllLEDs(true);
            this.game.OnGameStateChanged += OnGameStateChanged;
            this.game.Reset();
        }

        private void ButtonRedClicked(object sender, EventArgs e)
        {
            this.OnButton(0);
        }

        private void ButtonGreenClicked(object sender, EventArgs e)
        {
            this.OnButton(1);
        }
        
        private void ButtonBlueClicked(object sender, EventArgs e)
        {
            this.OnButton(2);
        }
        
        private void ButtonYellowClicked(object sender, EventArgs e)
        {
            this.OnButton(3);
        }

        private void OnButton(int buttonIndex)
        {
            Console.WriteLine("Button tapped: " + buttonIndex);

            if (this.isAnimating == false)
            {
                this.TurnOnLED(buttonIndex);
                this.game.EnterStep(buttonIndex);
            }
        }

        private object lockObject = new object();

        void OnGameStateChanged(object sender, SimonEventArgs e)
        {
            var th = new Thread(() =>
            {
                lock (lockObject)
                {
                    switch (e.GameState)
                    {
                        case GameState.Start:
                            break;

                        case GameState.NextLevel:
                            this.ShowStartAnimation();
                            this.ShowNextLevelAnimation(game.Level);
                            this.ShowSequenceAnimation(game.Level);
                            break;

                        case GameState.GameOver:
                            this.ShowGameOverAnimation();
                            this.game.Reset();
                            break;

                        case GameState.Win:
                            this.ShowGameWonAnimation();
                            break;
                    }
                }
            });

            th.Start();
        }

        private void TurnOnLED(int index, int duration = 400)
        {
            this.leds[index].IsOn = true;
            this.speaker.PlayTone(notes[index], duration);
            this.leds[index].IsOn = false;
        }

        private void SetAllLEDs(bool isOn)
        {
            this.leds[0].IsOn = isOn;
            this.leds[1].IsOn = isOn;
            this.leds[2].IsOn = isOn;
            this.leds[3].IsOn = isOn;
        }

        private void ShowStartAnimation()
        {
            if (this.isAnimating)
                return;

            this.isAnimating = true;

            this.SetAllLEDs(false);

            for (int i = 0; i < 4; i++)
            {
                this.leds[i].IsOn = true;
                Thread.Sleep(ANIMATION_DELAY);
            }

            for (int i = 0; i < 4; i++)
            {
                this.leds[3 - i].IsOn = false;
                Thread.Sleep(ANIMATION_DELAY);
            }

            this.isAnimating = false;
        }

        private void ShowNextLevelAnimation(int level)
        {
            if (this.isAnimating)
                return;

            this.isAnimating = true;

            this.SetAllLEDs(false);

            for (int i = 0; i < level; i++)
            {
                Thread.Sleep(ANIMATION_DELAY);
                this.SetAllLEDs(true);
                Thread.Sleep(ANIMATION_DELAY * 3);
                this.SetAllLEDs(false);
            }

            this.isAnimating = false;
        }

        private void ShowSequenceAnimation(int level)
        {
            if (this.isAnimating)
                return;

            this.isAnimating = true;

            var steps = this.game.GetStepsForLevel();
            this.SetAllLEDs(false);

            for (int i = 0; i < level; i++)
            {
                Thread.Sleep(200);
                this.TurnOnLED(steps[i], 400);
            }

            this.isAnimating = false;
        }

        private void ShowGameOverAnimation()
        {
            if (this.isAnimating)
                return;

            this.isAnimating = true;
            this.speaker.PlayTone(123.47f, 750);

            for (int i = 0; i < 20; i++)
            {
                this.SetAllLEDs(false);
                Thread.Sleep(50);
                this.SetAllLEDs(true);
                Thread.Sleep(50);
            }
            isAnimating = false;
        }

        void ShowGameWonAnimation()
        {
            this.ShowStartAnimation();
            this.ShowStartAnimation();
            this.ShowStartAnimation();
            this.ShowStartAnimation();
        }
    }
}