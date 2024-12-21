using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// 如果是以场景附加的形式, InputControl在Game场景中, 那编辑Core场景时, 总是要附加Game场景
public class CoreManager : MonoBehaviour, IStorable<CoreStorage>
{
    // Instance
    private static CoreManager instance;
    public static CoreManager Instance => instance;
    
    // Inspector Configure
    // 因为是Mono, 所以不需要手动初始化
    public Board board;
    public Tetris tetris;
    [FormerlySerializedAs("moveInternalFrame")] public int moveIntervalFrame = 8;
    [FormerlySerializedAs("turnInternalFrame")] public int turnIntervalFrame = 30;
    [FormerlySerializedAs("downInternalFrame")] public int downIntervalFrame = 10;
    
    // private
    // start初始化
    private TetrisModel[] tetrisModels;
    private PlayerInputController inputController;
    // 运行时私有字段, 游戏重置时重置
    private InputInfo inputInfo = new InputInfo();
    private List<Vector2Int> tetrisGridPosList = new List<Vector2Int>();
    private List<Cube> eliminateCubes = new List<Cube>();
    private List<Board.MoveCube> moveCubes = new List<Board.MoveCube>();
    // 需要进行初始化调用, 游戏重新开始和加载初始化的内容不一样
    private int tetrisMoveTimer;
    private int turnTimer;
    private int downTimer; // 控制下落的倒计时帧数
    private Turn turn = Turn.TetrisSpawn;

    private void ResetFields(int _tetrisMoveTimer, int _turnTimer, int _downTimer, Turn _turn)
    {
        inputInfo.Reset();
        tetrisGridPosList.Clear();
        eliminateCubes.Clear();
        moveCubes.Clear();
        tetrisMoveTimer = _tetrisMoveTimer;
        turnTimer = _turnTimer;
        downTimer = _downTimer;
        turn = _turn;
    }

    private void ResetFields()
    {
        ResetFields(-1, turnIntervalFrame, -1, Turn.TetrisSpawn);
    }
    
    private void Awake()
    {
        instance = this;
        var values = DesignerTable.Instance.tetrisModelTable.Values;
        tetrisModels = values.ToArray();
        inputController = PlayerInputController.Instance;
    }

    private bool IsFirstInScene
    {
        get
        {
            return board.cell2DArray == null;
        }
    }
    
    // 开始并初始化游戏, 或者根据存储构建游戏
    // SettingsPanel中StartNewGame所使用的方法
    public void BuildNewGame()
    {
        if (IsFirstInScene)
        {
            board.InitCell2DArray(false);
        }
        else
        {
            tetris.Reset();
            board.Clear();
            ResetFields();
        }
        // 默认enabled=false
        enabled = true;
    }
    
    // 与BuildNewGame相对应
    // SettingPanel中有两个按钮, LoadAtStart和LoadAtPause, 这里是其他部分加载完成后(特别是场景), 还原Board, Core, Tetris
    public void LoadGame()
    {
        if (IsFirstInScene)
        {
            // 直接根据StorageManager中的data还原就可以
            Retrieve(StorageManager.Instance.coreLoadData);
            board.Retrieve(StorageManager.Instance.boardLoadData);
            tetris.Retrieve(StorageManager.Instance.tetrisLoadData);
        }
        else
        {
            // 先清理再还原
            tetris.Reset();
            board.Clear();
            ResetFields();
            Retrieve(StorageManager.Instance.coreLoadData);
            board.Retrieve(StorageManager.Instance.boardLoadData);
            tetris.Retrieve(StorageManager.Instance.tetrisLoadData);
        }
        
        // 还原后需要更新游戏
        board.UpdateBoardCubes();
        if (tetris.model != null)
            board.UpdateTetrisCubes(tetris.GetGridPosList(tetrisGridPosList), tetris.model.color);
        else
        {
            board.UpdateTetrisCubes(tetris.GetGridPosList(tetrisGridPosList));
        }
    }
    
    private void OnEnable()
    {
        inputInfo.Reset();
        UIManager.Instance.ShowCorePanel();
        inputController.RegisterKeyEvent(KeyCode.Escape, UIManager.Instance.CorePanel.Pause);
        
        // 开始游戏1s后播放bgm
        scheduleBgmCoroutine = StartCoroutine(ScheduleBgm());
    }

    private Coroutine scheduleBgmCoroutine;
    private IEnumerator ScheduleBgm(float timeDelay = 1f)
    {
        yield return new WaitForSeconds(timeDelay);
        SoundManager.Instance.PlayBgm(Constants.bgm2);
        scheduleBgmCoroutine = null;
    }
    
