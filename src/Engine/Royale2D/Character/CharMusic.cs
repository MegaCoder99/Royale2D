namespace Royale2D
{
    public class CharMusic
    {
        public Character character;
        public string baseMusicName;
        public string musicZoneName = "";
        public int warningMusicTime;

        public CharMusic(Character character, string initialMusicName)
        {
            this.character = character;
            baseMusicName = initialMusicName;
        }

        // NOTE: stuff that calls MusicManager should NOT be here, but in UpdateSpecChar() instead. Each CharMusic is independent and runs on its own behind the scenes,
        // even if not being played, so if the controlled/spec'd char changes, so would the global MusicManager music to correspond to the character's unique CharMusic
        public void Update()
        {
            if (warningMusicTime > 0)
            {
                warningMusicTime--;
            }
            musicZoneName = GetMusicZoneName();
        }

        public void UpdateSpecChar()
        {
            MusicManager.main.ChangeMusic(GetMusicToPlay());
            MusicManager.main.ChangeOverlayMusic(GetOverlayMusicToPlay());
        }

        public string GetMusicZoneName()
        {
            foreach (PixelZone musicZone in character.section.mapSection.musicChangeZones)
            {
                if (character.pos.x.intVal >= musicZone.rect.x1 && character.pos.x.intVal <= musicZone.rect.x2 && character.pos.y.intVal >= musicZone.rect.y1 && character.pos.y.intVal <= musicZone.rect.y2)
                {
                    return musicZone.name;
                }
            }
            return "";
        }

        public void SetWarningMusic()
        {
            warningMusicTime = 220;
        }

        public string GetMusicToPlay()
        {
            if (character.IsWinner() && character.IsMainChar()) return "victory";
            if (character.section.mapSection.IsWoods())
            {
                if (character.world.masterSwordWoods?.isPulling == true) return "master_sword";
                else if (character.world.masterSwordWoods?.isPulled == true) return "overworld";
                else return "lost_woods";
            }
            if (character.bunnyComponent.bunnyTime > 0) return "bunny";
            if (musicZoneName != "") return musicZoneName;
            return baseMusicName;
        }

        public string GetOverlayMusicToPlay()
        {
            if (warningMusicTime > 0) return "warning";
            return "";
        }
    }
}
