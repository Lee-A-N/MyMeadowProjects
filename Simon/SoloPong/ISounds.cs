namespace SoloPong
{
    public interface ISounds
    {
        void PlayBorderHitSound();

        void PlayPaddleHitSound();

        void PlayGameOverSound();

        void PlayStartSound();
    }
}
