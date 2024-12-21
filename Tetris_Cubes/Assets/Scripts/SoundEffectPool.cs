using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class SoundEffectPool : Pool<SoundEffect>
{
    private LinkedList<SoundEffect> soundEffectNodes = new LinkedList<SoundEffect>();

    [SerializeField] private float checkPlaybackIntervalTime = 5f;
    private float checkPlaybackTimer;

    private void Start()
    {
        checkPlaybackTimer = checkPlaybackIntervalTime;
    }

    private void Update()
    {
        checkPlaybackTimer -= Time.deltaTime;
        if (checkPlaybackTimer < 0f)
        {
            // 遍历values? 需要高效地动态添加和移除, 使用LinkedList, 如果没有查找需求就不需要id
            // foreach (var soundEffect in soundEffects)
            // {
            //     if (soundEffect.IsPlaybackComplete)
            //     {
            //         soundEffect.Release();
            //         // 链表可以安全地移除, 好像不可以安全移除
            //         soundEffects.Remove(soundEffect);
            //     }
            // }

            var node = soundEffectNodes.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.IsPlaybackComplete)
                {
                    node.Value.Release();
                    soundEffectNodes.Remove(node);
                }

                node = next;
            }
            checkPlaybackTimer = checkPlaybackIntervalTime;
        }
    }

    public override SoundEffect Get()
    {
        SoundEffect soundEffect = pool.Get(); 
        // 如果不需要在外边动态移除那就不需要一个id字段
        soundEffectNodes.AddLast(soundEffect);
        return soundEffect;
    }
}
