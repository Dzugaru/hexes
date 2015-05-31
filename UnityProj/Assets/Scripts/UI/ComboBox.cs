using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("UI/ComboBox", 31)]
public class ComboBox : Selectable, IEventSystemHandler, IPointerClickHandler
{
    public GameObject itemPrefab;
    public float itemHeight;

    [Serializable]
    public class ComboBoxEvent : UnityEvent<string>
    {
    }

    [SerializeField, Space(6f)]
    private ComboBoxEvent m_OnValueChanged = new ComboBoxEvent();

    GameObject items;
    Text selText;  
    string selectedValue;
    bool isExpanded;
    bool isInitted;

    void Init()
    {
        if (!isInitted)
        {
            isInitted = true;
            items = transform.Find("items").gameObject;
            selText = transform.Find("sel_txt").GetComponent<Text>();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Init();
    }  

    public void OnPointerClick(PointerEventData eventData)
    {
        SetExpanded(!isExpanded);        
    }

    public void SetExpanded(bool exp)
    {
        isExpanded = exp;
        items.SetActive(isExpanded);
    }

    public void SetSelectedValue(string val)
    {
        if (selectedValue != val)
        {
            selectedValue = val;
            selText.text = val;
            m_OnValueChanged.Invoke(val);
        }
    }

    public void SetItems(params string[] items)
    {
        Init();
        float ypos = 0;
        foreach (var item in items)
        {
            var cbItem = Instantiate(itemPrefab).GetComponent<ComboBoxItem>();
            cbItem.transform.SetParent(this.items.transform, false);
            cbItem.GetComponent<RectTransform>().localPosition = new Vector3(0, ypos, 0);
            ypos -= itemHeight;
            cbItem.text.text = item;
            cbItem.parent = this;
        }

        SetSelectedValue(items[0]);
    }

    public void SetItemsByEnum(Type enumType)
    {
        SetItems(Enum.GetNames(enumType));
    }
}


