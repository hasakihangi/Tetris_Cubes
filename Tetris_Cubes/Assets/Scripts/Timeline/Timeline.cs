using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Timeline
{
    public int id; // 为了配合Dictionary, 可以通过timeline实例找到再字典中的位置
    public LinkedList<TimelineNode> orderNodes = new LinkedList<TimelineNode>(); // 判断null或.First == null
    // 只用于后续动态添加, 初始化时将一系列List<Node>直接加入runningNodes
    // 这里每一阶段应该都是List<Node>, 不应该是只能执行一个
    // List<List<TimelineNode>> ? 这样又显得比较繁琐, 一个内部类, 可以是单个Node, 也可以是List<Node> ?
    // 用Timeline.Next的方式串联
    
    
    public Timeline next;
    public Action OnDone; // 一般都为null, 不需要在构造函数中初始化
    public float timelineRate = 1; // 不需要在构造函数中设置

    private LinkedList<TimelineNode> runningNodes = new LinkedList<TimelineNode>();
    // 应该是个链表

    private void ReSet()
    {
        // orderNodes.Clear(); // Release中处理了
        // next = null; // 这个也要处理
        OnDone = null;
        timelineRate = 1f;
        // runningNodes.Clear();
    }
    
    public void Set(TimelineNode[] _runningNodes, TimelineNode[] _orderNodes)
    {
        if (_runningNodes != null)
        {
            foreach (var node in _runningNodes)
            {
                this.runningNodes.AddLast(node);
            }
        }

        if (_orderNodes != null)
        {
            foreach (var node in _orderNodes)
            {
                this.orderNodes.AddLast(node);
            }
        }
    }

    private Timeline()
    {
        
    }
    
    // 暂时不考虑null的情况
    // private Timeline(TimelineNode[] runningNodes, TimelineNode[] orderNodes)
    // {
    //     if (runningNodes != null)
    //     {
    //         foreach (var node in runningNodes)
    //         {
    //             this.runningNodes.AddLast(node);
    //         }
    //     }
    //
    //     if (orderNodes != null)
    //     {
    //         foreach (var node in orderNodes)
    //         {
    //             this.orderNodes.AddLast(node);
    //         }
    //     }
    // }

    private void Set(Func<float, float, bool> method)
    {
        TimelineNode node = TimelineNode.Get(method);
        runningNodes.AddLast(node);
    }
    
    // private Timeline(Func<float, float, bool> method)
    // {
    //     TimelineNode node = TimelineNode.Get(method);
    //     runningNodes.AddLast(node);
    // }

    private void Set(params Func<float, float, bool>[] methods)
    {
        if (methods != null)
        {
            foreach (var method in methods)
            {
                TimelineNode node = TimelineNode.Get(method);
                runningNodes.AddLast(node);
            }
        }
    }
    
    // 这里的rate时Manager传过来的rate
    public bool Update(float delta, float rate)
    {
        // 往下传时, 记得rate*timelineRate
        // 如果runningNode有内容, 则遍历runningNode
        LinkedListNode<TimelineNode> linkedNode = runningNodes.First;
        while (linkedNode != null)
        {
            // Update Note
            // 如果Node执行完毕就移除
            if (linkedNode.Value.Update(delta, rate * timelineRate))
            {
                runningNodes.Remove(linkedNode);
                
                // 是否有next?
                List<TimelineNode> nodes = linkedNode.Value.next;
                if (nodes.Count != 0)
                {
                    foreach (var node in nodes)
                    {
                        runningNodes.AddLast(node);
                    }

                    nodes.Clear();
                }
                
                linkedNode.Value.Release();
            }

            linkedNode = linkedNode.Next;
        }

        if (runningNodes.First == null) // 说明执行完了
        {
            OrderNodeToRunningNodes();
            return (runningNodes.First == null);
        }

        return false;
    }

    private void OrderNodeToRunningNodes()
    {
        // 将OrderNode加入后, 就从OrderNodes中移除
        if (orderNodes.First != null) // 还要判断oderNodes.First.Value不为null ?
        {
            // 将First移入runningNodes, 并从orderNodes中移除
            runningNodes.AddLast(orderNodes.First.Value);
            orderNodes.RemoveFirst();
        }
    }

    public static Timeline ArrangeTimelinesInOrder(params Timeline[] timelines)
    {
        if (timelines.Length == 0) return Timeline.Get((elapsed, rate) => true);
        int index = 0;
        while (index < timelines.Length - 1)
        {
            timelines[index].next = timelines[index + 1];
            index++;
        }

        return timelines[0];
    }

    public void Release()
    {
        // 如果Release时runningNodes和orderNodes有内容
        foreach (var node in runningNodes)
        {
            node.Release();
        }
        runningNodes.Clear();

        foreach (var node in orderNodes)
        {
            node.Release();
        }
        orderNodes.Clear();

        if (next != null)
        {
            next.Release();
        }
        next = null;
        
        pool.Release(this);
    }
    
    private static ObjectPool<Timeline> pool = new ObjectPool<Timeline>(CreatePoolItem, OnTakeFromPool, OnReturnToPool);

    private static int currentId = 0; // 禁止访问

    private static int CurrentId
    {
        get
        {
            return currentId++;
        }
    }
    
    private static Timeline CreatePoolItem()
    {
        Timeline timeline = new Timeline();
        Debug.Log($"TimelinePool.CountAll: {pool.CountAll}");
        return timeline;
    }

    private static void OnTakeFromPool(Timeline timeline)
    {
        timeline.id = CurrentId;
    }

    private static void OnReturnToPool(Timeline node)
    {
        node.ReSet();
    }

    public static Timeline Get(TimelineNode[] _runningNodes, TimelineNode[] _orderNodes)
    {
        Timeline timeline = pool.Get();
        timeline.Set(_runningNodes, _orderNodes);
        return timeline;
    }

    public static Timeline Get(Func<float, float, bool> method)
    {
        Timeline timeline = pool.Get();
        timeline.Set(method);
        return timeline;
    }

    public static Timeline Get(params Func<float, float, bool>[] methods)
    {
        Timeline timeline = pool.Get();
        timeline.Set(methods);
        return timeline;
    }
}