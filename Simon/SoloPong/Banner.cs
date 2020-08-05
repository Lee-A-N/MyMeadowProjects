namespace SoloPong
{
    using Meadow.Foundation;

    class Banner
    {
        public const int HEIGHT = 18;

        public const string START_TEXT = "Press knob to start";
        public const string RESTART_TEXT = "Press to restart";
        public const string SCORE_TEXT = "SCORE: ";

        private readonly int width;
        private readonly int top;
        private readonly AsyncGraphics asyncGraphics;
        private readonly Color backgroundColor;
        private readonly Color color;

        public Banner(int displayWidth, AsyncGraphics graphics, int fontHeight, Color backgroundColor, Color color, int top)
        {
            this.width = displayWidth;
            this.Height = Banner.HEIGHT;
            this.asyncGraphics = graphics;
            this.FontHeight = fontHeight;
            this.backgroundColor = backgroundColor;
            this.color = color;
            this.top = top;
        }

        public int FontHeight { get; set; }

        public string Text { get; set; }

        public int Height { get; set; }

        public void Draw()
        {
            this.asyncGraphics.DrawRectangle(
                x0: 0,
                y0: this.top,
                width: this.width,
                height: this.Height,
                color: this.color);

            int y = this.top + (this.Height - this.FontHeight) / 2;
            this.asyncGraphics.DrawText(this.Text, 5, y, Color.Black);
        }

        public void Hide()
        {
            this.asyncGraphics.DrawRectangle(
                x0: 0,
                y0: this.top,
                width: this.width,
                height: this.Height,
                color: this.backgroundColor);
        }

        public void OnScoreChanged(object sender, Ball.ScoreChangedArgs args)
        {
            int y = this.top + (this.Height - this.FontHeight) / 2;

            // erase the old score
            this.asyncGraphics.DrawText(args.OldScore.ToString(), this.width / 2, y, this.color);

            // draw the new score
            this.asyncGraphics.DrawText(args.Score.ToString(), this.width / 2, y, Color.Black);
        }
    }
}
