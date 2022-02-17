using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoElementUpdate : MonoBehaviour
{
    private float m_lineWidth = 1.0f;
    private float m_oldDepth = 0.0f;
    private LineRenderer m_lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        m_lineRenderer = transform.gameObject.GetComponent<LineRenderer>();
        m_lineWidth = m_lineRenderer.startWidth;
    }

    // Update is called once per frame
    void Update()
    {
        float depth = Vector3.Dot(Camera.main.transform.position - transform.position, Camera.main.transform.forward);

        if (m_oldDepth != depth)
        {
            m_lineRenderer.startWidth = m_lineWidth * depth;
            m_lineRenderer.endWidth = m_lineWidth * depth;
            m_oldDepth = depth;
        }
    }
}
