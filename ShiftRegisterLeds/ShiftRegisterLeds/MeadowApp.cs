using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System.Threading;

namespace ShiftRegisterLeds
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        x74595 shiftRegister;

        public MeadowApp()
        {
            this.shiftRegister = new x74595(
                device: Device,
                spiBus: Device.CreateSpiBus(),
                pinChipSelect: Device.Pins.D03,
                pins: 8);

            this.TestX74595();
        }

        private void TestX74595()
        {
            while (true)
            {
                this.shiftRegister.Clear();

                for (uint num = 0; num < System.Math.Pow(2, this.shiftRegister.Pins.AllPins.Count); num++)
                {
                    for (int j = 0; j < this.shiftRegister.Pins.AllPins.Count; ++j )
                    {
                        int pin = 1 << j;
                        this.shiftRegister.WriteToPin(this.shiftRegister.Pins.AllPins[j], (num & pin) > 0);
                    }

                    Thread.Sleep(500);

                    this.shiftRegister.Clear();
                }
            }
        }
    }
}