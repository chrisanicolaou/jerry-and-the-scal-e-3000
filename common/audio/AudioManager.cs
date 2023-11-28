using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ChiciStudios.GithubGameJam2023.Common.Audio
{
    public class AudioManager : Node
    {
        [Export] private int _sfxPoolSize;
        private AudioStreamPlayer _musicPlayer;
        private List<AudioStreamPlayer> _sfxPlayerPool = new List<AudioStreamPlayer>();

        private AudioOptions _defaultSfxOpts = new AudioOptions { BusName = AudioBusName.Sfx };
        private AudioOptions _defaultMusicOpts = new AudioOptions { BusName = AudioBusName.Music };

        public AudioStreamPlayer PlayMusic(AudioStream track, AudioOptions opts = null) => Play(track, _musicPlayer, opts ?? _defaultMusicOpts);
        public AudioStreamPlayer PlaySfx(AudioStream sfx,AudioOptions opts = null) => Play(sfx, _sfxPlayerPool.FirstOrDefault(asp => !asp.Playing), opts ?? _defaultSfxOpts);
        public void StopMusic() => _musicPlayer.Stop();

        public void SetBusDb(string busName, float db) => AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(busName), db);

        public override void _Ready()
        {
            _musicPlayer = new AudioStreamPlayer { Name = "MusicPlayer", PauseMode = PauseModeEnum.Process };
            AddChild(_musicPlayer);
            for (var i = 0; i < _sfxPoolSize; i++)
            {
                var sfxPlayer = new AudioStreamPlayer { Name = $"SfxPlayer{i}" };
                AddChild(sfxPlayer);
                _sfxPlayerPool.Add(sfxPlayer);
            }
        }

        private AudioStreamPlayer Play(AudioStream audioStream, AudioStreamPlayer player, AudioOptions opts)
        {
            if (player == null)
            {
                player = new AudioStreamPlayer();
                AddChild(player);
                _sfxPlayerPool.Add(player);
            }
            SetupPlayer(player, audioStream, opts);
            player.Play();
            return player;
        }

        private void SetupPlayer(AudioStreamPlayer player, AudioStream audioStream, AudioOptions opts)
        {
            player.Stream = audioStream;
            player.VolumeDb = opts.Db;
            player.Bus = opts.BusName;
        }
    }
}
