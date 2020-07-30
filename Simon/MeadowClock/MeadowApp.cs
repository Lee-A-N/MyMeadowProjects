using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Peripherals;
using Meadow.Hardware;

namespace MeadowClock
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        PushButton minute;
        PushButton hour;
        CharacterDisplay display;
        IDigitalInputPort inputPort;

        public MeadowApp()
        {
            try
            {
                hour = new PushButton(Device, Device.Pins.D15);
                hour.Clicked += HourClicked;

                Console.WriteLine("subscribe test");
                //minute = new PushButton(Device, Device.Pins.D12);
                //minute.Clicked += MinuteClicked;
                this.inputPort = Device.CreateDigitalInputPort(Device.Pins.D12, interruptMode:  InterruptMode.EdgeBoth, debounceDuration: 100);

                //this.inputPort.Changed += (object sender, DigitalInputPortEventArgs e) =>
                //{
                //    //Console.WriteLine("Change detected");
                //    Console.WriteLine($"Old school event raised; Time: {e.New.Millisecond}, Value: {e.Value}");
                //};

                this.inputPort.Subscribe(new FilterableChangeObserver<DigitalInputPortEventArgs, DateTime>(
                    e =>
                    {
                        if (!e.Value)
                        {
                            Console.WriteLine($"Event observed at {e.New.Millisecond}, Value: {e.Value}\r\n");
                            this.MinuteClicked(null, null);
                        }
                    },
                    f =>
                    {
                        return true; // return (f.Delta > new TimeSpan(0, 0, 0, 0, 100));
                    }));

                display = new CharacterDisplay
                (
                    device: Device,
                    pinRS: Device.Pins.D10,
                    pinE: Device.Pins.D09,
                    pinD4: Device.Pins.D08,
                    pinD5: Device.Pins.D07,
                    pinD6: Device.Pins.D06,
                    pinD7: Device.Pins.D05
                );

                Device.SetClock(new DateTime(2020, 07, 24, 11, 00, 00));

                CharacterDisplayClock();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.ToString()}");
            }
        }

        void HourClicked(object sender, EventArgs e)
        {
            Device.SetClock(DateTime.Now.AddHours(1));
            this.UpdateTime(DateTime.Now);
        }

        void MinuteClicked(object sender, EventArgs e)
        {
            Device.SetClock(DateTime.Now.AddMinutes(1));
            this.UpdateTime(DateTime.Now);
        }

        private const int DISPLAY_CHAR_COUNT = 20;
        private string marqueeText;

        private void WriteLine(string s, byte lineIndex)
        {
            display.WriteLine(s, lineIndex);
        }

        private void UpdateTime(DateTime currentTime)
        {
            string time = $"{currentTime:hh}:{currentTime:mm}:{currentTime:ss} {currentTime:tt}";
            int padCount = (int)((DISPLAY_CHAR_COUNT - time.Length) / 2);
            string paddedTime = new string(' ', padCount) + time;

            new Thread(() =>
            {
                display.WriteLine($"{paddedTime}", 3);
            }).Start();
        }

        private void CharacterDisplayClock()
        {
            int lastSecond = -1;
            int lastDay = -1;
            bool swapWriteOrder = false;

            try
            {
                display.ClearLines();
                display.WriteLine($"", 1);

                string pad = new string(' ', 2 * DISPLAY_CHAR_COUNT - 5);
                marqueeText = "abcde" + pad;

                while (true)
                {
                    string line0 = this.marqueeText.Substring(0, DISPLAY_CHAR_COUNT);
                    string line1 = this.marqueeText.Substring(DISPLAY_CHAR_COUNT, DISPLAY_CHAR_COUNT);

                    if (swapWriteOrder)
                    {
                        this.WriteLine(line1, 1);
                        this.WriteLine(line0, 0);
                    }
                    else
                    {
                        this.WriteLine(line0, 0);
                        this.WriteLine(line1, 1);
                    }

                    DateTime currentTime = DateTime.Now;

                    if (currentTime.Second != lastSecond)
                    {
                        this.UpdateTime(currentTime);
                        lastSecond = currentTime.Second;
                    }

                    if (currentTime.Day != lastDay)
                    {
                        string date = $"{currentTime:MM}/{currentTime:dd}/{currentTime:yyyy}";
                        int padCount = (int)((DISPLAY_CHAR_COUNT - date.Length) / 2);
                        string paddedDate = new string(' ', padCount) + date;

                        new Thread(() =>
                        {
                            display.WriteLine($"{paddedDate}", 2);
                        }).Start();

                        lastDay = currentTime.Day;
                    }

                    char c = marqueeText[2 * DISPLAY_CHAR_COUNT - 1];
                    swapWriteOrder = c != ' ';

                    this.marqueeText = $"{c}{marqueeText.Substring(0, 2 * DISPLAY_CHAR_COUNT - 1)}";

                    Thread.Sleep(80);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"caught exception {e}");
            }
        }
    }
}
