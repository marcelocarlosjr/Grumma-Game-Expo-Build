using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnUI : MonoBehaviour
{
    public GameObject button;

    public void Show()
    {
        button.SetActive(true);
    }

    public void Hide()
    {
        button.SetActive(false);
    }

    public void Respawn()
    {
        var MyCore = FindObjectOfType<NetworkCore>();
        MyCore.StartCoroutine(MyCore.Disconnect(MyCore.LocalConnectionID));
        SceneManager.LoadScene(0);
        Hide();
    }
}
