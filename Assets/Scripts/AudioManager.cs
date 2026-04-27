using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance { get; private set; }
  public Sound[] music;
  public Sound[] sfx;

  private void Awake()
  {
    if (Instance) DestroyImmediate(gameObject);
    else
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
  }

  private void Start()
  {
    SetupSounds(music);
    SetupSounds(sfx);

    StartCoroutine(PlayRampSound("MainTheme"));
    if (!SaveManager.Instance.musicEnabled) MuteList(music);
    if (!SaveManager.Instance.sfxEnabled) MuteList(sfx);
  }

  void SetupSounds(Sound[] sounds)
  {
    foreach (Sound snd in sounds)
    {
      snd.source = gameObject.AddComponent<AudioSource>();
      snd.source.clip = snd.clip;

      snd.source.loop = snd.loop;
      snd.source.volume = snd.volume;
    }
  }

  Sound FindSound(string name)
  {
    Sound snd = Array.Find(music, snd => snd.name == name);
    if (snd == null) snd = Array.Find(sfx, snd => snd.name == name);

    return snd;
  }

  public void MuteList(Sound[] sources)
  {
    foreach (Sound snd in sources)
    {
      snd.source.mute = !snd.source.mute;
    }
  }

  public void PlaySound(string name, float pitchRandomRange = 0)
  {
    Sound snd = FindSound(name);
    if (snd == null) return;
    snd.source.pitch = 1 + UnityEngine.Random.Range(-pitchRandomRange, +pitchRandomRange);
    snd.source.Play();
  }

  public void StopSound(string name)
  {
    Sound snd = FindSound(name);
    if (snd == null) return;
    snd.source.Stop();
  }

  public IEnumerator StopRampSound(string name)
  {
    Sound snd = FindSound(name);
    if (snd == null) yield return null;
    yield return StartCoroutine(RampSound(snd.source, 0f));
    snd.source.Stop();
  }

  public IEnumerator PlayRampSound(string name)
  {
    Sound snd = FindSound(name);
    if (snd == null) yield return null;
    snd.source.volume = 0f;
    snd.source.Play();
    yield return StartCoroutine(RampSound(snd.source, snd.volume));
  }

  public void SwitchTracks(string track1, string track2)
  {
    StartCoroutine(SwitchTracksEnum(track1, track2));
  }

  IEnumerator SwitchTracksEnum(string track1, string track2)
  {
    yield return StartCoroutine(StopRampSound(track1));
    yield return StartCoroutine(PlayRampSound(track2));
  }

 IEnumerator RampSound(AudioSource source, float targetVolume)
  {
    var changeSign = Mathf.Sign(targetVolume - source.volume);
    while (Mathf.Sign(targetVolume - source.volume) == changeSign)
    {
      source.volume += Time.deltaTime * changeSign * 0.4f;
      yield return null;
    }
  }

}
