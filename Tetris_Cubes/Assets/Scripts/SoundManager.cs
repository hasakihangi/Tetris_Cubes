using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// 统一管理Bgm和音效
public class SoundManager : MonoBehaviour
{
    public SoundEffectPool soundEffectPool;
    public static SoundManager Instance { get; private set; }

    private Dictionary<string, AudioClip> soundEffectClipTable = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> bgmClipTable = new Dictionary<string, AudioClip>();
    private List<AudioClip> bgms = new List<AudioClip>();

    public AudioSource bgmPlayer;

    private Coroutine playBgmCoroutine;
    private Coroutine pauseBgmCoroutine;
    
    private void Awake()
    {
        Instance = this;
        
        soundEffectPool = GetComponent<SoundEffectPool>();
        bgmPlayer = GetComponent<AudioSource>();
        
        // 可以加载, 这里rider提示有问题
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("sound/sound effect");
        if (audioClips == null)
        {
            Debug.Log("Load Audio Asset Err!");
        }
        else
        {
            foreach (AudioClip clip in audioClips)
            {
                soundEffectClipTable.Add(clip.name, clip);
            }
        }
        
        audioClips = Resources.LoadAll<AudioClip>("sound/bgm");
        if (audioClips == null)
        {
            Debug.Log("Load Audio Asset Err!");
        }
        else
        {
            foreach (AudioClip clip in audioClips)
            {
                // print(clip.name);
                bgmClipTable.Add(clip.name, clip);
            }
            bgms.AddRange(audioClips);
        }
    }

    // 最好从一开始就加载到内存中, 通过字典来索引
    public void PlaySoundEffect(string soundEffectName)
    {
        if (soundEffectClipTable.TryGetValue(soundEffectName, out var clip))
        {
            soundEffectPool.Get().Play(clip);
        }
        else
        {
            Debug.Log("AudioClipTable dont have this clip!");
        }
    }

    public void PauseBgm()
    {
        if (bgmPlayer.isPlaying)
        {
            // 如果正在进行暂停
            if (pauseBgmCoroutine != null)
            {
                return;
            }
            else
            {
                // 正在进行转换
                if (playBgmCoroutine != null)
                {
                    // 还没来得及置换clip
                    if (bgmPlayer.clip != targetClip)
                    {
                        // 停止转换, 开始暂停, 暂停后, 将bgmPlayer.clip换为targetClip, 并将targetClip置为null
                        StopCoroutine(playBgmCoroutine);
                        playBgmCoroutine = null;
                        StartCoroutine(PauseBgm(1f,  targetClip));
                    }
                    else // 已经置换了clip
                    {
                        StopCoroutine(playBgmCoroutine);
                        playBgmCoroutine = null;
                        StartCoroutine(PauseBgm(1f));
                    }
                }
                else // 正在某个状态上
                {
                    // 直接暂停
                    StartCoroutine(PauseBgm(1f));
                }
            }
        }
    }

    public void PlayBgm()
    {
        if (bgmPlayer.clip != null) 
        {
            // 如果是暂停状态
            if (!bgmPlayer.isPlaying)
            {
                // 恢复播放
                PlayBgm(bgmPlayer.clip);
            }
            else // 正在播放
            {
                // 正在进行暂停
                if (pauseBgmCoroutine != null)
                {
                    // PlayBgm(bgmPlayer.clip);
                    // 如果暂停后需要切换clip, 这时要播放那个切换的clip
                    if (targetClip != null)
                    {
                        PlayBgm(targetClip);
                    }
                    else
                    {
                        PlayBgm(bgmPlayer.clip);
                    }
                }
                else // 没进行暂停
                {
                    // 保持播放
                    return;
                }
            }
        }
        else // 如果还没播放过
        {
            // 随机选择一个进行播放
            int random = UnityEngine.Random.Range(0, bgms.Count);
            AudioClip clip = bgms[random];
            PlayBgm(clip);
        }
    }

