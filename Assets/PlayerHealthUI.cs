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
}
