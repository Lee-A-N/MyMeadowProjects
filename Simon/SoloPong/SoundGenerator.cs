namespace SoloPong
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Audio;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;

    public class SoundGenerator : ISounds
    {
        private enum SoundMode
        {
            Silent,
            Soft,
            Normal
        }

        private readonly PiezoSpeaker speaker;

        private readonly IPwmPort speakerPWM;

        private readonly IDigitalInputPort volumeIn1;
        private readonly IDigitalInputPort volumeIn2;

        public SoundGenerator(IDigitalInputPort inputPort1, IDigitalInputPort inputPort2, IPwmPort pwmPort)
        {
            this.volumeIn1 = inputPort1;
            this.volumeIn2 = inputPort2;
            this.speakerPWM = pwmPort;

            this.speaker = new PiezoSpeaker(speakerPWM);

            this.PlayInitialSound();
        }

        public void PlayBorderHitSound()
        {
            this.PlaySound(1500, 10);
        }

        public void PlayPaddleHitSound()
        {
            this.PlaySound(1300, 10);
        }

        public void PlayGameOverSound()
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

        public void PlayConstructionCompleteSound()
        {
            new Thread(() =>
            {
                this.PlaySound(1200, 1);
                this.PlaySound(1200, 1);
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
    }
}
