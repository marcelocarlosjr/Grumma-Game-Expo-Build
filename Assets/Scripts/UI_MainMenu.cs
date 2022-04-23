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
        FindObjectOfType<OfflinePlayerHolder>().setPNAME(n);
    }

    public void SetChar(int c)
    {
        Char = c;
        OfflinePlayerHolder.PlayerPrefab = c;
        FindObjectOfType<AudioManager>().Play("ClickUI");
    }

    private void Start()
    {
        FindObjectOfType<OfflinePlayerHolder>().RemoveLoading();
    }

    public void Connect()
    {
        if(Pname == "SPECTATE")
        {
            OfflinePlayerHolder.PlayerPrefab = 69;
            SceneManager.LoadScene(1);
        }

        if(Pname != "" && Char != -1)
        {
            FindObjectOfType<AudioManager>().Play("ClickUI");
            FindObjectOfType<OfflinePlayerHolder>().ShowLoading();
            SceneManager.LoadScene(1);
        }
    }
}
