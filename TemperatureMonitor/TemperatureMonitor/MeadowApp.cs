namespace TemperatureMonitor
{
    using Meadow;
    using Meadow.Devices;
    using Meadow.Foundation;
    using Meadow.Foundation.Displays.Tft;
    using Meadow.Foundation.Graphics;
    using Meadow.Foundation.Sensors.Temperature;
    using Meadow.Hardware;
    using Meadow.Peripherals.Sensors.Atmospheric;
    using System;
    using System.Runtime.CompilerServices;
    using System.Timers;

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Color[] colors = new Color[4]
        {
            Color.FromHex("#008500"),
            Color.FromHex("#269926"),
            Color.FromHex("#00CC00"),
            Color.FromHex("#67E667")
        };

        private AnalogTemperature analogTemperature;

        private St7789 st7789;

        private GraphicsLibrary graphics;

        private int displayWidth;

        private int displayHeight;

        private Timer unitsTimer = new Timer(5000);

        private bool displayFarenheit;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            this.analogTemperature = new AnalogTemperature(
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );
            this.analogTemperature.Updated += AnalogTemperatureUpdated;

            var config = new SpiClockConfiguration(6000,SpiClockConfiguration.Mode.Mode3);

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

            this.LoadScreen();
            this.analogTemperature.StartUpdating();

            this.unitsTimer.AutoReset = true;
            this.unitsTimer.Elapsed += UnitsTimer_Elapsed;
            this.unitsTimer.Start();
        }

        private void UnitsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.displayFarenheit = !this.displayFarenheit;
        }

        private void LoadScreen()
        {
            Console.WriteLine("LoadScreen...");

            this.graphics.Clear();

            int radius = 225;
            int originX = this.displayWidth / 2;
            int originY = this.displayHeight / 2 + 130;

            this.graphics.Stroke = 3;

            for (int i = 1; i < 5; i++)
            {
                this.graphics.DrawCircle(
                    centerX: originX,
                    centerY: originY,
                    radius: radius,
                    color: this.colors[i - 1],
                    filled: true);

                this.graphics.Show();
                radius -= 20;
            }

            this.graphics.DrawLine(0, 220, 240, 220, Color.White);
            this.graphics.DrawLine(0, 230, 240, 230, Color.White);

            this.graphics.CurrentFont = new Font12x20();
            this.graphics.DrawText(54, 130, "TEMPERATURE", Color.White);

            this.graphics.Show();
        }

        private string oldTemperatureString = "";
        private int oldTemperatureX = 48;

        void AnalogTemperatureUpdated(object sender, AtmosphericConditionChangeResult e)
        {
            bool displayF = this.displayFarenheit;

            float newTemp = (float)(e.New.Temperature / 1000);

            string temperatureString;
            
            if (displayF)
            {
                float newTempF = (newTemp * 9 / 5) + 32;
                temperatureString = string.Format("{0}°F", newTempF.ToString("##.#"));
            }
            else
            {
                temperatureString = string.Format("{0}°C", newTemp.ToString("##.#"));
            }

            int temperatureX = (int)((this.displayWidth - (temperatureString.Length * 24)) / 2);

            // erase the old value
            this.graphics.DrawText(
                x: this.oldTemperatureX, y: 160,
                text: oldTemperatureString,
                color: colors[colors.Length - 1],
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);

            // display the new value
            this.graphics.DrawText(
                x: temperatureX, y: 160,
                text: temperatureString,
                color: Color.White,
                scaleFactor: GraphicsLibrary.ScaleFactor.X2);

            graphics.Show();

            this.oldTemperatureString = temperatureString;
            this.oldTemperatureX = temperatureX;
        }
    }
}