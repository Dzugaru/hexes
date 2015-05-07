using UnityEngine;
using System.Collections.Generic;

public class GUI : MonoBehaviour
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct GUIData
    {
        public float cooldownBarValue;
    }

    GUIData guiData;
    Canvas canvas;
    

    string hpBarPrefabName = "Prefabs/UI/HPBar";
    List<GameObject> hpBars = new List<GameObject>();    

    public bool showHPBars;
    public float hpBarsWidth;
    public GameObject cooldown;

    void Start ()
    {
        canvas = GetComponent<Canvas>();
        //
        //bar.transform.SetParent(canvas.transform, false);
        //barTransform = bar.GetComponent<RectTransform>();

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
                if (eg.entityType == (int)CharacterType.Player) continue;

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
                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (eg.info.currentHP / eg.info.maxHP) * hpBarsWidth);

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

        guiData = D.getGuiData();
        cooldown.SetActive(guiData.cooldownBarValue != 0);
        cooldown.GetComponent<UnityEngine.UI.Slider>().value = guiData.cooldownBarValue;
    }
}
