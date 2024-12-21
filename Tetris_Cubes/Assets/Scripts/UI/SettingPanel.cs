using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingPanel : MonoBehaviour
{
    // 多个Button可以用Dictionary的方式添加, 通过字符串索引到具体的Button, 因为在列表中, 很容易遍历到每一个
    public Button StartNewGameButton;
    public Button ResumeButton;
    public Button SaveButton;
    public Button LoadAtStartButton;
    public Button LoadAtPauseButton;
    public GameObject pointerHandler;
    // 点击周围区域就关闭该面板

    private EventSystem eventSystem;
    private void Awake()
    {
        eventSystem = EventSystem.current;
    }

    private void OnDisable()
    {
        PlayerInputController.Instance.UnRegisterKeyEvent(KeyCode.Escape, Resume);
    }

    public enum SettingState
    {
        Start,
        GameSetting,
        GameOver
    }

    public void SetButtons(SettingState settingState)
    {
        StartNewGameButton.gameObject.SetActive(false);
        ResumeButton.gameObject.SetActive(false);
        pointerHandler.SetActive(false);
        SaveButton.gameObject.SetActive(false);
        LoadAtPauseButton.gameObject.SetActive(false);
        LoadAtStartButton.gameObject.SetActive(false);
        switch (settingState)
        {
            case SettingState.Start:
                StartNewGameButton.gameObject.SetActive(true);
                // 如果有找到存储文件, 则显示LoadAtStart Button
                if (File.Exists(StorageManager.Instance.SaveFilePath))
                {
                    LoadAtStartButton.gameObject.SetActive(true);
                }
                eventSystem.SetSelectedGameObject(StartNewGameButton.gameObject);
                break;
            case SettingState.GameSetting:
                StartNewGameButton.gameObject.SetActive(true);
                ResumeButton.gameObject.SetActive(true);
                pointerHandler.SetActive(true);
                SaveButton.gameObject.SetActive(true);
                // 如果有找到存储文件, 则显示LoadAtPause Button
                if (File.Exists(StorageManager.Instance.SaveFilePath))
                {
                    LoadAtPauseButton.gameObject.SetActive(true);
                }
                
                eventSystem.SetSelectedGameObject(ResumeButton.gameObject);
                PlayerInputController.Instance.RegisterKeyEvent(KeyCode.Escape, Resume);
                break;
            // case SettingState.PassivePause:
            //     ResumeButton.gameObject.SetActive(true);
            //     pointerHandler.SetActive(true);
            //     eventSystem.SetSelectedGameObject(ResumeButton.gameObject);
            //     GameManager.Instance.InputController.RegisterKeyEvent(KeyCode.Escape, Resume);
            //     break;
            case SettingState.GameOver:
                StartNewGameButton.gameObject.SetActive(true);
                if (File.Exists(StorageManager.Instance.SaveFilePath))
                {
                    LoadAtPauseButton.gameObject.SetActive(true);
                }
                break;
        }
    }

    // public void StartNewGame()
    // {
    //     // 如果CoreScene没加载, 则加载, 其逻辑在里面有了
    //     if (!GameManager.Instance.CheckCoreSceneLoaded(out _))
    //     {
    //         // 在加载完成之后会自动加载游戏逻辑
    //         GameManager.Instance.LoadCoreScene();
    //     }
    //     else
    //     {
    //         CoreManager.Instance.BuildNewGame();
    //     }
    //
    //     this.gameObject.SetActive(false);
    // }

    public void StartNewGame()
    {
        if (!SceneController.HasSceneLoaded("Core"))
        {
            SceneController.LoadScene("Core", LoadSceneMode.Additive, () =>
            {
                CoreManager.Instance.BuildNewGame();
            });
        }
        else
        {
            CoreManager.Instance.BuildNewGame();
        }
        this.gameObject.SetActive(false);
        SoundManager.Instance.PlaySoundEffect(Constants.gameStartSound);
    }

    public void Resume()
    {
        CoreManager.Instance.Pause(false);
        this.gameObject.SetActive(false);
    }

    public void LoadAtStart()
    {
        // GameManager.Instance.LoadGameAtStart();
        // 首先加载场景, 然后加载LoadData, 最后CoreManager.LoadGame
        StorageManager.Instance.LoadData();
        SceneController.LoadSceneIfNotLoaded("Core", LoadSceneMode.Additive, () =>
        {
            CoreManager.Instance.LoadGame();
            CoreManager.Instance.enabled = true;
        });
        this.gameObject.SetActive(false);
        SoundManager.Instance.PlaySoundEffect(Constants.gameStartSound);
    }

    public void LoadAtPause()
    {
        StorageManager.Instance.LoadData();
        CoreManager.Instance.LoadGame();

        Resume();
    }

    public void Save()
    {
        StorageManager.Instance.SaveData();
    }
}
