using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController: MonoBehaviour
{
    public static bool HasSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName("Core");
        if (scene.isLoaded)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void LoadScene(string sceneName, LoadSceneMode mode, Action onSceneLoaded)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName, mode);
        
        void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
        {
            if (loadedScene.name == sceneName)
            {
                SceneManager.SetActiveScene(loadedScene);
                onSceneLoaded?.Invoke();
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    }

    public static void LoadSceneIfNotLoaded(string sceneName, LoadSceneMode mode, Action onSceneLoaded)
    {
        if (!HasSceneLoaded(sceneName))
        {
            LoadScene(sceneName, mode, onSceneLoaded);
        }
    }
}