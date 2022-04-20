using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeController : MonoBehaviour
{
    public int currentUpgradeAmount;
    PlayerController LocalPlayer;
    public GameObject UpgradePanel;

    public void SetCurrentUpgradeAmount(int _amount)
    {
        currentUpgradeAmount += _amount;
    }

    public void UseUpgrade()
    {
        currentUpgradeAmount -= 1;
    }

    public void setPlayer(PlayerController _player)
    {
        LocalPlayer = _player;
        currentUpgradeAmount = 0;
    }

    public void Show()
    {
        UpgradePanel.SetActive(true);
    }

    public void Hide()
    {
        UpgradePanel.SetActive(false);
    }

    public void OnUpgradePressed(string _upgrade)
    {
        if (currentUpgradeAmount == 0)
        {
            currentUpgradeAmount = 0;
        }
        if (currentUpgradeAmount > 0)
        {
            LocalPlayer.SendUpgrade(_upgrade);
            currentUpgradeAmount -= 1;
            if (currentUpgradeAmount == 0)
            {
                Hide();
            }
        }
    }
}
