using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AxisEventSystem //: MonoBehaviour
{

    public event Action OnAxisKeyDown;
    public void AxisKeyDown()
    {
        if(OnAxisKeyDown != null)
        {
            OnAxisKeyDown();
        }
    }
    public event Action OnAxisKeyUp;
    public void AxisKeyUp()
    {
        if (OnAxisKeyUp != null)
        {
            OnAxisKeyUp();
        }
    }
    public event Action OnAxisKeyStay;
    public void AxisKeyStay()
    {
        if (OnAxisKeyStay != null)
        {
            OnAxisKeyStay();
        }
    }

    public event Action OnAxisKeyChanged;
    public void AxisKeyChanged()
    {
        if(OnAxisKeyChanged != null)
        {
            OnAxisKeyChanged();
        }
    }

    /*

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
