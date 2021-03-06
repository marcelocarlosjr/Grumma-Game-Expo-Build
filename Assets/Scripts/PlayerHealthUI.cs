using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerController LocalPlayer;

    public RectTransform HP;
    public RectTransform Stamina;
    public RectTransform EXPBAR;
    public Text Level;

    public Image UIImage;

    public Sprite Warrior;
    public Sprite Archer;
    public Sprite Mage;

    public LevelSystemAnimated levelSystemAnimated;

    bool PlayerConnected;

    public void SetPlayer(PlayerController _player)
    {
        LocalPlayer = _player;
        PlayerConnected = true;
    }

    public void RemovePlayer()
    {
        LocalPlayer = null;
        PlayerConnected = false;
    }

    private void Update()
    {
        if(PlayerConnected)
        {
            HP.localScale = Vector3.Lerp(HP.localScale, new Vector3(LocalPlayer.Health / LocalPlayer.MaxHealth, 1, 1), 1f);
            Stamina.localScale = Vector3.Lerp(Stamina.localScale, new Vector3(LocalPlayer.Stamina / LocalPlayer.MaxStamina, 1, 1), 1f);
            EXPBAR.localScale = new Vector3((float)LocalPlayer.EXP / (float)LocalPlayer.EXPToLevel, 1, 1);
            Level.text = LocalPlayer.Level.ToString();
        }
    }
    
    public void SetPlayerImage(int prefab)
    {
        switch (prefab)
        {
            case 0:
                UIImage.sprite = Warrior;
                break;
            case 1:
                UIImage.sprite = Archer;
                break;
            case 2:
                UIImage.sprite = Mage;
                break;
        }
    }
}
