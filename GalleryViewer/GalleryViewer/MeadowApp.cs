using System;
using System.IO;
using System.Reflection;
using System.Threading;

using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using SimpleJpegDecoder;

namespace GalleryViewer
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Led red, green;
        St7789 display;
        GraphicsLibrary graphics;
        PushButton buttonNext;
        PushButton buttonPrevious;

        int selectedIndex;
        string[] images = { "XY_smaller.jpg", "tulipSmall.jpg", "Lee_small.jpg", "LAN icon.jpg" };

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            this.red = new Led(Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed));
            this.green = new Led(Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen));

            this.red.IsOn = true;

            this.buttonNext = new PushButton(
                Device.CreateDigitalInputPort(
                    Device.Pins.D03,
                    InterruptMode.EdgeBoth,
                    ResistorMode.Disabled
            ));
            this.buttonNext.Clicked += this.ButtonNextClicked;

            this.buttonPrevious = new PushButton(
                Device.CreateDigitalInputPort(
                    Device.Pins.D04,
                    InterruptMode.EdgeBoth,
                    ResistorMode.Disabled
            ));
            this.buttonPrevious.Clicked += this.ButtonPreviousClicked;

            var config = new SpiClockConfiguration(
                    speedKHz: 6000,
                    mode: SpiClockConfiguration.Mode.Mode3);

            this.display = new St7789
            (
                device: Device,
                spiBus: Device.CreateSpiBus(
                    clock: Device.Pins.SCK,
                    mosi: Device.Pins.MOSI,
                    miso: Device.Pins.MISO,
                    config: config),
                chipSelectPin: null,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240
            );

            this.display.IgnoreOutOfBoundsPixels = true;

            this.graphics = new GraphicsLibrary(display);
            this.graphics.Rotation = GraphicsLibrary.RotationType._270Degrees;

            this.ShowJpeg();

            this.red.IsOn = false;
            this.green.IsOn = true;
        }

        void ButtonNextClicked(object sender, EventArgs e)
        {
            this.red.IsOn = true;
            this.green.IsOn = false;

            ++this.selectedIndex;
            this.selectedIndex %= 4;

            Console.WriteLine("Selected Index is " + this.selectedIndex);

            this.ShowJpeg();

            this.red.IsOn = false;
            this.green.IsOn = true;
        }

        void ButtonPreviousClicked(object sender, EventArgs e)
        {
            this.red.IsOn = true;
            this.green.IsOn = false;

            --this.selectedIndex;
            this.selectedIndex += 4;
            this.selectedIndex %= 4;

            Console.WriteLine("Selected Index is " + this.selectedIndex);

            this.ShowJpeg();

            this.red.IsOn = false;
            this.green.IsOn = true;
        }

        void ShowJpeg()
        {
            var jpgData = this.LoadResource(images[this.selectedIndex]);
            var decoder = new JpegDecoder();
            var jpg = decoder.DecodeJpeg(jpgData);

            Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes");
            Console.WriteLine($"Width {decoder.Width}");
            Console.WriteLine($"Height {decoder.Height}");

            this.graphics.Clear();
            this.display.Show();

            int x = 0;
            int y = 0;
            byte r, g, b;

            for (int i = 0; i < jpg.Length; i += 3)
            {
                r = jpg[i];
                g = jpg[i + 1];
                b = jpg[i + 2];

                graphics.DrawPixel(x, y, Color.FromRgb(r, g, b));

                x++;

                if (x % decoder.Width == 0)
                {
                    y++;
                    x = 0;
                }
            }

            this.display.Show();
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"GalleryViewer.{filename}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
