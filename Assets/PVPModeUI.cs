using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PVPModeUI : MonoBehaviour
{
    public Image uiPVP;

    public Sprite NOPVP;
    public Sprite PVP;

    public void SetPVP(bool _IsSafe)
    {
        if (_IsSafe)
        {
            uiPVP.sprite = NOPVP;
        }
        else
        {
            uiPVP.sprite = PVP;
        }
    }
}
