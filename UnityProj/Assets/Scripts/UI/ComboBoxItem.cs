using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class ComboBoxItem : Selectable, IEventSystemHandler, IPointerClickHandler
{
    public ComboBox parent;
    public Text text;

    public void OnPointerClick(PointerEventData eventData)
    {
        parent.SetSelectedValue(text.text);
        parent.SetExpanded(false);
    }

    public void OnSubmit(BaseEventData eventData)
    {

    }

    protected override void Awake()
    {
        base.Awake();

        text = transform.Find("Text").GetComponent<Text>();
    }
}
