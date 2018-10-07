using System;
using System.Collections.Generic;
using UnityEngine;

public class Controls {

    [SerializeField] private Dictionary<string, KeyCode> Keys = new Dictionary<string, KeyCode> {
        {"Set Marker", KeyCode.Mouse0 },
        {"Delete Marker", KeyCode.Mouse1 },
        {"Up", KeyCode.W },
        {"Left", KeyCode.A },
        {"Down", KeyCode.S},
        {"Right", KeyCode.D},
        {"Stop", KeyCode.Space},
        {"Show Map", KeyCode.M},
        {"Chat", KeyCode.Return}
    };
    [SerializeField] private int HorAxis = 0;
    [SerializeField] private int VerAxis = 0;
    [SerializeField] private int AxisStepping = 50; //how many increments there are between 0 and 1 of an axis (actually there are always twice as much in each direction but nvmd...)

    public void DecreaseMovement(bool Instantly) //has to be called every Fixed Update by  the Movement Script
    {
        if (Instantly) HorAxis = VerAxis = 0;
        else
        {
            if (HorAxis != 0) HorAxis -= HorAxis / Math.Abs(HorAxis);
            if (VerAxis != 0) VerAxis -= VerAxis / Math.Abs(VerAxis);
        }
    }

    public KeyCode getKey(string Key)
    {
        KeyCode Result = KeyCode.None;
        Keys.TryGetValue(Key, out Result);
        return Result;
    }

    public bool setKey(string Index, KeyCode newKeyCode)
    {
        bool Warning = false;
        Keys[Index] = newKeyCode;
        foreach (KeyValuePair<string, KeyCode> Element in Keys) if (Element.Key != Index && Element.Value == newKeyCode) { Warning = true; break; };
        return Warning;
    }

    public Vector2 getDir()
    { 
            if (Input.GetKey(Keys["Up"]) && VerAxis <= AxisStepping) VerAxis += 2;
            if (Input.GetKey(Keys["Down"]) && VerAxis >= -AxisStepping) VerAxis -= 2;
            if (Input.GetKey(Keys["Right"]) && HorAxis <= AxisStepping) HorAxis += 2;
            if (Input.GetKey(Keys["Left"]) && HorAxis >= -AxisStepping) HorAxis -= 2;
         return new Vector2(HorAxis, VerAxis);
    }

    public bool Moving()
    {
        if (Input.inputString.Contains(Keys["Up"].ToString()) ||
            Input.inputString.Contains(Keys["Down"].ToString()) ||
            Input.inputString.Contains(Keys["Left"].ToString()) ||
            Input.inputString.Contains(Keys["Right"].ToString())) return true;
        return false;
    }
}
