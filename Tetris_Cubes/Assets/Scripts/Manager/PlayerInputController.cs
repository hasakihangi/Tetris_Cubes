using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class PlayerInputController: MonoBehaviour
{
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    //public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode leftRotation = KeyCode.Q;
    public KeyCode rightRotation = KeyCode.E;
    public KeyCode drop = KeyCode.Space;

    public Dictionary<KeyCode, Action> keyEvents = new Dictionary<KeyCode, Action>();

    public static PlayerInputController Instance { get; private set; }

    public void Init()
    {
        keyEvents.Add(KeyCode.A, null);
        keyEvents.Add(KeyCode.D, null);
        keyEvents.Add(KeyCode.W, null);
        keyEvents.Add(KeyCode.S, null);
        keyEvents.Add(KeyCode.Space, null);
        keyEvents.Add(KeyCode.Escape, null);
    }
    
    private void Awake()
    {
        Instance = this;
    }

    // 每个按钮有一个事件, 用Dictionary存储
    public void CheckInput(InputInfo inputInfo)
    {
        if (Input.GetKeyDown(leftRotation))
            inputInfo.leftRotation = true;

        if (Input.GetKeyDown(rightRotation))
            inputInfo.rightRotation = true;

        // 如果是GetKey需要中间的间隔帧数
        // 只有GetKeyDown适合事件系统
        if (Input.GetKey(left))
        {
            inputInfo.left = true;
        }

        if (Input.GetKey(right))
        {
            inputInfo.right = true;
        }

        if (Input.GetKey(down))
        {
            inputInfo.down = true;
        }

        if (Input.GetKeyDown(drop))
        {
            inputInfo.drop = true;
        }
    }

    public void SendInputEvents()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            keyEvents[KeyCode.A]?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            keyEvents[KeyCode.D]?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            keyEvents[KeyCode.W]?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            keyEvents[KeyCode.S]?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            keyEvents[KeyCode.Space]?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            keyEvents[KeyCode.Escape]?.Invoke();
        }
    }


    public void RegisterKeyEvent(KeyCode key, Action action)
    {
        if (keyEvents.ContainsKey(key))
        {
            if (keyEvents[key] == null)
            {
                keyEvents[key] = action;
            }
            else
            {
                keyEvents[key] += action;
            }
        }
    }

    public void UnRegisterKeyEvent(KeyCode key, Action action)
    {
        if (keyEvents.ContainsKey(key))
        {
            keyEvents[key] -= action;
        }
    }
}

// 旋转操作没有必要定间隔帧, 点一下旋转一次就行了

// 没有必要反复创建, 使用Class还是ref?
public class InputInfo
{
    public bool left;
    public bool right;
    public bool down;
    public bool leftRotation;
    public bool rightRotation;
    public bool drop;

    public void Reset()
    {
        left = right = down = leftRotation = rightRotation = drop = false;
    }
}