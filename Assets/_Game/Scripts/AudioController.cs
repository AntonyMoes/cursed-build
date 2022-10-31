using System;
using System.Linq;
using DG.Tweening;
using GeneralUtils;
using UnityEngine;

namespace _Game.Scripts {
    public class AudioController : SingletonBehaviour<AudioController> {
        [SerializeField] private AudioSource _music;
        [SerializeField] private AudioSource[] _sounds;

        private float _defaultMusicVolume;

        private void Awake() {
            _defaultMusicVolume = _music.volume;
        }

        public void SetMusicVolume(float volume, bool instant = true) {
            _music.DOFade(_defaultMusicVolume * volume, instant ? 0f : 0.3f);
        }

        public void PlayMusic() {
            if (_music.isPlaying) {
                return;
            }

            _music.Play();
        }

        public void PlaySound(string sound) {
            _sounds.First(source => source.clip.name == sound).Play();
        }
    }
}