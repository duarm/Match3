using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class AudioPlayer : MonoBehaviour
    {
        static AudioPlayer Instance;

        [SerializeField] AudioClip music;
        AudioSource source;
        private void Awake ()
        {
            if (Instance != null)
                Destroy (gameObject);

            Instance = this;
            source = GetComponent<AudioSource> ();
        }

        private void Start ()
        {
            source.loop = true;
            source.clip = music;
            source.Play ();
        }

        public static void PlaySFX (AudioClip clip)
        {
            Instance.source.PlayOneShot (clip);
        }
    }
}