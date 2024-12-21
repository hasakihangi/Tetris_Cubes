
using UnityEngine;
using UnityEngine.Pool;

// bgm的播放和音效的播放还是有一些不一样的, 音效播放完成后可以立即回收, 但是bgm经常是需要重复播放并且不需要移动位置
// 这里的PooledObject适合于SoundEffect
// Bgm不需要对象池来管理
public class SoundEffect: PoolItem<SoundEffect>
{
    [SerializeField]
    private AudioSource audioSource;
    
    public bool IsPlaybackComplete => !audioSource.isPlaying;

    public void Play(AudioClip clip)
    {
        Play(clip, 1f, Vector3.zero);
    }

    public void Play(AudioClip clip, Vector3 position)
    {
        Play(clip, 1f, position);
    }

    public void Play(AudioClip clip, float volume)
    {
        Play(clip, volume, Vector3.zero);
    }

    public void Play(AudioClip clip, float volume, Vector3 position)
    {
        transform.position = position;
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
    }

    public override void Release()
    {
        pool.Release(this);
    }
}