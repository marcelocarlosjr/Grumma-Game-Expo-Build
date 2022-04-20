using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeController : MonoBehaviour
{
    public Animator MyAnim;
    [SerializeField] private int currentUpgradeAmount;
    PlayerController LocalPlayer;

    private void Start()
    {
        MyAnim = GetComponent<Animator>();
    }

    public void SetCurrentUpgradeAmount(int _amount)
    {
        currentUpgradeAmount += _amount;
    }

    public void UseUpgrade()
    {
        currentUpgradeAmount -= 1;
    }

    public void FadeIN()
    {
        MyAnim.SetTrigger("FADEIN");
    }
    public void FadeOut()
    {
        MyAnim.SetTrigger("FADEOUT");
    }

    public void setPlayer(PlayerController _player)
    {
        LocalPlayer = _player;
        currentUpgradeAmount = 0;
    }

    public void OnUpgradePressed(string _upgrade)
    {
        if (currentUpgradeAmount == 0)
        {
            FadeOut();
            currentUpgradeAmount = 0;
        }
        if (currentUpgradeAmount > 0)
        {
            switch (_upgrade)
            {
                case "damage":
                    LocalPlayer.DamageUpgrade += 1;
                    break;
                case "health":
                    LocalPlayer.HealthUpgrade += 1;
                    break;
                case "speed":
                    LocalPlayer.MoveSpeedUpgrade += 1;
                    break;
                case "stamina":
                    LocalPlayer.StaminaUpgrade += 1;
                    break;
                case "regen":
                    LocalPlayer.HealthRegenerationUpgrade += 1;
                    break;
                case "exp":
                    LocalPlayer.EXPModUpgrade += 1;
                    break;
            }
            currentUpgradeAmount -= 1;
        }
    }
}
