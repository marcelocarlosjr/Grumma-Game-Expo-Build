using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInputManager : MonoBehaviour
{
    public GameObject LocalNPM;
    public InputField NameInput;
    public Toggle ReadyInput;


    public void GetName(string value)
    {
        if(LocalNPM != null)
        {
            LocalNPM.GetComponent<NPM>().GetName(value);
        }
    }
    public void GetClass(int value)
    {
        if(LocalNPM != null)
        {
            LocalNPM.GetComponent<NPM>().GetClass(value);
        }
    }
    public void GetReady(bool value)
    {
        if(LocalNPM != null)
        {
            LocalNPM.GetComponent<NPM>().GetReady(value);
        }
    }
}
