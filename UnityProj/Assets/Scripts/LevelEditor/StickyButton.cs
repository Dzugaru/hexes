using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class StickyButton : MonoBehaviour
{    
    public bool isPressed = false;
    public Color pressedColor = new Color(0.5f, 1f, 0.5f);
    public Color releasedColor = new Color(1f, 1f, 1f);

    public event Action<bool> PressedChanged;

    void OnEnable()
    {
        SetPressed(isPressed);
    }

    public void OnClick()
    {
        if (!isPressed)
        {
            foreach (var btn in GameObject.FindGameObjectsWithTag(gameObject.tag))
            {
                if (btn.GetComponent<StickyButton>().isPressed)
                    btn.GetComponent<StickyButton>().SetPressed(false);
            }

            SetPressed(true);
        }
        else
        {
            SetPressed(false);
        }
    }

    public virtual void SetPressed(bool pressed)
    {
        isPressed = pressed;
        if (isPressed) GetComponent<Image>().color = pressedColor;
        else GetComponent<Image>().color = releasedColor;

        if (PressedChanged != null)
            PressedChanged(isPressed);
    }
}

