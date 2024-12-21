using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CorePanel : MonoBehaviour
{
    public Button PauseButton;

    public void Pause()
    {
        UIManager.Instance.ShowSettingPanel(SettingPanel.SettingState.GameSetting);
        CoreManager.Instance.Pause(true);
    }
}
