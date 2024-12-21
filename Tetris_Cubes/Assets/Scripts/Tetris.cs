using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 作为Active Tetris物体上的组件
public class Tetris: MonoBehaviour, IStorable<TetrisStorage>
{
    public Vector2Int currentGridPos;
    public TetrisRotation currentTetrisRotation;
    public TetrisModel model;

    public void Set(TetrisModel _model, Vector2Int _gridPosition, TetrisRotation _rotation)
    {
        model = _model;
        currentGridPos = _gridPosition;
        currentTetrisRotation = _rotation;
    }

    public void Reset()
    {
        model = null;
        currentGridPos = default;
        currentTetrisRotation = default;
    }
    
    // 提供被控制的方法
    public void Rotate(bool right)
    {
        currentTetrisRotation = currentTetrisRotation.TetrisRotate(right);
    }

    public void Move(bool right)
    {
        if (right)
            currentGridPos.x++;
        else
            currentGridPos.x--;
    }

    public List<Vector2Int> GetGridPosList(List<Vector2Int> gridPosList)
    {
        return GetGridPosList(gridPosList, currentGridPos, currentTetrisRotation);
    }

    /// <summary>
    /// 根据传入centerGridPos, 返回每个方块的gridPos.
    /// </summary>
    /// <param name="gridPosList"></param>
    /// <param name="centerGridPos"></param>
    /// <returns></returns>
    public List<Vector2Int> GetGridPosList(List<Vector2Int> gridPosList, Vector2Int centerGridPos)
    {
        return GetGridPosList(gridPosList, centerGridPos, currentTetrisRotation);
    }

    public List<Vector2Int> GetGridPosList(List<Vector2Int> gridPosList, TetrisRotation tetrisRotation)
    {
        return GetGridPosList(gridPosList, currentGridPos, tetrisRotation);
    }

    public List<Vector2Int> GetGridPosList(List<Vector2Int> gridPosList, Vector2Int centerGridPos,
        TetrisRotation tetrisRotation)
    {
        gridPosList.Clear();

        if (model == null)
            return gridPosList;
        
        for (int i = 0; i < model.cubeLocalPosArray.Length; i++)
        {
            Vector2Int gridPos = model.cubeLocalPosArray[i];
            gridPos = gridPos.TetrisRotateTo(tetrisRotation);
            gridPos += centerGridPos;
            gridPosList.Add(gridPos);
        }
        return gridPosList;
    }

    public void Store(TetrisStorage storageData)
    {
        storageData.gridPos = currentGridPos;
        storageData.rotation = (int)currentTetrisRotation;
        if (model != null)
            storageData.model = model.name; // 这里的model可能为null, 如果是null该怎么办呢?
        else
        {
            storageData.model = "null";
        }
    }

    public void Retrieve(TetrisStorage storageData)
    {
        currentGridPos = storageData.gridPos;
        currentTetrisRotation = (TetrisRotation)storageData.rotation;
        if (storageData.model != "null")
            model = DesignerTable.Instance.tetrisModelTable[storageData.model];
        else
        {
            model = null;
        }
    }
}

public class TetrisModel
{
    public Color color;
    public Vector2Int[] cubeLocalPosArray { private set; get; }
    public string name;
    public TetrisModel(Color color, Vector2Int[] cubeLocalPosArray, string name)
    {
        this.color = color;
        this.cubeLocalPosArray = cubeLocalPosArray;
        this.name = name;
    }
}

public enum TetrisRotation
{
    L, F, R, B
}

public static class TetrisRotationExtensions
{
    public static TetrisRotation TetrisRotate(this TetrisRotation rotation, bool right)
    {
        TetrisRotation newRotation;
        if (right)
        {
            newRotation = (TetrisRotation)(((int)rotation + 1) % 4);
        }
        else
        {
            int rotationInteger = (int)rotation - 1;
            if (rotationInteger < 0)
                rotationInteger += 4;
            newRotation = (TetrisRotation)rotationInteger;
        }
        return newRotation;
    }
}

public static class Vector2IntExtensions
{
    public static Vector2Int TetrisRotateTo(this Vector2Int vector2Int, TetrisRotation rotation)
    {
        switch (rotation)
        {
            case TetrisRotation.L:
                return new Vector2Int(-vector2Int.y - 1, vector2Int.x);
            case TetrisRotation.F:
                return vector2Int;
            case TetrisRotation.R:
                return new Vector2Int(vector2Int.y, -vector2Int.x - 1);
            case TetrisRotation.B:
                return new Vector2Int(-vector2Int.x - 1, -vector2Int.y - 1);
        }
        return vector2Int;
    }
}