using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KW_Mocap
{
    public class AudioCapture : MonoBehaviour
    {
        [SerializeField] private string m_DeviceName;
        private AudioClip m_AudioClip;
        private AudioSource m_MicAudioSource;

        void Awake()
        {
            m_MicAudioSource = GetComponent<AudioSource>();
        }
        void Start()
        {
            string targetDevice = "";

            foreach (var device in Microphone.devices)
            {
                Debug.Log($"Device Name: {device}");
                if (device.Contains(m_DeviceName))
                {
                    targetDevice = device;
                }
            }

            Debug.Log($"=== Device Set: {targetDevice} ===");
            m_AudioClip = Microphone.Start(targetDevice, true, 10, 48000);
            m_MicAudioSource.clip = m_AudioClip;
            m_MicAudioSource.Play();
        }
    }
}
