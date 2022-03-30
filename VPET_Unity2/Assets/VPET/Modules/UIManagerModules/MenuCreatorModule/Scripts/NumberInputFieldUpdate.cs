using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class NumberInputFieldUpdate : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private static readonly int m_sections = 3;
    private static readonly int m_sectionscale = 10;
    private float m_startVal = 0;
    private Vector2 m_startPos = Vector2.zero;
    private TMP_InputField m_inputField;
    private Canvas m_canvas;
    void Awake()
    {
        m_inputField = GetComponent<TMP_InputField>();
        m_canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_startPos = eventData.position;
        m_startVal = float.Parse(m_inputField.text);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_inputField.DeactivateInputField(true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_inputField.DeactivateInputField(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scale = Mathf.Floor(((eventData.position.y - m_startPos.y) / Screen.height) * m_sections) * m_sectionscale;  
        
        if (scale < 0)
            scale = 1f / Mathf.Abs(scale);
        else if (scale == 0)
            scale += 1;

        m_inputField.text = (m_startVal + (eventData.position.x - m_startPos.x) * scale/m_canvas.scaleFactor).ToString();
        //m_inputField.text = scale.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
