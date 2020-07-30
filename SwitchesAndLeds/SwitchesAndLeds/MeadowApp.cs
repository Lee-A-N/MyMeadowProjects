using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Motors;
using Meadow.Hardware;
using System.Text;

namespace SwitchesAndLeds
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private const string BACK = "back";
        private const string FORWARD = "forward";
        private const string RIGHT = "right";
        private const string LEFT = "left";
        private const string STOP = "stop";

        private delegate void command();

        private command[] commands = new command[5];

        private PushButton commandButton;
        private int commandIndex = 0;

        private System.Timers.Timer debounceTimer = new System.Timers.Timer(500);
        private bool isClickDetected;

        private IDigitalOutputPort onboardRedLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);
        private IDigitalOutputPort onboardBlueLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
        private IDigitalOutputPort onboardGreenLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);

        private CarController carController;

        private ISerialMessagePort serialPort;

        public MeadowApp()
        {
            //this.commandButton = new PushButton(Device, Device.Pins.D10, debounceDuration: 50);
            //this.commandButton.Clicked += this.CommandButton_Button_Clicked;
            ////this.commandButton.PressEnded += CommandButton_PressEnded;

            //this.debounceTimer.Elapsed += DebounceTimer_Elapsed;

            var motorLeft = new HBridgeMotor
            (
                a1Pin: Device.CreatePwmPort(Device.Pins.D05),
                a2Pin: Device.CreatePwmPort(Device.Pins.D06),
                enablePin: Device.CreateDigitalOutputPort(Device.Pins.D07)
            );

            var motorRight = new HBridgeMotor
            (
                a1Pin: Device.CreatePwmPort(Device.Pins.D02),
                a2Pin: Device.CreatePwmPort(Device.Pins.D03),
                enablePin: Device.CreateDigitalOutputPort(Device.Pins.D04)
            );

            carController = new CarController(motorLeft, motorRight);

            this.commands[0] = this.Back;
            this.commands[1] = this.Right;
            this.commands[2] = this.Forward;
            this.commands[3] = this.Left;
            this.commands[4] = this.Stop;

            this.serialPort = Device.CreateSerialMessagePort(Device.SerialPortNames.Com1, suffixDelimiter: Encoding.UTF8.GetBytes("\r\n"), preserveDelimiter: true, baudRate: 115200);
            this.serialPort.Open();
            this.serialPort.MessageReceived += SerialPort_MessageReceived;
            //Console.WriteLine("Serial port " + this.serialPort.PortName + " is " + (this.serialPort.IsOpen ? "open" : "closed"));
            this.serialPort.Write(Encoding.UTF8.GetBytes("serial port is open\r\n"));

            this.UpdateOnboardLed("red");

            Thread.Sleep(Timeout.Infinite);
        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            string message = e.GetMessageString(Encoding.UTF8).Trim();

            string response = "";

            switch(message)
            {
                case FORWARD:
                    this.Forward();
                    response = "Going Forward\r\n";
                    break;

                case BACK:
                    this.Back();
                    response = "Going Backward\r\n";
                    break;

                case RIGHT:
                    this.Right();
                    response = "Turning right\r\n";
                    break;

                case LEFT:
                    this.Left();
                    response = "Turning left\r\n";
                    break;

                case STOP:
                    this.Stop();
                    response = "Stopping\r\n";
                    break;

                default:
                    response = $"Unrecognized '{message}'\r\n";
                    Console.WriteLine($"Message not recognized: '{message}'");
                    break;
            }

            if (!string.IsNullOrEmpty(response))
            {
                this.serialPort.Write(Encoding.UTF8.GetBytes(response));
            }
        }

        private void UpdateOnboardLed(string color)
        {
            switch (color)
            {
                case "green":
                    this.onboardRedLed.State = false;
                    this.onboardGreenLed.State = true;
                    this.onboardBlueLed.State = false;
                    break;

                case "red":
                    this.onboardRedLed.State = true;
                    this.onboardGreenLed.State = false;
                    this.onboardBlueLed.State = false;
                    break;

                case "blue":
                    this.onboardRedLed.State = false;
                    this.onboardGreenLed.State = false;
                    this.onboardBlueLed.State = true;
                    break;

                case "purple":
                    this.onboardRedLed.State = true;
                    this.onboardGreenLed.State = false;
                    this.onboardBlueLed.State = true;
                    break;

                case "cyan":
                    this.onboardRedLed.State = false;
                    this.onboardGreenLed.State = true;
                    this.onboardBlueLed.State = true;
                    break;

                default:
                    this.onboardRedLed.State = false;
                    this.onboardGreenLed.State = false;
                    this.onboardBlueLed.State = false;
                    break;
            }
        }

        private void UpdateLeds(string cmd)
        {
            //Console.WriteLine(cmd);

            switch (cmd)
            {
                case BACK:
                    this.UpdateOnboardLed("cyan");
                    break;

                case RIGHT:
                    this.UpdateOnboardLed("blue");
                    break;

                case FORWARD:
                    this.UpdateOnboardLed("green");
                    break;

                case LEFT:
                    this.UpdateOnboardLed("purple");
                    break;

                default:
                    // stop
                    this.UpdateOnboardLed("red");
                    break;
            }
        }

        private void CommandButton_Button_Clicked(object sender, EventArgs e)
        {
            if (!this.isClickDetected)
            {
                this.isClickDetected = true;
                this.debounceTimer.Start();
                this.commands[this.commandIndex++]();
                this.commandIndex %= this.commands.Length;
            }
        }

        private void CommandButton_PressEnded(object sender, EventArgs e)
        {
            if (!this.isClickDetected)
            {
                this.isClickDetected = true;
                this.debounceTimer.Start();
                this.commands[this.commandIndex++]();
                this.commandIndex %= this.commands.Length;
            }
        }

        private void DebounceTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.isClickDetected = false;
        }

        private void Back()
        {
            this.carController.MoveBackward();
            this.UpdateLeds(BACK);
        }

        private void Forward()
        {
            this.carController.MoveForward();
            this.UpdateLeds(FORWARD);
        }

        private void Right()
        {
            this.carController.TurnRight();
            this.UpdateLeds(RIGHT);
        }

        private void Left()
        {
            this.carController.TurnLeft();
            this.UpdateLeds(LEFT);
        }

        private void Stop()
        {
            this.carController.Stop();
            this.UpdateLeds(STOP);
        }
    }
}
