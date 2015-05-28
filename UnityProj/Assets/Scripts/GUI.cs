using UnityEngine;
using Engine;
using System.Collections.Generic;

public class GUI : MonoBehaviour
{   
    public static GUI S { get; private set; }

    Canvas canvas;

    string hpBarPrefabName = "Prefabs/UI/HPBar";
    List<GameObject> hpBars = new List<GameObject>();    

    public bool showHPBars;
    public float hpBarsWidth;
    public GameObject cooldown;
    public GameObject[] gemCounts;

    public VariableFloat playerMana;
    public VariableFloat playerMaxMana;

    public StickyButton[] abilityButtons;

    public Variable<int> playerSelectedAbilitySpell;

    void Start ()
    {
        S = this;

        playerSelectedAbilitySpell = new Variable<int>(() => E.player.selectedAbilitySpell);

        canvas = GetComponent<Canvas>();
        //
        //bar.transform.SetParent(canvas.transform, false);
        //barTransform = bar.GetComponent<RectTransform>();

        Interfacing.ShowScrollWindow = ShowScrollWindow;

        if (E.player != null)
        {
            playerMana = new VariableFloat(() => E.player.mana);
            playerMaxMana = new VariableFloat(() => E.player.maxMana);
        }

        abilityButtons = new StickyButton[3];
        abilityButtons[0] = canvas.transform.FindChild("Abilities").FindChild("First").GetComponent<StickyButton>();
        abilityButtons[1] = canvas.transform.FindChild("Abilities").FindChild("Second").GetComponent<StickyButton>();
        abilityButtons[2] = canvas.transform.FindChild("Abilities").FindChild("Third").GetComponent<StickyButton>();

        for (int i = 0; i < 3; i++)
        {
            int ci = i;
            abilityButtons[i].PressedChanged += (obj, val) => { if (val) E.player.selectedAbilitySpell = ci; };
        }
    }
	
	void Update ()
    {
        #region HP bars
        if (Input.GetKeyDown(KeyCode.U))
        {
            showHPBars = !showHPBars;
        }
        
        if (showHPBars)
        {
            int barIdx = 0;
            foreach (var eg in CharacterGraphics.activeCharacters)
            {
                if (eg.entity is Player || eg.entity is IAvatarElement) continue;

                Vector3 worldBarPosition = new Vector3(eg.transform.position.x, eg.transform.position.y + eg.hpBarUpDistance, eg.transform.position.z);
                Vector3 screenBarPosition = Camera.main.WorldToScreenPoint(worldBarPosition);
                if (screenBarPosition.x >= 0 && screenBarPosition.x < Screen.width &&
                    screenBarPosition.y >= 0 && screenBarPosition.y < Screen.height)
                {
                    GameObject bar;
                    if (barIdx >= hpBars.Count)
                    {
                        bar = GObjFreelist.I.Get(hpBarPrefabName);
                        bar.transform.SetParent(canvas.transform, false);
                        hpBars.Add(bar);
                    }
                    else
                    {
                        bar = hpBars[barIdx];
                    }

                    var rect = bar.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector3(screenBarPosition.x - hpBarsWidth * 0.5f, screenBarPosition.y - Screen.height, -1);
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (eg.currentHP / eg.maxHP) * hpBarsWidth);

                    ++barIdx;
                }
            }

            for (int i = hpBars.Count - 1; i >= barIdx; --i)
            {
                GObjFreelist.I.Put(hpBars[i], hpBarPrefabName);
                hpBars.RemoveAt(hpBars.Count - 1);
            }
        }
        else
        {
            foreach (var bar in hpBars)            
                GObjFreelist.I.Put(bar, hpBarPrefabName);

            hpBars.Clear();
        }
        #endregion

        if (playerMana.IsNew || playerMaxMana.IsNew)
        {
            canvas.transform.FindChild("Mana").GetComponent<UnityEngine.UI.Slider>().value = playerMana.value / playerMaxMana.value;
        }

        if (playerSelectedAbilitySpell.IsNew)
        {
            if (playerSelectedAbilitySpell.value == -1)
            {
                foreach (var btn in abilityButtons)
                    btn.SetPressed(false);
            }
            else
            {
                abilityButtons[playerSelectedAbilitySpell.value].SetPressed(true);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (E.player.abilitySpells[i] != null)
                abilityButtons[i].GetComponentInChildren<UnityEngine.UI.Text>().text = E.player.abilitySpells[i].root.type.ToString().Substring(0, 1);
            else
                abilityButtons[i].GetComponentInChildren<UnityEngine.UI.Text>().text = string.Empty;
        }
    }

    public void ShowScrollWindow(Scroll scroll)
    {
        var scrollWindow = canvas.transform.FindChild("ScrollWindow").gameObject;
        var text = scrollWindow.transform.FindChild("Text").gameObject;
        text.GetComponent<UnityEngine.UI.Text>().text = scroll.text;
        text.SetActive(true);
        scrollWindow.SetActive(true);        
    }

    public void CloseScrollWindow()
    {
        var scrollWindow = canvas.transform.FindChild("ScrollWindow").gameObject;
        var text = scrollWindow.transform.FindChild("Text").gameObject;
        scrollWindow.SetActive(false);
        text.SetActive(false);       
    }

}
