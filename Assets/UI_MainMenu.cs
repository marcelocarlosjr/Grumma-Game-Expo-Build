using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    string Pname = "";
    int Char = -1;
    public void SetPName(string n)
    {
        Pname = n;
        OfflinePlayerHolder.PName = n;
    }

    public void SetChar(int c)
    {
        Char = c;
        OfflinePlayerHolder.PlayerPrefab = c;
    }

    public void Connect()
    {
        if(Pname != "" && Char != -1)
        {
            SceneManager.LoadScene(1);
        }
    }
}
