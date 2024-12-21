using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    
    public CubePool CubePool => GetComponent<CubePool>();
    
    private void Awake()
    {
        instance = this;
        
        // Init
        GetComponent<DesignerTable>().Init();
        GetComponent<PlayerInputController>().Init();
    }

    private void Start()
    {
        UIManager.Instance.ShowSettingPanel(SettingPanel.SettingState.Start);
        UIManager.Instance.HideCorePanel();
    }

    private void Update()
    {
        PlayerInputController.Instance.SendInputEvents();
        if (Input.GetMouseButtonDown(0))
        {
            SoundManager.Instance.PlaySoundEffect(Constants.clickSound);
        }
    }

    // // 加载Core场景, 获取到CoreLogicManager
    // public void LoadCoreScene()
    // {
    //     SceneManager.LoadScene("Core", LoadSceneMode.Additive);
    //     SceneManager.sceneLoaded += OnCoreSceneLoaded;
    // }
    //
    // public void LoadGameAtStart()
    // {
    //     SceneManager.LoadScene("Core", LoadSceneMode.Additive);
    //     SceneManager.sceneLoaded += OnLoadAtStart;
    // }

    // private void OnCoreSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    // {
    //     if (loadedScene.name == "Core")
    //     {
    //         SceneManager.SetActiveScene(loadedScene);
    //
    //         // 处理游戏开始的逻辑
    //         CoreManager.Instance.BuildNewGame();
    //         
    //         uiManager.ShowCorePanel();
    //
    //         // 卸载事件监听，避免重复调用
    //         SceneManager.sceneLoaded -= OnCoreSceneLoaded;
    //     }
    // }
    //
    // private void OnLoadAtStart(Scene loadedScene, LoadSceneMode mode)
    // {
    //     if (loadedScene.name == "Core")
    //     {
    //         SceneManager.SetActiveScene(loadedScene);
    //         storageManager.LoadData();
    //         SceneManager.sceneLoaded -= OnLoadAtStart;
    //     }
    // }

    public void OnApplicationPause(bool pause)
    {
        
    }
}
