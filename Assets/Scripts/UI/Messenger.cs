using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Messenger {
/*
    private Text Feed;
    private string[] Lines = new string[1];
    private int FeedLength;


    //==================================================================
    //                           Constructor
    //==================================================================
    public Messenger(Text TextField, int MaxLines)
    {
        Feed = TextField;
        FeedLength = MaxLines;
        Array.Resize<string>(ref Lines, FeedLength);
        for (int i = 0; i < FeedLength; i++) Lines[i] = "";
    }

    //==================================================================
    //                           Setters & Getters
    //==================================================================
    public string ReadLine(int Index)
    {
        if (Index < FeedLength) return Lines[Index];
        else return String.Empty;
    }
    public int Length() { return FeedLength; }

    //==================================================================
    //                           Minor Functions
    //==================================================================
    private void RefreshFeed()
    {
        Feed.text = "";
        for (int i = 0; i < FeedLength; i++) Feed.text += Lines[i] + Environment.NewLine;
    }
    private void PushLines(int Amount)
    {
        for (int i = 0; i < FeedLength - Amount; i++) Lines[i] = Lines[i + Amount];
        for (int i = FeedLength -1; i > FeedLength - Amount; i--) Lines[i] = "";
    }


    //==================================================================
    //                           Major Functions
    //==================================================================
    public void Log(string NewMessage)
    {
        PushLines(1);
        Lines[FeedLength - 1] = NewMessage;
        RefreshFeed();
    }
    */
}

