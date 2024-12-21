
// node播放完成后自己就移出了执行列表, 执行列表和时间轴上没有节点, 则表明timeline执行完毕
// Timeline(时间)包含TimelineNode(操作), 如果有next, 则按照当前时间点加入到Timeline中? 或者直接执行, 执行的node是一个列表, 应该是加入这个列表中
// 一些node安排在时间轴上, 等到特定时间节点加入到执行列表, 如果执行列表为空
using System;
using System.Collections.Generic;
using UnityEngine;

public class TimelineManager: MonoBehaviour
{
    // 正在运行的timelines
    // private readonly List<Timeline> timelines = new List<Timeline>();
    private readonly Dictionary<int, Timeline> runningTimelines = new Dictionary<int, Timeline>();
    // 要频繁的插入删除, 也有查找需求, 似乎用Dictionary更好
    // 但是dic不太适合遍历? 遍历都是O(n)
    // 考虑linkedList
    
    // public 
    // 只会有少量的Timeline会执行, 其他的一些Timeline只做为对List<TimelineNode>存储容器

    private List<int> removeList = new List<int>();
    private List<Timeline> addList = new List<Timeline>();
    // private int currentId = 0; // 拒绝访问
    //
    // public int CurrentId
    // {
    //     get
    //     {
    //         return currentId++;
    //     }
    // }
    
    public static TimelineManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateTimelines(Time.deltaTime);
    }

    public void UpdateTimelines(float delta, float rate = 1f)
    {
        foreach (var timeline in addList)
        {
            AddToRunning(timeline);
        }
        
        addList.Clear();
        
        foreach (var id in removeList)
        {
            RemoveFromRunning(id);
        }

        removeList.Clear();

        foreach (var timeline in runningTimelines.Values)
        {
            if (timeline.Update(delta, rate))
            {
                timeline.OnDone?.Invoke();
                removeList.Add(timeline.id);
                if (timeline.next != null)
                {
                    addList.Add(timeline.next);
                    timeline.next = null;
                }
                timeline.Release();
            }
        }
    }
    
    // rate的处理
    // 只想某一个node加速? 在创建时设置该node的rate值, 传下来时多个node相乘
    // 只想某一个timeline加速, 也是一样的处理方法
    // 整体加速, TimelineManager中的Update方法
    // 在运行时动态加速? 实时修改timeline的rate值
    
    private void AddToRunning(Timeline timeline)
    {
        if (!runningTimelines.ContainsKey(timeline.id))
        {
            runningTimelines.Add(timeline.id, timeline);
        }
        else
        {
            Debug.Log($"AddToRunning Err! Timeline.ID = {timeline.id}");
        }
    }

    private void RemoveFromRunning(int id)
    {
        if (runningTimelines.ContainsKey(id))
        {
            runningTimelines.Remove(id);
        }
        else
        {
            Debug.Log("RemoveFromRunning Err!");
        }
    }

    // 如果主动移除timeline, 里面的timelineNode怎么比较好的进行释放?
    public void RemoveTimeline(int id)
    {
        if (runningTimelines.TryGetValue(id, out var timeline))
        {
            // timeline.OnDone?.Invoke(); // 目前觉得是, 只有正常移除时才调用
            removeList.Add(id);
        }
        else
        {
            Debug.Log("StopTimeline Err!");
        }
    }

    public Timeline AddTimeline(Timeline timeline)
    {
        addList.Add(timeline);
        return timeline;
    }
    
}

