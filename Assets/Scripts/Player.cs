namespace KW_Mocap
{
    public interface Player
    {
        public void StartPlaying();
        public void PausePlaying();
        public void Skip(float seconds);
        public void ChangeSpeed(float speedRatio);
    }
}
