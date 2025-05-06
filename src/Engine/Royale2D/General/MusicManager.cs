using SFML.Audio;
using SFML.System;
using Shared;
using System.Globalization;

namespace Royale2D
{
    public class MusicData
    {
        public string name;
        public bool loop;
        public float loopStartPos;  // In seconds
        public float loopEndPos;    // In seconds
        FilePath musicFilePath;

        public MusicData(FilePath musicFilePath)
        {
            this.musicFilePath = musicFilePath;

            string[] pieces = musicFilePath.fileNameNoExt.Split('.');
            name = pieces[0];

            int pieceIndex = 1;
            float startPos = 0;
            float endPos = 0;
            if (pieceIndex < pieces.Length && float.TryParse(pieces[pieceIndex].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out startPos))
            {
                pieceIndex++;
            }
            if (pieceIndex < pieces.Length && float.TryParse(pieces[pieceIndex].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out endPos))
            {
                pieceIndex++;
            }

            if (endPos != 0)
            {
                loop = true;
                loopStartPos = startPos;
                loopEndPos = endPos;
            }
        }

        // PERF object pooling?
        public Music GetMusic()
        {
            var music = new Music(musicFilePath.fullPath);
            music.Loop = true;
            // Only SFML.NET 2.5.1 and later supports LoopPoints feature
            music.LoopPoints = new Music.TimeSpan(Time.FromSeconds(loopStartPos), Time.FromSeconds(loopEndPos - loopStartPos));
            return music;
        }
    }

    public class MusicInstance
    {
        public string rawMusicName = "";    // "raw" means can be "overworld.50" meaning play overworld music at 50% volume
        public string musicName = "";
        public Music? music;
        public MusicData musicData => Assets.musicDatas[musicName];
        public float volumeModifier = 1;

        public MusicInstance()
        {
        }

        public void ChangeMusic(string newRawMusicName, float externalVolumeModifier)
        {
            if (rawMusicName == newRawMusicName) return;
            if (!Assets.musicDatas.ContainsKey(musicName)) return;

            rawMusicName = newRawMusicName;
            if (rawMusicName == "")
            {
                musicName = "";
                volumeModifier = 1;
                music?.Stop();
                music = null;
                return;
            }

            (string newMusicName, float newVolumeModifier) = GetMusicAndVolumeModifier(newRawMusicName);
            volumeModifier = newVolumeModifier;

            if (musicName != newMusicName)
            {
                musicName = newMusicName;
                music?.Stop();
                music = musicData.GetMusic();
                UpdateVolume(externalVolumeModifier);
                music.Play();
            }
        }

        public void UpdateVolume(float externalVolumeModifier)
        {
            if (music != null)
            {
                music.Volume = 100 * volumeModifier * externalVolumeModifier;
            }
        }

        public (string, float) GetMusicAndVolumeModifier(string rawMusicName)
        {
            string musicToPlay;
            float volumeModifier = 1;
            if (rawMusicName.Contains("."))
            {
                musicToPlay = rawMusicName.Split('.')[0];
                volumeModifier = float.Parse(rawMusicName.Split('.')[1]) / 100f;
            }
            else
            {
                musicToPlay = rawMusicName;
            }

            return (musicToPlay, volumeModifier);
        }
    }

    public class MusicManager
    {
        private static MusicManager _main;
        public static MusicManager main
        {
            get
            {
                if (_main == null) _main = new MusicManager();
                return _main;
            }
        }

        public MusicInstance musicInstance;

        // Sometimes we want an "overlay" music that plays on top of the current "base", with base being reduced in volume, i.e. twilight move warning music
        public MusicInstance overlayMusicInstance;
        
        public float baseMusicVolumeModifierWhenOverlay = 0.5f;

        public MusicManager()
        {
            musicInstance = new MusicInstance();
            overlayMusicInstance = new MusicInstance();
        }

        public void Update()
        {
            musicInstance.UpdateVolume(GetVolumeModifier());
            overlayMusicInstance.UpdateVolume(GetOverlayVolumeModifier());
        }

        // All the "Change" methods can be safely called every frame. Only on change would something actually happen
        public void ChangeMusic(string rawMusicName)
        {
            musicInstance.ChangeMusic(rawMusicName, GetVolumeModifier());
        }

        public void ChangeOverlayMusic(string rawOverlayMusicName)
        {
            overlayMusicInstance.ChangeMusic(rawOverlayMusicName, GetOverlayVolumeModifier());
        }

        private float GetVolumeModifier()
        {
            if (Debug.disableMusic) return 0;
            float baseModifier = 1;
            if (overlayMusicInstance.musicName != "") baseModifier *= baseMusicVolumeModifierWhenOverlay;
            // TODO multiply music setting here too
            return baseModifier;
        }

        private float GetOverlayVolumeModifier()
        {
            if (Debug.disableMusic) return 0;
            float baseModifier = 1;
            // TODO multiply music setting here too
            return baseModifier;
        }
    }
}
