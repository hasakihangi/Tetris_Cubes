using System;
using System.Reflection;
using UnityEngine;

// 似乎可以用反射的方式读取成员变量类型, 自动进行Save和Load, 仅需提供Storage类
// 具体的类应该是一个子类, Save和Load方法在父类中
public class Storage : IPersistable
{
    public void Save(GameDataWriter gameDataWriter)
    {
        // 获取当前类的所有字段（包括子类的字段）
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // 根据字段的类型进行处理
            if (field.FieldType == typeof(int))
            {
                gameDataWriter.Write((int)field.GetValue(this));  // 保存 int 类型字段
            }
            else if (field.FieldType == typeof(float))
            {
                gameDataWriter.Write((float)field.GetValue(this));  // 保存 float 类型字段
            }
            else if (field.FieldType == typeof(string))
            {
                gameDataWriter.Write((string)field.GetValue(this));  // 保存 string 类型字段
            }
            else if (field.FieldType == typeof(bool))
            {
                gameDataWriter.Write((bool)field.GetValue(this));  // 保存 bool 类型字段
            }
            else if (field.FieldType == typeof(Color))
            {
                gameDataWriter.Write((Color)field.GetValue(this));
            }
            else if (field.FieldType == typeof(Vector2Int))
            {
                gameDataWriter.Write((Vector2Int)field.GetValue(this));
            }
            else
            {
                throw new NotImplementedException($"Save for type {field.FieldType} not implemented");
            }
        }
    }

    // 使用反射自动加载字段
    public void Load(GameDataReader gameDataReader)
    {
        // 获取当前类的所有字段（包括子类的字段）
        FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // 根据字段的类型进行处理
            if (field.FieldType == typeof(int))
            {
                field.SetValue(this, gameDataReader.ReadInt());  // 读取并赋值给 int 类型字段
            }
            else if (field.FieldType == typeof(float))
            {
                field.SetValue(this, gameDataReader.ReadFloat());  // 读取并赋值给 float 类型字段
            }
            else if (field.FieldType == typeof(string))
            {
                field.SetValue(this, gameDataReader.ReadString());  // 读取并赋值给 string 类型字段
            }
            else if (field.FieldType == typeof(bool))
            {
                field.SetValue(this, gameDataReader.ReadBool());  // 读取并赋值给 bool 类型字段
            }
            else if (field.FieldType == typeof(Color))
            {
                field.SetValue(this, gameDataReader.ReadColor());
            }
            else if (field.FieldType == typeof(Vector2Int))
            {
                field.SetValue(this, gameDataReader.ReadVector2Int());
            }
            else
            {
                throw new NotImplementedException($"Load for type {field.FieldType} not implemented");
            }
        }
    }
}

public class CoreStorage : Storage
{
    public int tetrisMoveTimer = -1;
    public int turnTimer = 30;
    public int downTimer = -1;
    public int turn = (int)CoreManager.Turn.PlayerControl;
}

// public class CoreStorage: IPersistable
// {
//     public int tetrisMoveTimer = -1;
//     public int turnTimer = 30;
//     public int downTimer = -1;
//     public int turn = (int)CoreManager.Turn.PlayerControl;
//     public void Save(GameDataWriter gameDataWriter)
//     {
//         gameDataWriter.Write(tetrisMoveTimer);
//         gameDataWriter.Write(turnTimer);
//         gameDataWriter.Write(downTimer);
//         gameDataWriter.Write(turn);
//     }
//
//     public void Load(GameDataReader gameDataReader)
//     {
//         tetrisMoveTimer = gameDataReader.ReadInt();
//         turnTimer = gameDataReader.ReadInt();
//         downTimer = gameDataReader.ReadInt();
//         turn = gameDataReader.ReadInt();
//     }
// }

// 但是这个不行, 需要手动实现
public class BoardStorage: IPersistable
{
    public struct CellStorage
    { 
        public bool isOccupied;
        public Color color;
    } 
    
    // 几x几的Board, 每个格子上是否占有, 以及它是什么颜色
    public int width;
    public int height;
    public CellStorage[,] cellStorage2DArray;
    
    public void Save(GameDataWriter gameDataWriter)
    {
        gameDataWriter.Write(width);
        gameDataWriter.Write(height);
        for (int j = 0; j < cellStorage2DArray.GetLength(1); j++)
        {
            for (int i = 0; i < cellStorage2DArray.GetLength(0); i++)
            {
                gameDataWriter.Write(cellStorage2DArray[i,j].isOccupied);
                gameDataWriter.Write(cellStorage2DArray[i,j].color);
            }
        }
    }

    public void Load(GameDataReader gameDataReader)
    {
        width = gameDataReader.ReadInt();
        height = gameDataReader.ReadInt();
        cellStorage2DArray = new CellStorage[width, height];
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                cellStorage2DArray[i, j].isOccupied = gameDataReader.ReadBool();
                cellStorage2DArray[i, j].color = gameDataReader.ReadColor();
            }
        }
    }
}

public class TetrisStorage: Storage
{
    public Vector2Int gridPos;
    public int rotation;
    public string model;
}