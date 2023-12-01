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
        private List<AudioStreamPlayer2D> _positionalSfxPlayerPool = new List<AudioStreamPlayer2D>();

        private AudioOptions _defaultSfxOpts = new AudioOptions { BusName = AudioBusName.Sfx };
        private AudioOptions _defaultMusicOpts = new AudioOptions { BusName = AudioBusName.Music };

        public AudioStreamPlayer PlayMusic(AudioStream track, AudioOptions opts = null) => Play(track, _musicPlayer, opts ?? _defaultMusicOpts);

        public AudioStreamPlayer PlaySfx(AudioStream sfx, AudioOptions opts = null)
        {
            return Play(sfx, _sfxPlayerPool.FirstOrDefault(asp => !asp.Playing), opts ?? _defaultSfxOpts);
        }

        public AudioStreamPlayer2D PlayPositionalSfx(AudioStream sfx, AudioOptions opts = null)
        {
            return PlayPositionalSfx(sfx, _positionalSfxPlayerPool.FirstOrDefault(asp => !asp.Playing), opts ?? _defaultSfxOpts);
        }
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

        private AudioStreamPlayer2D PlayPositionalSfx(AudioStream audioStream, AudioStreamPlayer2D player, AudioOptions opts)
        {
            if (player == null)
            {
                player = new AudioStreamPlayer2D();
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
            player.PauseMode = opts.PauseMode;
        }

        private void SetupPlayer(AudioStreamPlayer2D player, AudioStream audioStream, AudioOptions opts)
        {
            player.Stream = audioStream;
            player.VolumeDb = opts.Db;
            player.Bus = opts.BusName;
            player.MaxDistance = opts.MaxDistance;
            player.Attenuation = opts.Attenuation;
            player.PauseMode = opts.PauseMode;
        }
    }
}
