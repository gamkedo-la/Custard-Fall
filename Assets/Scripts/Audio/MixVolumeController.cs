using UnityEngine;
using UnityEngine.Audio;

public class MixVolumeController : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer = null;

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
}