    public void PlayBgm(AudioClip clip)
    {
        if (pauseBgmCoroutine == null)
        {
            // 如果当前是暂停状态, 则直接调用切换
            if (!bgmPlayer.isPlaying)
            {
                BeginBgmTransition(clip);
            }
            
            // 如果没有在转换(在某个状态上)
            else if (playBgmCoroutine == null)
            {
                // 且要播放的clip是其他clip
                if (bgmPlayer.clip != clip)
                {
                    BeginBgmTransition(clip);
                }
                else // bgmPlayer.clip == clip
                {
                    // do nothing
                    return;
                }
            }
            else // 在播放, 且正在转换
            {
                // 如果当前的转换跟, 这次调用的转换一致, 需要一个字段来记录targetClip
                if (clip == targetClip)
                {
                    return;
                }
                else
                {
                    StopCoroutine(playBgmCoroutine);
                    playBgmCoroutine = null;
                    BeginBgmTransition(clip);
                }
            }
        }
        else // 如果当前正在暂停
        {
            // 停止暂停, 调用切换
            StopCoroutine(pauseBgmCoroutine);
            pauseBgmCoroutine = null;
            BeginBgmTransition(clip);
        }
    }
    
    public void PlayBgm(string bgmName)
    {
        if (bgmClipTable.TryGetValue(bgmName, out var clip))
        {
            PlayBgm(clip);
        }
        else
        {
            Debug.Log("bgmClipTable dont have this clip!");
        }
    }

    private void BeginBgmTransition(AudioClip clip)
    {
        playBgmCoroutine = StartCoroutine(PlayBgm(clip, 2, 0.6f));
    }

    private AudioClip targetClip;
    
    private IEnumerator PlayBgm(AudioClip clip, float intervalTime, float volumeWeight = 1f)
    {
        targetClip = clip;
        // 切换clip会导致stop
        // print(bgmPlayer.isPlaying);
        
        // 如果正在播放其他bgm
        float time = 0;
        float startVolume = bgmPlayer.volume;
        if (bgmPlayer.isPlaying && bgmPlayer.clip != clip) // 如果有正在播放的bgm, 就先用一半的时间来暂停播放
        // 并且播放的不是要切换的
        {
            while (time < intervalTime / 2)
            {
                time += Time.deltaTime;
                bgmPlayer.volume = Mathf.Lerp(startVolume, 0, time / (intervalTime / 2));
                print(time);
                yield return null;  
            }
            bgmPlayer.volume = 0;
        }
        
        
        bgmPlayer.clip = clip; 
        bgmPlayer.Play();

        time = 0;
        startVolume = bgmPlayer.volume;
        while (time < intervalTime)
        {
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVolume, volumeWeight, time / intervalTime);
            yield return null;
        }

        bgmPlayer.volume = volumeWeight;
        playBgmCoroutine = null;
        targetClip = null;
    }

    private IEnumerator PauseBgm(float internalTime, AudioClip clipAfterPause = null)
    {
        float time = 0f;
        float startVolume = bgmPlayer.volume;
        while (time < internalTime)
        {
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVolume, 0, time / internalTime);
            yield return null;
        }

        bgmPlayer.volume = 0f;
        bgmPlayer.Pause();
        
        pauseBgmCoroutine = null;
        if (clipAfterPause != null)
        {
            bgmPlayer.clip = clipAfterPause;
            targetClip = null;
        }
    }
    
    // Test sound effect
    // private void Update()
    // {
    //     // if (Input.GetKeyDown(KeyCode.UpArrow))
    //     // {
    //     //     PlaySoundEffect("click_sound");
    //     // }
    // }

    // Test bgm
    // 经测试, 有些情况下还是有bug, 但是目前够用了
    // private void Update()
    // {
    //     // print(playBgmCoroutine != null);
    //     // if (Input.GetKeyDown(KeyCode.UpArrow))
    //     // {
    //     //     // 播放bgm_1
    //     //     PlayBgm("bgm_1");
    //     // }
    //     // else if (Input.GetKeyDown(KeyCode.DownArrow))
    //     // {
    //     //     // 播放bgm_2
    //     //     PlayBgm("bgm_2");
    //     // }
    //     // else if (Input.GetKeyDown(KeyCode.LeftArrow))
    //     // {
    //     //     PlayBgm();
    //     // }
    //     // else if (Input.GetKeyDown(KeyCode.RightArrow))
    //     // {
    //     //     PauseBgm();
    //     // }
    // }
}

// public struct BgmState
// {
//     // pub; // 来自
//     public float weight;
// }
