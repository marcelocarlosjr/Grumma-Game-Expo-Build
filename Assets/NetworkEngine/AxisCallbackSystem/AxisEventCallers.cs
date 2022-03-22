using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AxisEventCallers : MonoBehaviour
{
    public static Dictionary<string, AxisEventSystem> InputEvents;
    public static Dictionary<string, float> LastInput;
    public string[] WatchedAxis;
    public bool DirChanged = false;
    public bool IsMoving = false;

    public static AxisEventCallers current;
    public event Action OnDirectionChanged;
    public void DirectionChanged()
    {
        if (OnDirectionChanged != null)
        {
            OnDirectionChanged();
        }
    }
    public event Action OnMove;
    private void Move()
    {
        if (OnMove != null)
        {
            OnMove();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_SERVER
        Debug.Log("Destroying the Event Listener since we are a server.");
        Destroy(this);
#else



        //Initialize LastInputs
        LastInput = new Dictionary<string, float>();
        //Initialize InputEvents
        InputEvents = new Dictionary<string, AxisEventSystem>();
        foreach (string x in WatchedAxis)
        {
            LastInput.Add(x, 0);
            InputEvents.Add(x, new AxisEventSystem());
        }
#endif
    }

    private void Awake()
    {
        current = this;
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            foreach (string x in WatchedAxis)
            {
                if (Input.GetAxis(x) != LastInput[x])
                {
                    InputEvents[x].AxisKeyChanged();
                    if (Mathf.Abs(Input.GetAxis(x)) < .1f)
                    {
                        InputEvents[x].AxisKeyUp();
                    }
                    else
                    {
                        InputEvents[x].AxisKeyDown();
                    }
                    LastInput[x] = Input.GetAxis(x);
                    if (x == "Vertical" || x == "Horizontal")
                    {
                        DirChanged = true;
                    }
                }
                else if (Mathf.Abs(Input.GetAxis(x)) > .1f)
                {
                    InputEvents[x].AxisKeyStay();
                    if (x == "Vertical" || x == "Horizontal")
                    {
                        IsMoving = true;
                    }
                }
            }
            if (DirChanged)
            {
                DirectionChanged();
                DirChanged = false;
            }
            if (IsMoving)
            {
                Move();
                IsMoving = false;
            }
        }
        catch {
            //Will only happen on scene switch.
        }


    }
}
