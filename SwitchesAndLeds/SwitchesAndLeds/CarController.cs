namespace SwitchesAndLeds
{
    using Meadow.Foundation.Motors;
    using System;

    public class CarController
    {
        float SPEED = 0.75f;
        HBridgeMotor motorLeft;
        HBridgeMotor motorRight;

        public CarController(HBridgeMotor motorLeft, HBridgeMotor motorRight)
        {
            this.motorLeft = motorLeft;
            this.motorRight = motorRight;
        }

        public void Stop()
        {
            Console.WriteLine("stop");
            motorLeft.Speed = 0f;
            motorRight.Speed = 0f;
        }

        public void TurnLeft()
        {
            Console.WriteLine("left");
            motorLeft.Speed = SPEED;
            motorRight.Speed = -SPEED;
        }

        public void TurnRight()
        {
            Console.WriteLine("right");
            motorLeft.Speed = -SPEED;
            motorRight.Speed = SPEED;
        }

        public void MoveForward()
        {
            Console.WriteLine("forward");
            motorLeft.Speed = -SPEED;
            motorRight.Speed = -SPEED;
        }

        public void MoveBackward()
        {
            Console.WriteLine("backward");
            motorLeft.Speed = SPEED;
            motorRight.Speed = SPEED;
        }
    }
}