    private void OnDisable()
    {
        PlayerInputController.Instance.UnRegisterKeyEvent(KeyCode.Escape, UIManager.Instance.CorePanel.Pause);
        if (scheduleBgmCoroutine != null)
        {
            StopCoroutine(scheduleBgmCoroutine);
            scheduleBgmCoroutine = null;
        }
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PauseBgm();
    }

    void Update()
    {
        // 检测输入
        inputController.CheckInput(inputInfo);
    }
    
    public enum Turn
    {
        PlayerControl,
        TetrisDown,
        TetrisDrop,
        TetrisSpawn,
        Performance
    }

    private void FixedUpdate()
    {
        if (turn == Turn.Performance)
            return;
        
        tetrisMoveTimer--;
        turnTimer--;
        downTimer--;

        // timeline的演出时间和帧数之间需要进行换算
        if (turn == Turn.TetrisSpawn)
        {
            tetris.Set(RandomTetrisModel(), board.TetrisSpawnGridPos, TetrisRotation.F);
            
            if (!BoardCheckTetrisValid())
                GameOver();

            turn = Turn.PlayerControl;
            turnTimer = turnIntervalFrame;
            
            inputInfo.Reset(); // 表示在方块产生时不接受操作
            board.UpdateTetrisCubes(tetris.GetGridPosList(tetrisGridPosList), tetris.model.color);
        }

        else if (turn == Turn.TetrisDrop)
        {
            while (BoardCheckTetrisValid(new Vector2Int(0, -1)))
            {
                tetris.currentGridPos.y--;
            }
            RoundOver();
            inputInfo.Reset();
        }

        else if (turn == Turn.TetrisDown)
        {
            // 下降一格, 若不能下降(碰到地图边界或其他方块), 则保持不动
            if (board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentGridPos + new Vector2Int(0, -1))))
            {
                tetris.currentGridPos.y--;
                turn = Turn.PlayerControl;
            }
            else
            {
                // 将Tetris的Cubes转移到Board二维数组的对应位置, 将turn置为Spawn
                RoundOver();
            }
            turnTimer = turnIntervalFrame;
            board.UpdateTetrisCubes(tetris.GetGridPosList(tetrisGridPosList));
        }
        
        else if (turn == Turn.PlayerControl)
        {
            // 执行输入
            if (tetrisMoveTimer < 0)
            {
                if (inputInfo.left && !inputInfo.right)
                {
                    if (board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentGridPos + new Vector2Int(-1, 0))))
                    {
                        tetris.currentGridPos.x--;
                        tetrisMoveTimer = moveIntervalFrame;
                    }
                }
                
                if (!inputInfo.left && inputInfo.right)
                {
                    if (board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentGridPos + new Vector2Int(1, 0))))
                    {
                        tetris.currentGridPos.x++;
                        tetrisMoveTimer = moveIntervalFrame;
                    }
                }
            }

            if (inputInfo.leftRotation && !inputInfo.rightRotation)
            {
                if (board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentTetrisRotation.TetrisRotate(false))))
                {
                    tetris.Rotate(false);
                }
            }

            if (!inputInfo.leftRotation && inputInfo.rightRotation)
            {
                if (board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentTetrisRotation.TetrisRotate(true))))
                {
                    tetris.Rotate(true);
                }
            }

            if (downTimer<0 && inputInfo.down)
            {
                turn = Turn.TetrisDown;
                downTimer = downIntervalFrame;
            }
            
            // 只有在turn == Control的时候, 才会发生turn=Down
            if (turnTimer < 0)
            {
                turn = Turn.TetrisDown;
            }
            
            if (inputInfo.drop)
            {
                turn = Turn.TetrisDrop;
            }
            
            inputInfo.Reset();
            board.UpdateTetrisCubes(tetris.GetGridPosList(tetrisGridPosList));
        }
    }

    private TetrisModel RandomTetrisModel()
    {
        int length = tetrisModels.Length;
        int random = Random.Range(0, length);
        return tetrisModels[random];
    }
    
    private bool BoardCheckTetrisValid()
    {
        return board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList));
    }

    private bool BoardCheckTetrisValid(Vector2Int deltaGridPos)
    {
        return board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentGridPos + deltaGridPos));
    }

    private bool BoardCheckTetrisValid(bool rightRotate)
    {
        return board.CheckTetrisValid(tetris.GetGridPosList(tetrisGridPosList, tetris.currentTetrisRotation.TetrisRotate(rightRotate)));
    }

    public void GameOver()
    {
        this.enabled = false;
        SoundManager.Instance.PlaySoundEffect(Constants.gameLoseSound);
        UIManager.Instance.ShowSettingPanel(SettingPanel.SettingState.GameOver);
    }

    private Timeline eliminateTimeline;
    private Timeline moveTimeline;
    public float timelineDuration = 1f;

    private void GenerateEliminateTimeline(List<Cube> _eliminateCubes, float duration)
    {
        // // 先设置seed
        // for (int i=0;
        //      i < _eliminateCubes.Count; i++)
        // {
        //     _eliminateCubes[i].noiseSeed = i;
        // }
        
        // 先获取到初始位置, 和计算目标位置
        Vector3[] startPosArray = new Vector3[_eliminateCubes.Count];
        Vector3[] targetPosArray = new Vector3[_eliminateCubes.Count];
        Color[] startColorArray = new Color[_eliminateCubes.Count];

        for(int i=0; i<_eliminateCubes.Count; i++)
        {
            _eliminateCubes[i].spriteRenderer.sortingOrder += 1;
            startPosArray[i] = _eliminateCubes[i].transform.localPosition;
            targetPosArray[i] = startPosArray[i] + new Vector3(0, 1, 0);
            startColorArray[i] = _eliminateCubes[i].spriteRenderer.color;
        }
        
        eliminateTimeline = Timeline.Get(
        (elapsed, rate) =>
        {
            for (int i = 0; i < _eliminateCubes.Count; i++)
            {
                _eliminateCubes[i].Shake(1.5f, 50, elapsed / duration + i);
                _eliminateCubes[i].PosLerp(startPosArray[i], targetPosArray[i], elapsed/duration);
                _eliminateCubes[i].ColorLerp(startColorArray[i], Color.black, elapsed/duration);
                _eliminateCubes[i].ScaleLerp(new Vector3(1,1,1), new Vector3(0.1f, 0.1f, 1f), elapsed/duration);
            }

            if (elapsed >= duration)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // (elapsed, rate) =>
        // {
        //     
        // }
        );
        eliminateTimeline.OnDone = () =>
        {
            foreach (var cube in _eliminateCubes)
            {
                cube.Release();
            }
        };
    }

    private void GenerateMoveTimeline(List<Board.MoveCube> _moveCubes, float duration)
    {
        moveTimeline = Timeline.Get(
            (elapsed, rate) =>
            {
                for (int i = 0; i < _moveCubes.Count; i++)
                {
                    _moveCubes[i].cube.PosLerp(_moveCubes[i].from, _moveCubes[i].to, elapsed/duration);
                }
                if (elapsed >= duration)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        );
    }
    
    private void RoundOver()
    {
        board.TetrisCubesToBoard(tetris.GetGridPosList(tetrisGridPosList));
        board.UpdateBoardCubes();
        tetris.model = null;
        // 消除方块
        bool needEliminate = board.EliminateRows(eliminateCubes, moveCubes);

        if (needEliminate)
        {
            // foreach (Cube cube in eliminateCubes)
            // {
            //     cube.Release();
            // }
            
            // 如何在消除前安排好方块的位置
            // 因为已经移动到boardArray中, 所以只需要更新boardArray
            // board.UpdateBoardCubes();
            // 不太行, 因为boardCubes的位置已经是
            
            turn = Turn.Performance;
            
            // 这里生成timeline
            // eliminate timeline
            GenerateEliminateTimeline(eliminateCubes, 0.5f*timelineDuration);
            GenerateMoveTimeline(moveCubes, 0.2f*timelineDuration);
            eliminateTimeline.next = moveTimeline;
            moveTimeline.OnDone = () =>
            {
                turn = Turn.TetrisSpawn;
            };
            TimelineManager.Instance.AddTimeline(eliminateTimeline);
        }
        else
        {
            board.UpdateBoardCubes();
            turn = Turn.TetrisSpawn;
        }
    }

    public void Pause(bool isPause)
    {
        if (isPause)
        {
            this.enabled = false;
        }
        else
        {
            this.enabled = true;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void Store(CoreStorage storageData)
    {
        storageData.turn = (int)turn;
        storageData.tetrisMoveTimer = tetrisMoveTimer;
        storageData.turnTimer = turnTimer;
        storageData.downTimer = downTimer;
    }

    public void Retrieve(CoreStorage storageData)
    {
        turn = (Turn)storageData.turn;
        
        if (turn == Turn.Performance)
            turn = Turn.TetrisSpawn;
        
        tetrisMoveTimer = storageData.tetrisMoveTimer;
        turnTimer = storageData.turnTimer;
        downTimer = storageData.downTimer;
    }
}
