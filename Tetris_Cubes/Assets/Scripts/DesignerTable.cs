using System;
using System.Collections.Generic;
using UnityEngine;

// 被GameManager引用
public class DesignerTable: MonoBehaviour
{
    public Dictionary<string, TetrisModel> tetrisModelTable;
    
    // 如果有一些管理器的初始化需要DesignerTable, 那么需要在GameManager中安排初始化顺序
    // 如果十分常用才需要设置静态字段, 都可以在GameManager中获取到
    public static DesignerTable Instance { get; private set; }

    private void Awake()
    {
        Instance = this; // 在初始化时不能使用, 因为这里的Awake可能在其他之后发生
    }

    public void Init()
    {
        tetrisModelTable = new Dictionary<string, TetrisModel>();
        // Desiner Table
        TetrisModel L = new TetrisModel(new Color(0.8f, 0.3f, 0.3f), new 
            Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1)
        }, "L");
        tetrisModelTable.Add(L.name, L);
        
        TetrisModel NL = new TetrisModel(new Color(0.5f, 0.2f, 0.5f), new 
            Vector2Int[]
            {
                new Vector2Int(-1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1)
            }, "NL");
        tetrisModelTable.Add(NL.name, NL);

        TetrisModel Z = new TetrisModel(new Color(0.2f, 0.3f, 0.8f), new 
            Vector2Int[]
        {
            new Vector2Int(0,1),
            new Vector2Int(0,0),
            new Vector2Int(-1,0),
            new Vector2Int(-1,-1)
        }, "Z");
        tetrisModelTable.Add(Z.name, Z);
        
        TetrisModel NZ = new TetrisModel(new Color(0.2f, 0.5f, 0.6f), new 
            Vector2Int[]
            {
                new Vector2Int(-1,-1),
                new Vector2Int(-1,0),
                new Vector2Int(0,0),
                new Vector2Int(0,-1)
            }, "NZ");
        tetrisModelTable.Add(NZ.name, NZ);
        
        TetrisModel O = new TetrisModel(new Color(0.1f, 0.5f, 0.3f), new 
            Vector2Int[]
        {
            new Vector2Int(0,0),
            new Vector2Int(0,-1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1,-1)
        }, "O");
        tetrisModelTable.Add(O.name, O);
        
        TetrisModel I = new TetrisModel(new Color(0.8f, 0.6f, 0.2f), new 
            Vector2Int[]
        {
            new Vector2Int(0,1),
            new Vector2Int(0,0),
            new Vector2Int(0,-1),
            new Vector2Int(0,-2)
        }, "I");
        tetrisModelTable.Add(I.name, I);
    }
}