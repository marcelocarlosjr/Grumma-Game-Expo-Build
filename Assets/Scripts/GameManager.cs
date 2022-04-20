using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject LoadingScene;

    private void Awake()
    {
        instance = this;

        SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
    }

    public void LoadGame()
    {
        SceneManager.UnloadSceneAsync(0);
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }
}
