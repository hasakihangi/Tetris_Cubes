using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class TimelineTest : MonoBehaviour
{
    private TimelineManager timelineManager;

    private void Awake()
    {
        timelineManager = GetComponent<TimelineManager>();
    }

    private float timer = -1f;
    private const float interval = 1;
    private int count = 0;
    
    // 我感觉需要进入函数和退出函数, 不然有些地方不太方便
    // 我感觉需要二次封装成具体的类型

    private TimelineNode node1;
    private TimelineNode node1_1;
    // 每次都要新创建, 不然内部的elapsed变量不会重置
    private TimelineNode node2;
    private TimelineNode node3;

    private Timeline timeline1;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 每个脚本, 只能启用一个Node?
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // noteRate影响了elapsed, 也要影响这里传入的rate, 已经影响了
            node1 = TimelineNode.Get(((elapsed, rate) =>
            {
                if (timer < 0)
                {
                    print("1");
                    print(elapsed);
                    timer = interval;
                    count++;
                    if (count == 5)
                    {
                        return true;
                    }
                }

                timer -= Time.deltaTime * rate;
                return false;
            }));
            timer = -1;
            count = 0;
            Timeline t = Timeline.Get(new TimelineNode[] {node1}, null);
            TimelineManager.Instance.AddTimeline(t);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            node1_1 = TimelineNode.ArrangeNodesInOrder(
                TimelineNode.Get((f, f1) =>
                {
                    print("1");
                    return true;
                }),
                TimelineNode.WaitSeconds(3f),
                TimelineNode.Get((f, f1) =>
                {
                    print("2");
                    return true;
                })
            );
            Timeline t = Timeline.Get(new[] { node1_1 }, null);
            TimelineManager.Instance.AddTimeline(t);
        }
    }
}
