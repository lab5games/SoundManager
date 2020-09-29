using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Lab5Games
{
    /*
     * https://github.com/prime31/SoundKit
     * */
    public class SoundManager : MonoBehaviour
    {
        public enum EVolumeTypes
        {
            Master      = 0,
            Music       = 1,
            Effects     = 2
        }

        private float _dt;

        private AudioMixer _audioMixer;

        private List<USound> _playingSounds = new List<USound>(MAX_SOUNDS);
        private List<USound> _avaliableSounds = new List<USound>(MAX_SOUNDS);

        public USound BGM { get; private set; }

        public const int MAX_SOUNDS = 8;

        public void SetVolume(EVolumeTypes type, float volume)
        {
            _audioMixer.SetFloat(type.ToString(), Mathf.Log(Remap(volume, 0f, 1f, -80f, 0f)) * 20f);
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
            if (BGM == null)
            {
                BGM = new USound(CreateNewAudioSource(EVolumeTypes.Music));
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
            USound sound = GetAvaliableSound();

            sound.Play(clip, volume, pitch, pan, loop);

            return sound;
        }

        public void RecycleSound(USound sound)
        {
            if (sound.Equals(BGM)) return;

            for(int i=0; i<_playingSounds.Count; i++)
            {
                if(sound == _playingSounds[i])
                {
                    _playingSounds.RemoveAt(i);
                    break;
                }
            }

            if((_avaliableSounds.Count + _playingSounds.Count) > MAX_SOUNDS)
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

            if(sound == null)
            {
                sound = new USound(CreateNewAudioSource(EVolumeTypes.Effects));
            }

            _playingSounds.Add(sound);

            return sound;
        }

        private AudioSource CreateNewAudioSource(EVolumeTypes type)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Master")[(int)type];

            return src;
        }


        private static SoundManager _instance = null;

        public static SoundManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SoundManager>();
                }

                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
            }
            else
            {
                _instance = this;

                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            _audioMixer = Resources.Load<AudioMixer>("SoundManager");

            for(int i=0; i<MAX_SOUNDS; i++)
            {
                _avaliableSounds.Add(new USound(CreateNewAudioSource(EVolumeTypes.Effects)));
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

        private void OnDestroy()
        {
            _instance = null;

            _audioMixer = null;
            _playingSounds = null;
            _avaliableSounds = null;
        }
    }
}
