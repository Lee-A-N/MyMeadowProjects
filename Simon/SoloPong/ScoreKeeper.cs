using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoloPong
{
    public class ScoreKeeper
    {
        public class ScoreChangedArgs : EventArgs
        {
            public ScoreChangedArgs(int oldScore, int newScore)
            {
                this.OldScore = oldScore;
                this.Score = newScore;
            }

            public int OldScore { get; set; }

            public int Score { get; set; }
        }

        public delegate void NotifyScoreChanged(object sender, ScoreChangedArgs args);
        public event NotifyScoreChanged ScoreChanged;

        private int score = -1;

        public ScoreKeeper()
        {
            this.Reset();
        }

        public int Score
        {
            get
            {
                return this.score;
            }

            set
            {
                if (this.score != value)
                {
                    int oldScore = this.score;
                    this.score = value;
                    this.ScoreChanged?.Invoke(this, new ScoreChangedArgs(oldScore, this.score));
                }
            }
        }

        public void Reset()
        {
            this.Score = 0;
        }

        public void Increment()
        {
            ++this.Score;
        }
    }
}
