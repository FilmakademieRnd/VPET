using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace vpet
{
    public class TextHighlighter : MonoBehaviour
    {
        private RectTransform rect, refRect;
        private TextMeshProUGUI txt;
        private SnapSelect snapSelect;

        // Start is called before the first frame update
        void Start()
        {
            rect = this.GetComponent<RectTransform>();
            refRect = this.transform.parent.parent.GetComponent<RectTransform>();
            snapSelect = this.transform.parent.parent.parent.GetComponent<SnapSelect>();
            txt = this.GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            float dist = Vector2.Distance(rect.position, refRect.position);
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f - ((dist / snapSelect.fadeFactor)));
            /*if (dist < 50)
            {
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f);
            }
            else
            {
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f-((dist/1000f)));
            }*/
        }
    }
}
