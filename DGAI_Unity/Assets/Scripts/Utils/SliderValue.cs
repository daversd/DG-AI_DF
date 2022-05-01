using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValue : MonoBehaviour
{
    TextMeshProUGUI _component;
    public void Awake()
    {
        _component = GetComponent<TextMeshProUGUI>();
        _component.text = transform.parent.GetComponent<Slider>().value.ToString();
    }
    public void UpdateText(float val)
    {
        GetComponent<TextMeshProUGUI>().text = val.ToString();
    }
}
