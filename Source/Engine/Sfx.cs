using System.Collections.Generic;
using GlobalTypes;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    public static class Sfx
    {
        public static IReadOnlyDictionary<string, SoundEffect> Effects => _effects;

        public static float TotalVolume { get => _totalVolume; set => _totalVolume = value.Clamp01(); }

        private static float _totalVolume = 1.0f;
        private static Dictionary<string, SoundEffect> _effects = new();

        [Load]
        private static void Load()
        {
            string folderName = Asset.SoundsFolderName;
            _effects = Asset.LoadFolder<SoundEffect>(folderName);
        }

        public static void Play(string name) => Play(name, TotalVolume);
        public static void Play(string name, float volume) => Effects[name].Play(volume, 0, 0);

        public static Sound GetSound(string name) => new(Effects[name]);
    }

    public class Sound
    {
        public SoundEffectInstance Instance { get; init; }
        public SoundEffect Effect { get; init; }

        public float Volume 
        { 
            get => _originalVolume; 
            set
            {
                _originalVolume = value;
                Instance.Volume = _originalVolume * Sfx.TotalVolume; 
            }
        }
        public string Name => Effect.Name;
        public bool IsPlaying => Instance.State == SoundState.Playing;
        public bool IsPaused => Instance.State == SoundState.Paused;

        private float _originalVolume;

        public Sound(SoundEffect effect)
        {
            Effect = effect;
            Instance = Effect.CreateInstance();
        }

        public void Play() => Instance.Play();
        public void Stop() => Instance.Stop();
        public void Pause() => Instance.Pause();
        public void Dispose() => Instance.Dispose();
    }
}