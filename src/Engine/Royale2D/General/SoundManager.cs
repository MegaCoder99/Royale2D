using SFML.Audio;
using Shared;

namespace Royale2D
{
    public class SoundManager
    {
        public List<SoundInstance> soundInstances = [];

        // CLEANUP world destroyed => soundPool destroyed
        public static Dictionary<int, Sound> soundPool = new Dictionary<int, Sound>();
        public static int autoIncSoundId;

        public void Update(Camera camera)
        {
            foreach (SoundInstance soundInstance in soundInstances.ToList())
            {
                soundInstance.Update();
                if (soundInstance.isOver)
                {
                    soundInstances.Remove(soundInstance);
                }
                else
                {
                    Sound? sound = soundPool.GetValueOrDefault(soundInstance.id);
                    if (sound == null)
                    {
                        sound = new Sound(Assets.GetSoundBuffer(soundInstance.soundName));
                        sound.Volume = soundInstance.GetVolume(camera);
                        sound.Play();
                        soundPool[soundInstance.id] = sound;
                    }
                    else
                    {
                        sound.Volume = soundInstance.GetVolume(camera);
                    }
                }
            }

            foreach ((int id, Sound sound) in soundPool.ToList())
            {
                if (!soundInstances.Any(si => si.id == id))
                {
                    sound.Stop();
                    sound.Dispose();
                    soundPool.Remove(id);
                }
            }
        }

        public void AddSound(string soundName, Actor actor, bool dontAddIfExistsOnActor)
        {
            if (dontAddIfExistsOnActor && soundInstances.Any(si => si.actor == actor && si.soundName == soundName)) return;

            // Throttle same sounds that are close by and played recently
            foreach (SoundInstance si in soundInstances)
            {
                if (actor.pos.RoughDistTo(si.actor.pos) < 100 && soundName == si.soundName && si.frameTime < 10)
                {
                    return;
                }
            }

            var soundInstance = new SoundInstance(soundName, actor);
            soundInstances.Add(soundInstance);
        }

        public void StopSound(string soundName, Actor actor)
        {
            foreach (SoundInstance soundInstance in soundInstances.ToList())
            {
                if (soundInstance.actor == actor && soundInstance.soundName == soundName)
                {
                    soundInstances.Remove(soundInstance);
                }
            }
        }
    }

    // Only to be used in World, main menu just use Game.PlaySound()
    public class SoundInstance
    {
        public string soundName;
        public Actor actor;     // We don't have actor component with sounds, but go the other way around, because the actor can be destroyed before its sound is
        public int frameTime;
        public int maxFrameTime;
        public int id;

        public bool isOver => frameTime >= maxFrameTime;

        public SoundInstance(string soundName, Actor actor)
        {
            this.soundName = soundName;
            this.actor = actor;
            this.id = SoundManager.autoIncSoundId++;
            maxFrameTime = MyMath.Ceil(Assets.GetSoundBuffer(soundName).Duration.AsMilliseconds() * (60 / 1000f));
        }

        public void Update()
        {
            frameTime++;
        }

        public float GetVolume(Camera camera)
        {
            if (!camera.enabled || camera.section != actor.section)
            {
                return 0;
            }
            else
            {
                float minDist = 100;
                float falloffDist = 156;
                float distance = MyMath.ClampMin0(actor.pos.DistanceTo(camera.pos.ToFdPoint()).floatVal - minDist);
                float volume = (1 - (distance / falloffDist)) * 100;
                return MyMath.Clamp(volume, 0, 100);
            }
        }
    }
}
