using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

// 存储和加载的入口
public class StorageManager : MonoBehaviour
{
    public CoreStorage coreSaveData;
    public CoreStorage coreLoadData;

    public BoardStorage boardSaveData;
    public BoardStorage boardLoadData;

    public TetrisStorage tetrisSaveData;
    public TetrisStorage tetrisLoadData;

    private string saveFilePath;
    public string SaveFilePath => saveFilePath;
    public int version = 1;

    private static StorageManager instance;
    public static StorageManager Instance => instance;
    
    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveFile");
        print(saveFilePath);
        instance = this;
    }
    
    public void LoadData()
    {
        coreLoadData = new CoreStorage();
        boardLoadData = new BoardStorage();
        tetrisLoadData = new TetrisStorage();
        
        // 填充LoadData
        using (var binaryReader = new BinaryReader(File.Open(saveFilePath, FileMode.Open)))
        {
            // 这个是引用类型, 倒是可以重复利用
            GameDataReader dataReader = new GameDataReader(binaryReader);
            
            // 第一个就不能读取? 因为没成功写入, 是一个0字节的文件
            int loadVersion = dataReader.ReadInt();
            if (loadVersion == version)
            {
                coreLoadData.Load(dataReader);
                boardLoadData.Load(dataReader);
                tetrisLoadData.Load(dataReader);
            }
            else
            {
                Debug.Log("version error!");
            }
        }
    }

    public void SaveData()
    {
        coreSaveData = new CoreStorage();
        boardSaveData = new BoardStorage();
        tetrisSaveData = new TetrisStorage();
        
        // 先填充SaveData类型
        CoreManager.Instance.Store(coreSaveData);
        CoreManager.Instance.board.Store(boardSaveData);
        CoreManager.Instance.tetris.Store(tetrisSaveData);

        // 再写入文件
        using var binaryWriter = new BinaryWriter(File.Open(saveFilePath, FileMode.Create));
        GameDataWriter dataWriter = new GameDataWriter(binaryWriter);
        dataWriter.Write(version);
        coreSaveData.Save(dataWriter);
        boardSaveData.Save(dataWriter);
        tetrisSaveData.Save(dataWriter);
    }
}
