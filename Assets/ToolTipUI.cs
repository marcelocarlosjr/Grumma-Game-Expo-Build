using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipUI : MonoBehaviour
{
    public GameObject ToolTip;
    public Text Title;
    public Text Description;
    public Text Stat;

    public void SetStats(string _title, string _description, string _stat, string _rarity, bool _setactive)
    {
        if (_setactive)
        {
            Color32 RarityColor = new Color32(0,0,0,255);
            switch (_rarity)
            {
                case "Common":
                    RarityColor = new Color32(26, 88, 149, 225);
                    break;
                case "Uncommon":
                    RarityColor = new Color32(127, 43, 135, 255);
                    break;
                case "Rare":
                    RarityColor = new Color32(135, 43, 49, 255);
                    break;
                case "Legendary":
                    RarityColor = new Color32(225, 198, 61, 225);
                    break;
            }
            ToolTip.SetActive(true);
            Title.text = _title;
            Title.color = RarityColor;
            Description.text = _description;
            Stat.text = _stat + "%";
        }
        else
        {
            ToolTip.SetActive(false);
        }
    }

}
