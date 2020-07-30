using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Servos;
using Meadow.Hardware;

namespace ServoButton
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Servo servo;
        PushButton button;

        private int[] angles = { 0, 20, 40, 60, 80, 100, 120, 140 };

        private int angleIndex = 0;

        private System.Timers.Timer changeTimer = new System.Timers.Timer(1000);

        public MeadowApp()
        {
            var servoConfig = new ServoConfig(
                minimumAngle: 0,
                maximumAngle: 180,
                minimumPulseDuration: 700,
                maximumPulseDuration: 3000,
                frequency: 50);
            servo = new Servo(Device.CreatePwmPort(Device.Pins.D03), servoConfig);
            servo.RotateTo(0);

            button = new PushButton(Device, Device.Pins.D04);
            button.DebounceDuration = new TimeSpan(100);
            button.Clicked += ButtonClicked;

            //this.changeTimer.AutoReset = true;
            this.changeTimer.Elapsed += ChangeTimer_Elapsed;
            this.changeTimer.Start();

            // Keeps the app running
            Thread.Sleep(Timeout.Infinite);
        }

        private void ChangeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ++this.angleIndex;
            this.angleIndex %= this.angles.Length;
            servo.RotateTo(this.angles[this.angleIndex]);
        }

        void ButtonClicked(object sender, EventArgs e)
        {
            ++this.angleIndex;
            this.angleIndex %= this.angles.Length;
            servo.RotateTo(this.angles[this.angleIndex]);
        }
    }
}