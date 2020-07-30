namespace RotaryLedBar
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.ICs.IOExpanders;
    using Meadow.Foundation.Leds;
    using Meadow.Foundation.Sensors.Rotary;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Rotary;
    using System;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private float percentage;
        private readonly x74595 shiftRegister;
        private readonly LedBarGraph ledBarGraph;
        private readonly RotaryEncoder rotaryEncoder;
        private readonly RgbPwmLed onboardLed;

        public MeadowApp()
        {
            this.onboardLed = new RgbPwmLed(Device,
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);
            this.onboardLed.SetColor(Color.Red);

            this.shiftRegister = new x74595(Device, Device.CreateSpiBus(), Device.Pins.D00, 8);
            this.shiftRegister.Clear();

            IDigitalOutputPort[] ports =
            {
                Device.CreateDigitalOutputPort(Device.Pins.D14),
                Device.CreateDigitalOutputPort(Device.Pins.D15),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP0, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP1, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP2, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP3, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP4, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP5, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP6, false, OutputType.PushPull),
                shiftRegister.CreateDigitalOutputPort(
                    shiftRegister.Pins.GP7, false, OutputType.PushPull),
            };

            this.ledBarGraph = new LedBarGraph(ports);

            this.rotaryEncoder = new RotaryEncoder(
                Device, Device.Pins.D02, Device.Pins.D03);
           this. rotaryEncoder.Rotated += this.RotaryEncoderRotated;

            this.onboardLed.SetColor(Color.Green);
        }

        void RotaryEncoderRotated(object sender, RotaryTurnedEventArgs e)
        {
            if (e.Direction == RotationDirection.Clockwise)
                this.percentage += 0.05f;
            else
                this.percentage -= 0.05f;

            if (this.percentage > 1f)
                this.percentage = 1f;
            else if (percentage < 0f)
                this.percentage = 0f;

            this.ledBarGraph.Percentage = this.percentage;

            Console.WriteLine($"Percentage is {this.percentage}");
        }
    }
}
