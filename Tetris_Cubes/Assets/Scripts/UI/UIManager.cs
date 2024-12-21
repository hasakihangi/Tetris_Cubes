using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public SettingPanel SettingPanel;
    public CorePanel CorePanel;

    public static UIManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void ShowSettingPanel(SettingPanel.SettingState settingState)
    {
        SettingPanel.gameObject.SetActive(true);
        SettingPanel.SetButtons(settingState);
    }

    public void ShowCorePanel()
    {
        CorePanel.gameObject.SetActive(true);
    }

    public void HideCorePanel()
    {
        CorePanel.gameObject.SetActive(false);
    }
}
