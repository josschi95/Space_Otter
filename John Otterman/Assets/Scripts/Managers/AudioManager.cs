using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private void Awake()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }

    private AudioSource source;

    [SerializeField] private AudioClip[] themes;
    [SerializeField] private AudioClip[] clips;
    private Dictionary<string, AudioClip> audioClips;

    private void Start()
    {
        audioClips = new Dictionary<string, AudioClip>();
        for (int i = 0; i < clips.Length; i++)
        {
            audioClips.Add(clips[i].name, clips[i]);
        }
    }

    public static void SetTheme(int sceneIndex)
    {
        AudioClip clip = null; //Main_Title for Hub and Credits
        if (sceneIndex == 0 || sceneIndex == 6) clip = instance.themes[0];
        else if (sceneIndex == 1) clip = instance.themes[1]; //Player_Hub
        else clip = instance.themes[2];

        instance.source.clip = clip;
        instance.source.Play();
    }

    public static void PlayEnemyClip(AudioClip clip)
    {
        instance.source.PlayOneShot(clip, 0.35f);
    }

    public static void PlayEnemyClip(string clipName)
    {
        instance.source.PlayOneShot(instance.GetClip(clipName), 0.35f);
    }

    public static void PlayClip(AudioClip clip)
    {
        instance.source.PlayOneShot(clip);
    }

    private void PlayClip_New(string clipName)
    {
        var nc = GetClip(clipName);
        if (nc == null) return;
        source.PlayOneShot(nc);
    }

    public static void PlayClip(string clipName)
    {
        instance.PlayClip_New(clipName);
        //instance.source.PlayOneShot(instance.GetClip(clipName));
    }

    private AudioClip GetClip(string clipName)
    {
        if (audioClips == null) return null;
        return audioClips[clipName];
    }
}
