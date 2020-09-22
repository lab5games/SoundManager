﻿using System;
using UnityEngine;

namespace Lab5Games
{
    public class USound
    {

        public AudioSource source { get; private set; }

        public float elpasedTime { get; internal set; }

        public USound(AudioSource audioSource)
        {
            source = audioSource;

            source.priority = 128;
            source.pitch = 1;
            source.panStereo = 0;
            source.playOnAwake = false;
        }

        public void Stop()
        {
            source.Stop();

            SoundManager.instance.RecycleSound(this);
        }

        public void Play(AudioClip clip, float volume, float pitch, float pan, bool loop)
        {
            elpasedTime = 0;

            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.panStereo = pan;
            source.loop = loop;

            source.Play();
        }
    }
}