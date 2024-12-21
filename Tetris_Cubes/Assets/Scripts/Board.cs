using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour, IStorable<BoardStorage>
{
    // Inspector Configure
    public SpriteRenderer boardRenderer;
    public Grid grid;
    
    // private
    // 根据NewGame和LoadGame执行不同的初始化逻辑
    public BoardCell[,] cell2DArray; // 存储已经确认的方块, 表示Board的大小
    private Vector2Int tetrisSpawnGridPos;
    // 在cell2DArray初始化完后赋予
    public Vector2Int TetrisSpawnGridPos => tetrisSpawnGridPos;
    
    // 临时存储用于显示Tetris的Cube, 如果.Count==0, 则根据传入的girdPosList分配Cubes
    private List<Cube> tetrisCubeList;

    // 用于消除操作时的中间引用变量, 不需要重置
    // 因为该变量因为场景存在才会使用, 所以初始化放到Awake中会更好
    private List<int> eliminateRows;

    private CubePool cubePool;
    
    private void Awake()
    {
        cubePool = GameManager.Instance.CubePool;
        tetrisCubeList = new List<Cube>();
        eliminateRows = new List<int>();
    }


    private List<Cube> boardDebugCubeList;
    [ContextMenu("Show Board Cubes")]
    public void ShowBoardDebug()
    {
        if (boardDebugCubeList == null)
        {
            boardDebugCubeList = new List<Cube>();
        }
        else
        {
            boardDebugCubeList.Clear();
        }
        
        for (int j = 0; j < cell2DArray.GetLength(1); j++)
        {
            for (int i = 0; i < cell2DArray.GetLength(0); i++)
            {
                if (cell2DArray[i, j].IsOccupied)
                {
                    Cube cube = cubePool.Get();
                    Vector2Int gridPos = new Vector2Int(i, j);
                    Vector2 worldPos = GridPositionToWorldPosition(gridPos);
                    cube.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
                    boardDebugCubeList.Add(cube);
                }
            }
        }
    }

    [ContextMenu("Hide Board Cubes")]
    public void HideBoardDebug()
    {
        if (boardDebugCubeList == null)
            return;
        foreach (var cube in boardDebugCubeList)
        {
            cube.Release();
        }
        boardDebugCubeList.Clear();
    }
    
    public void ReleaseCell2DArray()
    {
        for (int i=0; i<cell2DArray.GetLength(0); i++)
        {
            for (int j = 0; j < cell2DArray.GetLength(1); j++)
            {
                if (cell2DArray[i,j].IsOccupied)
                {
                    cell2DArray[i, j].cube.Release();
                    cell2DArray[i, j].cube = null;
                }
            }
        }

        // cell2DArray = null;
    }

    public void ReleaseTetrisCubeList()
    {
        foreach(var cube in tetrisCubeList)
        {
            cube.Release();
        }
        tetrisCubeList.Clear();
    }

    public void InitCell2DArray(bool isLoading)
    {
        if (!isLoading)
        {
            Bounds bounds = boardRenderer.bounds;
            bounds.Expand(new Vector3(0.1f, 0.1f, 0f));
            float unitXLength = grid.cellSize.x;
            float unitYLength = grid.cellSize.y;
            int gridWidth = Mathf.FloorToInt(bounds.size.x / unitXLength);
            int gridHeight = Mathf.FloorToInt(bounds.size.y / unitYLength);
            cell2DArray = new BoardCell[gridWidth, gridHeight + 1];
        }
        else
        {
            Retrieve(StorageManager.Instance.boardLoadData);
        }
        
        tetrisSpawnGridPos = new Vector2Int(cell2DArray.GetLength(0) / 2, cell2DArray.GetLength(1) - 2);
    }

    // 只有在产生Tetris的时候需要设置颜色
    // 如果为空就先生成, 如果有就只设置位置
    public void UpdateTetrisCubes(List<Vector2Int> gridPosList, Color color = default)
    {
        Cube cube;
        Vector2 pos;
        if (tetrisCubeList.Count == 0)
        {   
            for (int i = 0; i < gridPosList.Count; i++)
            {
                cube = cubePool.pool.Get();
                tetrisCubeList.Add(cube);
                pos = GridPositionToWorldPosition(gridPosList[i]);
                cube.transform.position = new Vector3(pos.x, pos.y, 0f);
                cube.SetColor(color);
            }
        }
        else
        {
            for (int i = 0; i < gridPosList.Count; i++)
            {
                cube = tetrisCubeList[i];
                pos = GridPositionToWorldPosition(gridPosList[i]);
                cube.transform.position = new Vector3(pos.x, pos.y, 0f);
            }
        }
    }
    
    

    public void UpdateBoardCubes()
    {
        BoardCell cell;
        Vector2 pos;
        for (int i=0; i<cell2DArray.GetLength(0); i++)
        {
            for (int j=0; j<cell2DArray.GetLength(1); j++)
            {
                cell = cell2DArray[i, j];
                if (cell.IsOccupied)
                {
                    pos = GridPositionToWorldPosition(new Vector2Int(i, j));
                    cell.cube.transform.position = new Vector3(pos.x, pos.y, 0f);
                }
            }
        }
    }

    // 需要从Tetris中获取到Grid信息
    public void TetrisCubesToBoard(List<Vector2Int> gridPosList)
    {
        Vector2Int gridPos;
        Cube cube;
        for (int i=0; i<gridPosList.Count; i++)
        {
            gridPos = gridPosList[i];

            if (gridPos.y >= cell2DArray.GetLength(1))
                continue;

            cube = tetrisCubeList[i];
            cell2DArray[gridPos.x, gridPos.y].cube = cube;
        }
        tetrisCubeList.Clear();
    }

    // 将Vector2Int的坐标值, 换算成Cube应放置在的世界坐标的中心点
    public Vector2 GridPositionToWorldPosition(Vector2Int gridPos)
    {
        Vector2 worldPos = grid.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
        return worldPos + new Vector2(grid.cellSize.x / 2, grid.cellSize.y / 2);
    }

    public bool CheckTetrisValid(List<Vector2Int> gridPosList)
    {
        if (gridPosList == null)
            return false;
        
        if (gridPosList.Count == 0)
            return false;

        Vector2Int gridPos;
        for(int i=0; i<gridPosList.Count; i++)
        {
            gridPos = gridPosList[i];
            // 如果有方块超出了左右边界和下边界
            if (gridPos.x < 0 || gridPos.x >= cell2DArray.GetLength(0) ||
                gridPos.y < 0 )
            {
                return false; 
            }

            // 如果超出了上边界
            if (gridPos.y >= cell2DArray.GetLength(1))
            {
                continue;
            }

            //print($"boardArray.width:{boardUnitArray.GetLength(0)}");
            //print($"boardArray.height:{boardUnitArray.GetLength(1)}");
            //print($"gridPos.x:{gridPos.x}");
            //print($"gridPos.y:{gridPos.y}");

            // 检查是否与已有的内容重合
            if (cell2DArray[gridPos.x, gridPos.y].IsOccupied)
            {
                return false; // 被占用
            }
        }
        return true; // 所有检查通过
    }

    

    // 返回:
    // bool: 是否需要消除
    // 特点: 需要消除几行往上才需要移动, 消除几行就往下移动几行
    // 最好在两个for循环做完所有工作
    public bool EliminateRows(List<Cube> eliminateCubes, List<MoveCube> moveCubes)
    {
        eliminateRows.Clear();
        eliminateCubes.Clear();
        moveCubes.Clear();
        
        for (int j=0; j<cell2DArray.GetLength(1); j++)
        {
            bool rowAllOccupied = true;
            for (int i=0; i<cell2DArray.GetLength(0); i++)
            {
                // 如果为一行中有空, 则表示该行不满, 进入下一行的循环
                if (cell2DArray[i, j].IsEmpty)
                {
                    rowAllOccupied = false;
                    break;
                }
            }
            if (rowAllOccupied)
            {
                eliminateRows.Add(j);
            }
        }

        if (eliminateRows.Count == 0)
        {
            return false;
            // 也同时返回了空的eliminateCubes和moveCubes
        }
        else
        {
            Cube cube;

            // 填充eliminateCubes
            foreach (int row in eliminateRows)
            {
                for (int i=0; i<cell2DArray.GetLength(0); i++)
                {
                    cube = cell2DArray[i, row].cube;
                    if (cube is not null)
                    {
                        // 逻辑上的消除, 消除后才能移动
                        cell2DArray[i, row].cube = null;
                        eliminateCubes.Add(cube);
                    }
                }
            }

            int moveCount = eliminateRows.Count; // 需要移动的行数, 这个等于8是怎么回事?
            Vector2Int gridFrom;
            Vector2Int gridTo;
            int checkMoveRowStart = eliminateRows[eliminateRows.Count - 1]; // 需要消除的最上一行
            checkMoveRowStart++;

            for (int j=checkMoveRowStart; j<cell2DArray.GetLength(1); j++)
            {
                for (int i=0; i<cell2DArray.GetLength(0); i++)
                {
                    cube = cell2DArray[i, j].cube;
                    if (cube is not null)
                    {
                        gridFrom = new Vector2Int(i, j);
                        gridTo = gridFrom + new Vector2Int(0, -moveCount);

                        // 逻辑上的移动
                        cell2DArray[gridFrom.x, gridFrom.y].cube = null;
                        cell2DArray[gridTo.x, gridTo.y].cube = cube;

                        MoveCube moveCube;
                        moveCube.cube = cube;
                        moveCube.from = GridPositionToWorldPosition(gridFrom);
                        moveCube.to = GridPositionToWorldPosition(gridTo);
                        moveCubes.Add(moveCube);
                    }
                }
            }
            return true;
        }    
    }

    public struct MoveCube
    {
        public Vector2 from;
        public Vector2 to;
        public Cube cube;
    }
    
    public void Clear()
    {
        ReleaseCell2DArray();
        ReleaseTetrisCubeList();
    }

    public void Store(BoardStorage storageData)
    {
        storageData.width = cell2DArray.GetLength(0);
        storageData.height = cell2DArray.GetLength(1);
        storageData.cellStorage2DArray = new BoardStorage.CellStorage[storageData.width, storageData.height];
        for (int j = 0; j < cell2DArray.GetLength(1); j++)
        {
            for (int i = 0; i < cell2DArray.GetLength(0); i++)
            {
                if (cell2DArray[i, j].IsOccupied)
                {
                    storageData.cellStorage2DArray[i, j].isOccupied = true;
                    storageData.cellStorage2DArray[i, j].color = cell2DArray[i, j].cube.spriteRenderer.color;
                }
            }
        }
    }
    
    // 只是填充数据, 需要顺便将位置设定好吗? 不需要, 通过UpdateCubes方法设置位置
    public void Retrieve(BoardStorage storageData)
    {
        int width = storageData.width;
        int height = storageData.height;
        cell2DArray = new BoardCell[width, height];
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (storageData.cellStorage2DArray[i, j].isOccupied)
                {
                    Cube cube = cubePool.pool.Get();
                    cube.SetColor(storageData.cellStorage2DArray[i, j].color);
                    cell2DArray[i, j].cube = cube;
                }
            }
        }
        tetrisSpawnGridPos = new Vector2Int(cell2DArray.GetLength(0) / 2, cell2DArray.GetLength(1) - 2);
    }
}

public struct BoardCell
{
    public Cube cube; // null表示没有占用, 用is比较更加高效
    public bool IsOccupied => cube is not null;
    public bool IsEmpty => cube is null;
    // 需要反向索引到Board中的二维数组吗?
}
