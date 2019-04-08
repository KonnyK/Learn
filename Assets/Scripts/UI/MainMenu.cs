using System.Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [SerializeField] Camera MenuCamera;
    [SerializeField] Transform ButtonPanel;
    [SerializeField] Transform SettingsPanel;
    [SerializeField] Transform MultiplayerPanel;
    [SerializeField] Transform StartSettingsPanel;
    private Quaternion LastRot;
    private Quaternion TargetRot;
    private float Lerp = 0;
    private const float LerpStep = 0.001f;

    public void Start()
    {
        Button_Back();
        TargetRot = MenuCamera.transform.rotation;
        LastRot = MenuCamera.transform.rotation;
    }

    public void FixedUpdate()
    {
        if (LastRot == TargetRot) return;
        if (Lerp > 1)
        {
            LastRot = MenuCamera.transform.rotation = TargetRot;
            Lerp = 0;
            Debug.Log("Lerping finished");
        }
        else
        {
            MenuCamera.transform.rotation = Quaternion.Lerp(LastRot, TargetRot, Lerp);
            Debug.Log("Lerping " + Lerp);
            Lerp += 0.1f;
        }
    }

    private void CloseAllPanels()
    {
        ButtonPanel.gameObject.SetActive(false);
        SettingsPanel.gameObject.SetActive(false);
        MultiplayerPanel.gameObject.SetActive(false);
        StartSettingsPanel.gameObject.SetActive(false);
    }

    public void TurnCam(int Page)
    {
        LastRot = MenuCamera.transform.rotation;
        switch (Page)
        {
            case 0: TargetRot = Quaternion.LookRotation(Vector3.up, Vector3.back); break;
            case 1: TargetRot = Quaternion.LookRotation(Vector3.forward, Vector3.up); break;
            case 2: TargetRot = Quaternion.LookRotation(Vector3.left, Vector3.up); break;
            case 3: TargetRot = Quaternion.LookRotation(Vector3.back, Vector3.up); break;
            case 4: TargetRot = Quaternion.LookRotation(Vector3.right, Vector3.up); break;
            default: TargetRot = Quaternion.LookRotation(Vector3.forward, Vector3.up); break;
        }              
    }

    public void Button_Quit(){ Application.Quit(); }

    public void Button_Settings()
    {
        CloseAllPanels();
        SettingsPanel.gameObject.SetActive(true);
    }

    public void Button_Create()
    {
        CloseAllPanels();
        StartSettingsPanel.gameObject.SetActive(true);
    }

    public void Button_FindServers()
    {
        CloseAllPanels();
        MultiplayerPanel.gameObject.SetActive(true);
    }

    public void Button_Back()
    {
        CloseAllPanels();
        ButtonPanel.gameObject.SetActive(true);
    }

    public void Start_Game()
    {
        SceneManager.UnloadSceneAsync(0);
        SceneManager.LoadSceneAsync(1);
    }

}
