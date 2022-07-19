/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "NumberInputFieldUpdate.cs"
//! @brief Implementation of the VPET NumberInputFieldUpdate component, updating number values on swipes.
//! @author Simon Spielmann
//! @version 0
//! @date 01.04.2022

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

//!
//! Implementation of the VPET NumberInputFieldUpdate component, updating number values on swipes.
//!
public class NumberInputFieldUpdate : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //!
    //! Variable defining the number of screen sections for value scaling.
    //!
    private static readonly int m_sections = 3;
    //!
    //! Variable defining the value scaling for a section.
    //!
    private static readonly int m_sectionscale = 10;
    //!
    //! The start value of the input field at first pointer down.
    //!
    private float m_startVal = 0;

    //!
    //! magnitude of the value
    //!
    private float m_magnitude;
    //!
    //! The start posion of the pointer at pointer down.
    //!
    private Vector2 m_startPos = Vector2.zero;
    //!
    //! A reference to the scripts number input field.
    //!
    private TMP_InputField m_inputField;
    //!
    //! A reference to the screen canvas.
    //!
    private Canvas m_canvas;
    
    //!
    //! Early init.
    //!
    void Awake()
    {
        m_inputField = GetComponent<TMP_InputField>();
        m_canvas = GetComponentInParent<Canvas>();
    }

    //!
    //! Function callen when pointer goes down.
    //!
    public void OnPointerDown(PointerEventData eventData)
    {
        m_startPos = eventData.position;
        m_startVal = float.Parse(m_inputField.text);
        m_magnitude = (int)(Mathf.Log10(Mathf.Max(Mathf.Abs(m_startVal), 0.5f)) + 1f);
        m_inputField.shouldHideSoftKeyboard = true;
    }

    //!
    //! Function called when dragging begins.
    //!
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_inputField.DeactivateInputField(true);
    }

    //!
    //! Function called when dragging ends.
    //!
    public void OnEndDrag(PointerEventData eventData)
    {
        m_inputField.onEndEdit?.Invoke(m_inputField.text);
    }

    //!
    //! Function called when clicked
    //!
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        m_inputField.shouldHideSoftKeyboard = false;
    }


    //!
    //! Function called during drag updates.
    //!
    public void OnDrag(PointerEventData eventData)
    {
        float scale = Mathf.Floor(((eventData.position.y - m_startPos.y) / Screen.height) * m_sections) * m_sectionscale;
        if (scale < 0)
            scale = 0.01f / Mathf.Abs(scale);
        else if (scale == 0)
            scale += 0.01f;
        m_inputField.text = (m_startVal + (eventData.position.x - m_startPos.x) * scale * (1f+m_magnitude)).ToString();
    }
}
