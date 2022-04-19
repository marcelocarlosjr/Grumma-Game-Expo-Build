using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    public void SetPName(string n)
    {
        OfflinePlayerHolder.PName = n;
    }

    public void SetChar(int c)
    {
        OfflinePlayerHolder.PlayerPrefab = c;
    }

    public void Connect()
    {
        SceneManager.LoadScene(1);
    }
}
