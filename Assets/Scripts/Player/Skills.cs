using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public struct Skill_Button
    {
        public Button Btn;
        public Text Txt;
        public Image Img;
    }
       
    public class Skill
    {
        public string Name;
        public float CooldownTime;
        private float timeleft;
        public float TimeLeft { get { return timeleft; } set { timeleft = Mathf.Clamp(value, 0, CooldownTime); } }
        public delegate bool Action(Skill self, Transform player);
        public Action Perform;
        public Skill(string Name, float C_Time, Action A) { this.Name = Name; CooldownTime = TimeLeft = C_Time; Perform = A; }
    }

    private Skill[] SkillList =
    {
        new Skill("Dash", 5, (Skill S, Transform T) =>
        {
            if (S.TimeLeft != 0) return false;
            T.localPosition += T.forward * 10;
            S.TimeLeft = S.CooldownTime;
            return true;
        }),
        new Skill("Revive", 0, (Skill S, Transform T) =>
        {
            Player P = T.GetComponent<Player>();
            if (P.isAlive() || S.TimeLeft != 0) return false;
            P.ChangeStatus(1);
            S.CooldownTime += 30;
            S.TimeLeft = S.CooldownTime;
            return true;
        }),
        new Skill("None", 1, (Skill S, Transform T) => { return true; })
    };


    [SerializeField] private Player myPlayer;
    [SerializeField] private Button[] Buttons = new Button[3];
    private Skill_Button[] SkillButtons = new Skill_Button[3];
    public int[] ChosenSkills = new int[3];
    


    public void Start()
    {
        for (int i = 0; i < SkillButtons.Length; i++)
        {
            SkillButtons[i].Btn = Buttons[i];
            SkillButtons[i].Txt = Buttons[i].GetComponentInChildren<Text>();
            SkillButtons[i].Img = Buttons[i].transform.GetChild(0).GetComponent<Image>();
           // SkillButtons[i].Btn.onClick.AddListener(() => { SkillList[i].Perform(SkillList[i], myPlayer.transform); });
        }
    }

    public  void Btn_Click(int i) { SkillList[ChosenSkills[i]].Perform(SkillList[ChosenSkills[i]], myPlayer.transform); }

    public void FixedUpdate()
    {
        for (int i = 0; i < ChosenSkills.Length; i++)
        {
            Skill S = SkillList[ChosenSkills[i]];
            S.TimeLeft -= Time.fixedDeltaTime;
            string NewText = Mathf.RoundToInt(100 * S.TimeLeft).ToString();
            SkillButtons[i].Btn.enabled = (S.TimeLeft == 0);
            Text T = SkillButtons[i].Txt;
            T.enabled = !(S.TimeLeft == 0);
            T.text = NewText.Insert(Mathf.Clamp(NewText.Length - 2, 0, int.MaxValue), ".");
            T.fontSize = 30 + Mathf.RoundToInt(50 * (1 - S.TimeLeft / S.CooldownTime));
            SkillButtons[i].Img.fillAmount = S.TimeLeft / S.CooldownTime;
            if (Input.GetKeyDown(myPlayer.getControls().getKey("Skill_" + (i + 1).ToString()))) S.Perform(S, myPlayer.transform);
        }
    }


}    
    
