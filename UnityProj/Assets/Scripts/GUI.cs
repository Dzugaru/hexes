﻿using UnityEngine;
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

    void Start ()
    {
        S = this;

        canvas = GetComponent<Canvas>();
        //
        //bar.transform.SetParent(canvas.transform, false);
        //barTransform = bar.GetComponent<RectTransform>();

        Interfacing.ShowScrollWindow = ShowScrollWindow;
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
                if (eg.entityType == (int)CharacterType.Player || eg.entityType == (int)CharacterType.AvatarLearn) continue;

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
       
        cooldown.SetActive(Engine.GUIData.cooldownBarValue != 0);
        cooldown.GetComponent<UnityEngine.UI.Slider>().value = Engine.GUIData.cooldownBarValue;

        gemCounts[0].GetComponent<UnityEngine.UI.Text>().text = Engine.GUIData.fireGemsCount.ToString();
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
