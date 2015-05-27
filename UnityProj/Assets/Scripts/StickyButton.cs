using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class StickyButton : MonoBehaviour
{    
    public bool isPressed = false;    
    public bool isTextInsteadOfColor = false;
    public Color pressedColor = new Color(0.5f, 1f, 0.5f);
    public Color releasedColor = new Color(1f, 1f, 1f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f);

    public event Action<StickyButton, bool> PressedChanged;

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
        if (pressed == isPressed) return;

        isPressed = pressed;

        if (isTextInsteadOfColor)
        {
            if (isPressed) transform.GetChild(0).GetComponent<Text>().text = "x";
            else transform.GetChild(0).GetComponent<Text>().text = string.Empty;            
        }
        else
        {
            if (isPressed) GetComponent<Image>().color = pressedColor;
            else GetComponent<Image>().color = releasedColor;
        }

        if (PressedChanged != null)
            PressedChanged(this, isPressed);
    }

    public void SetDisabled(bool disabled)
    {
        if (disabled == !GetComponent<Button>().interactable) return;
        
        if (disabled)
        {
            if (isPressed)
                SetPressed(false);            
        }       

        GetComponent<Button>().interactable = !disabled;

    }
}


