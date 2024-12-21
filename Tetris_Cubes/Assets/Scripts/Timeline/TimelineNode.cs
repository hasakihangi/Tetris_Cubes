
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TimelineNode
{
    public Func<float, float, bool> method;
    public float elapsed = 0f;
    // 既然要回收这里next也是要重复利用
    // next这里使用后清空
    public List<TimelineNode> next = new List<TimelineNode>();
    public float nodeRate = 1f;

    // 提供Rest方法, 在回收时调用
    private void Reset()
    {
        method = null;
        elapsed = 0f;
        // next.Clear();
        nodeRate = 1f;
    }
    
    // 提供于构造函数等同的Set方法, 从Pool中取出时调用
    private void Set(Func<float, float, bool> _method)
    {
        this.method = _method;
    }

    private TimelineNode()
    {
        
    }
    
    private TimelineNode(Func<float, float, bool> method)
    {
        this.method = method;
    }

    // 这里的rate是timeline传过来的rate, 还有一个自己的rate
    public bool Update(float delta, float rate)
    {
        bool done = method == null || method(elapsed, rate * nodeRate);
        elapsed += delta * rate * nodeRate;
        return done;
    }

    public void AddToNext(TimelineNode node)
    {
        if (node == null)
            return;
        next.Add(node);
    }

    public void AddToNext(List<TimelineNode> nodes)
    {
        if (nodes == null || nodes.Count <= 0)
            return;
        // 如果list中节点不为null则加入到next, 使用AddToNext
        foreach (var node in nodes)
        {
            if (node != null)
            {
                AddToNext(node);
            }
        }
    }

    public void AddNodesToNextInOrder(List<TimelineNode> nodes, out TimelineNode finalNode)
    {
        int index = 0;
        finalNode = this;
        while (index < nodes.Count)
        {
            finalNode.AddToNext(nodes[index]);
            finalNode = nodes[index];
            index++;
        }
    }
    
    public static TimelineNode Nothing => Get((elapsed, rate) => true);

    public static TimelineNode WaitSeconds(float sec) =>Get((elapsed, rate) => elapsed >= sec);

    public static TimelineNode Action(Action<float> action) => Get(
        (elapsed, rate) =>
        {
            action?.Invoke(rate);
            return true;
        });

    // 链式串联TimelineNode, 返回首个不为null的TimelineNode
    public static TimelineNode ArrangeNodesInOrder(params TimelineNode[] nodes)
    {
        if (nodes.Length <= 0) return Nothing;
        TimelineNode first = null; // 首个不为null的TimelineNode
        TimelineNode node = null;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == null) continue;
            if (first == null)
            {
                first = nodes[i];
                node = first;
            }
            else
            {
                node.AddToNext(nodes[i]);
                node = nodes[i];
            }
        }
        return first;
    }

    public void Release()
    {
        if (next.Count != 0)
        {
            foreach (var node in next)
            {
                node.Release();
            }
        }
        
        next.Clear();
        
        pool.Release(this);
    }
    
    // 静态字段的Pool?
    private static ObjectPool<TimelineNode> pool = new ObjectPool<TimelineNode>(CreatePoolItem, null, OnReturnToPool, null);

    private static TimelineNode CreatePoolItem()
    {
        TimelineNode node = new TimelineNode();
        Debug.Log($"TimelineNodePool.CountAll: {pool.CountAll}");
        return node;
    }

    private static void OnReturnToPool(TimelineNode node)
    {
        node.Reset();
    }

    public static TimelineNode Get(Func<float, float, bool> method)
    {
        TimelineNode node = pool.Get();
        node.Set(method);
        return node;
    }
}