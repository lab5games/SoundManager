using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Lab5Games;

namespace Lab5Games.Audio
{
    /*
     * https://github.com/prime31/SoundKit
     * */
    public class SoundManager : Singleton<SoundManager>
    {
        public enum EMixerGroups
        {
            Music       = 0,
            Effects     = 1
        }

        public AudioMixer audioMixer;
        public int maxSrcCount = 8;
        public bool autoIncreaseSrc = true;

        private List<USound> _playingSounds;
        private List<USound> _avaliableSounds;

        public USound BGM { get; private set; }

        
        public void SetMainVolume(float volume)
        {
            if(audioMixer)
            {
                audioMixer.SetFloat("Master", Mathf.Log(Remap(volume, 0f, 1f, -80f, 0f)) * 20f);
            }
        }

        public void SetVolume(EMixerGroups type, float volume)
        {
            if (audioMixer)
            {
                audioMixer.SetFloat(type.ToString(), Mathf.Log(Remap(volume, 0f, 1f, -80f, 0f)) * 20f);
            }
        }

        private float Remap(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public void ClearSounds()
        {
            if (BGM != null)
            {
                BGM.source.clip = null;
            }

            for(int i=_playingSounds.Count-1; i>=0; i--)
            {
                USound s = _playingSounds[i];
                s.source.clip = null;

                _avaliableSounds.Add(s);
                _playingSounds.RemoveAt(i);
            }
        }

        public USound PlayBGM(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                DebugEx.Log(ELogType.Warning, "SoundManager: Failed to play BGM, clip is null.");
                return null;
            }

            if (BGM == null)
            {
                BGM = new USound(CreateNewAudioSource(EMixerGroups.Music));
            }

            BGM.Play(clip, volume, 1, 0, true);

            return BGM;
        }

        public USound PlaySound(AudioClip clip, bool loop = false)
        {
            return PlaySound(clip, 1, 1, 0, loop);
        }

        public USound PlaySound(AudioClip clip, float volume, bool loop = false)
        {
            return PlaySound(clip, volume, 1, 0, loop);
        }

        public USound PlaySound(AudioClip clip, float volume, float pitch, float pan, bool loop)
        {
            if (clip == null)
            {
                DebugEx.Log(ELogType.Warning, "SoundManager: Failed to play sound, clip is null.");
                return null;
            }

            USound sound = GetAvaliableSound();

            if (sound != null)
            {
                sound.Play(clip, volume, pitch, pan, loop);
            }

            return sound;
        }

        public void RecycleSound(USound sound)
        {
            if (sound.Equals(BGM)) 
                return;

            for(int i=0; i<_playingSounds.Count; i++)
            {
                if(sound == _playingSounds[i])
                {
                    _playingSounds.RemoveAt(i);
                    break;
                }
            }

            if((_avaliableSounds.Count + _playingSounds.Count) > maxSrcCount)
            {
                Destroy(sound.source);
            }
            else
            {
                _avaliableSounds.Add(sound);
            }
        }

        private USound GetAvaliableSound()
        {
            USound sound = null;

            if(_avaliableSounds.Count > 0)
            {
                int lastIndx = _avaliableSounds.Count - 1;
                sound = _avaliableSounds[lastIndx];
                _avaliableSounds.RemoveAt(lastIndx);
            }

            if(sound == null && autoIncreaseSrc)
            {
                sound = new USound(CreateNewAudioSource(EMixerGroups.Effects));
            }

            if (sound != null)
            {
                _playingSounds.Add(sound);
            }

            return sound;
        }

        private AudioSource CreateNewAudioSource(EMixerGroups type)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();

            if (audioMixer != null)
            {
                src.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[(int)type];
            }

            return src;
        }
        
        private void Awake()
        {
            _playingSounds = new List<USound>(maxSrcCount);
            _avaliableSounds = new List<USound>(maxSrcCount);

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            for(int i=0; i<maxSrcCount; i++)
            {
                _avaliableSounds.Add(new USound(CreateNewAudioSource(EMixerGroups.Effects)));
            }
        }

        private void Update()
        {
            for(int i=_playingSounds.Count-1; i>=0; i--)
            {
                USound s = _playingSounds[i];

                if (s.source.loop) 
                    continue;

                s.elpasedTime += Time.deltaTime;
                
                if (s.elpasedTime > s.source.clip.length)
                    s.Stop();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _playingSounds = null;
            _avaliableSounds = null;
        }
    }
}
