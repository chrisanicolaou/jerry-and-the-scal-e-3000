namespace ChiciStudios.GithubGameJam2023.Common.Audio
{
    public class AudioOptions
    {
        public float Db { get; set; } = 0f;
        public string BusName { get; set; } = AudioBusName.Master;
        public float Attenuation { get; set; } = 1f;
        public float MaxDistance { get; set; } = 1000f;
    }
}