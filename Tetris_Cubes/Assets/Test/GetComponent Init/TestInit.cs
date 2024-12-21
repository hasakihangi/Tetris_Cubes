using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class TestInit: MonoBehaviour
    {
        // 脚本调用顺序是以一种奇怪的方式存储在某处
        // 同一物体上的脚本调用顺序是固定的, 跟脚本的上下位置无关, 跟物体的上下位置也无关
        // 所以控制初始化顺序只有通过Init进行调用?
        private void Awake()
        {
            print("T");
            GetComponent<Init1>(); // 看来GetComponent不会改变Awake的调用顺序
            GetComponent<Init2>();
        }
    }
}