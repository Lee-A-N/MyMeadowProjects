namespace Simon
{
    using System;
    using System.Threading;

    public enum GameState
    {
        Start,
        NextLevel,
        Win,
        GameOver
    }

    public class SimonEventArgs : EventArgs
    {
        public GameState GameState { get; set; }

        public SimonEventArgs(GameState state)
        {
            this.GameState = state;
        }
    }

    class SimonGame
    {
        static int MAX_LEVELS = 4;
        static int NUM_BUTTONS = 4;

        public delegate void GameStateChangedDelegate(object sender, SimonEventArgs e);

        public event GameStateChangedDelegate OnGameStateChanged = delegate { };

        public int Level { get; set; }

        int currentStep;

        int[] Steps = new int[MAX_LEVELS];

        Random rand = new Random((int)DateTime.Now.Ticks);

        public void Reset()
        {
            this.OnGameStateChanged(this, new SimonEventArgs(GameState.Start));
            this.Level = 0;
            this.currentStep = 0;
            this.NextLevel();
        }

        public int[] GetStepsForLevel()
        {
            var steps = new int[Level];

            for (int i = 0; i < Level; i++)
            {
                steps[i] = this.Steps[i];
            }

            return steps;
        }

        public void EnterStep(int step)
        {
            if (this.Steps[this.currentStep] == step)
            {
                ++this.currentStep;
            }
            else
            {
                this.OnGameStateChanged(this, new SimonEventArgs(GameState.GameOver));
                this.Reset();
            }

            if (this.currentStep == this.Level)
            {
                this.NextLevel();
            }
        }

        private void NextLevel()
        {
            this.currentStep = 0;
            this.Level++;

            if (this.Level >= MAX_LEVELS)
            {
                this.OnGameStateChanged(this, new SimonEventArgs(GameState.Win));
                this.Reset();
                return;
            }

            var level = string.Empty;

            for (int i = 0; i < this.Level; ++i)
            {
                this.Steps[i] = this.rand.Next(NUM_BUTTONS);
                level += this.Steps[i] + ", ";
            }

            Console.WriteLine("steps for level " + this.Level + " are " + level);

            this.OnGameStateChanged(this, new SimonEventArgs(GameState.NextLevel));
        }
    }
}
