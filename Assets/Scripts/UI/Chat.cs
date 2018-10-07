using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Chat {
    /*
    private InputField TextInput;
    private Messenger TextOutput;
    private string[] History; //newest has highest Index
    private int MaxLength;
    private int RecallIndex = 0;

    public Chat(InputField Input, Messenger Output, int HistoryLength)
    {
        TextInput = Input;
        TextOutput = Output;
        MaxLength = RecallIndex = HistoryLength;
        Array.Resize<string>(ref History, 1);
        Transform Players = GameObject.Find("Players").transform;
        for (int i = 0; i < Players.childCount; i++) if (Players.GetChild(i).name.Contains("Player")) 
        TextInput.onEndEdit.AddListener(Log);
    }

    private void HistoryAdd(string Message)
    {
        if (History.Length < MaxLength) Array.Resize<string>(ref History, History.Length + 1);
        if (History.Length == MaxLength) for (int i = 0; i < History.Length - 1; i++) History[i] = History[i + 1];
        History[History.Length - 1] = Message;
        if (History.Length == 1) Array.Resize<string>(ref History, 2);
        RecallIndex = History.Length;
    }

    public void Log(string NewMessage)
    {
        if ( NewMessage != "")
        {
            TextOutput.Log(NewMessage);
            if (History[History.Length - 1] != NewMessage) HistoryAdd(NewMessage);
            TextInput.text = "";
        }
        }

    private void Recall()
    { //checks what was typed before via History and fills TextInput

        if (RecallIndex < 0) RecallIndex = 0;
        if (RecallIndex > History.Length) RecallIndex = History.Length;
        if (RecallIndex == History.Length) TextInput.text = "";
        else TextInput.text = History[RecallIndex];
    }

    public void PseudoUpdate()
    { // Has to be called on the Updatefunction from a MonoBehaviour
        if (TextInput.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                RecallIndex--;
                Recall();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                RecallIndex++;
                Recall();
            }
            if (Input.GetKeyDown(Controls.ChatKey)) Log(TextInput.text);
        } else if (Input.GetKeyDown(Controls.ChatKey)) TextInput.Select();
    }
    */
}

    









    /*
    private Text Read;
    public string[] History= new string[5]; //last shown in chat
    public string[] PrevWrite = new string[10]; //last entered lines
    private ushort PrevIndex = 0; //used for navgating between last entered lines
    
    private void Start()
    {
        Write = transform.Find("Write").GetComponent<InputField>();
        Write.interactable = false;
        Read = transform.Find("Read").GetComponent<Text>();
        for (ushort i = 0; i < PrevWrite.Length; i++) PrevWrite[i] = string.Empty;
    }

    private void Update()
    {
        if (Write.interactable)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) & PrevIndex <= PrevWrite.Length) PrevIndex++;
            if (Input.GetKeyDown(KeyCode.DownArrow) & PrevIndex > 0) PrevIndex--;
            if (PrevIndex > 0) Write.text = PrevWrite[PrevIndex - 1];
            if (!Input.GetKeyDown(KeyCode.UpArrow) & !Input.GetKeyDown(KeyCode.UpArrow) & Input.anyKeyDown) PrevIndex = 0;
            
            else if (Input.GetKeyDown(KeyCode.DownArrow)) Write.text = string.Empty;
        }

        if (Input.GetKeyDown(KeyCode.Return))
            if (!Write.interactable)
            { //Start of Typing
                Status.Paused = true;
                Write.interactable = true;
                Write.text = string.Empty;
                Write.Select();
            }
            else
            { //End of Typing
                Write.interactable = false;
                if (Write.text.Length > 2)
                    if (Write.text[0] == '/')
                    {
                        Insert(PrevWrite, Write.text, 0, false);
                        ExecuteCommand(Write.text.Substring(1, Write.text.Length - 1));
                    } else Insert(History, "Player: " + Write.text, 0, false);
                Write.text = string.Empty;
                PrevIndex = 0;
                Status.Paused = false;
            }
        

        Read.text = History[History.Length - 1];
        for (int i = History.Length - 2; i >= 0; i--) Read.text += Environment.NewLine + History[i];
    }

    public static void Log(string Message)
    {
        Chat C = GameObject.Find("Chat").GetComponent<Chat>();
        Insert(C.History, Message, 0, false);
    }

    public static void Insert(string[] Arr, string New, ushort Index, bool Down)
    {
        if (Down) for (ushort i = 1; i <= Index; i++) Arr[i - 1] = Arr[i];
        else for (int i = Arr.Length - 2; i >= Index; i--) Arr[i + 1] = Arr[i];
        Arr[Index] = New;
    }*/

    /*private void ExecuteCommand(string Command)
    {
        Status Player = GameObject.Find("Player").GetComponent<Status>();
        AI_Control AI = GameObject.Find("Enemies").GetComponent<AI_Control>();
        switch (Command)
        {
            case "NextLvl":
                GameObject.Find("Level").GetComponent<WorldGenerator>().LoadNextLevel();
                break;

            case "PrevLvl":
                GameObject.Find("Level").GetComponent<WorldGenerator>().LoadNextLevel();
                break;

            case "Kill":
                Player.Kill();
                break;


            case "Speed_Acc+":
                Player.transform.GetComponent<Movement>().Acceleration += 100;
                Chat.Log("Slightly increased Player acceleration.");
                break;

            case "Speed_Acc++":
                Player.transform.GetComponent<Movement>().Acceleration += 500;
                Chat.Log("Greatly increased Player acceleration.");
                break;

            case "Speed_Acc-":
                Player.transform.GetComponent<Movement>().Acceleration -= 100;
                Chat.Log("Slightly decreased Player acceleration.");
                break;

            case "Speed_Acc--":
                Player.transform.GetComponent<Movement>().Acceleration -= 500;
                Chat.Log("Greatly decreased Player acceleration.");
                break;


            case "Speed_Max+":
                Player.transform.GetComponent<Movement>().MaxSpeed ++;
                Chat.Log("Slightly increased Player speed.");
                break;

            case "Speed_Max++":
                Player.transform.GetComponent<Movement>().MaxSpeed += 10;
                Chat.Log("Greatly increased Player speed.");
                break;

            case "Speed_Max-":
                Player.transform.GetComponent<Movement>().Acceleration --;
                Chat.Log("Slightly decreased Player speed.");
                break;

            case "Speed_Max--":
                Player.transform.GetComponent<Movement>().Acceleration -= 10;
                Chat.Log("Greatly decreased Player speed.");
                break;


            case "AI_Amount+":
                AI.EnemyAmount += 10;
                Chat.Log("Slightly increased Enemy amount.");
                break;

            case "AI_Amount++":
                AI.EnemyAmount += 100;
                Chat.Log("Greatly increased Enemy amount.");
                break;

            case "AI_Amount-":
                AI.EnemyAmount -= 10;
                Chat.Log("Slightly decreased Enemy amount.");
                break;

            case "AI_Amount--":
                AI.EnemyAmount -= 100;
                Chat.Log("Greatly decreased Enemy amount.");
                break;


            case "AI_Speed+":
                AI.Speed += 0.25f;
                Chat.Log("Slightly increased Enemy speed.");
                break;

            case "AI_Speed++":
                AI.Speed += 1;
                Chat.Log("Greatly increased Enemy speed.");
                break;

            case "AI_Speed-":
                AI.Speed -= 0.25f;
                Chat.Log("Slightly decreased Enemy speed.");
                break;

            case "AI_Speed--":
                AI.Speed -= 1;
                Chat.Log("Greatly decreased Enemy speed.");
                break;

            default:
                Write.text = "Command unknown.";
                break;
        }
    }*/

