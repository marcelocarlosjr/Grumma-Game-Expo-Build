using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerController LocalPlayer;

    public RectTransform HP;
    public Text HPText;
    public RectTransform Stamina;

    public LevelSystemAnimated levelSystemAnimated;

    bool PlayerConnected;

    public void SetPlayer(PlayerController _player)
    {
        LocalPlayer = _player;
        PlayerConnected = true;
    }

    private void Update()
    {
        if(PlayerConnected)
        {
            HP.localScale = Vector3.Lerp(HP.localScale, new Vector3(LocalPlayer.Health / LocalPlayer.MaxHealth, 1, 1), 1f);
            Stamina.localScale = Vector3.Lerp(HP.localScale, new Vector3(LocalPlayer.Stamina / LocalPlayer.MaxStamina, 1, 1), 1f);
            HPText.text = LocalPlayer.Health + "/" + LocalPlayer.MaxHealth;
        }
    }

    public void SetLevelSystemAnimated(LevelSystemAnimated levelSystemAnimated)
    {
        //set values on network start
        levelSystemAnimated.OnExperienceChanged += LevelSystem_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged += LevelSystem_OnLevelChanged;
    }

    private void LevelSystem_OnExperienceChanged(object sender, EventArgs e)
    {
        //set experience bar
    }
    private void LevelSystem_OnLevelChanged(object sender, EventArgs e)
    {
        //set level num
    }
}
