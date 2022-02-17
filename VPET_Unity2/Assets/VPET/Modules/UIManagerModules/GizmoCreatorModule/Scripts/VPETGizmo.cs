using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class VPETGizmo
    {
        private GameObject m_root;
        private GameObject m_GizmoElementPrefab;
        private List<GameObject> m_GizmoElements;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the VPET core
        //!
        public VPETGizmo(string name, Transform parent = null)
        {
            m_GizmoElementPrefab = Resources.Load("Prefabs/GizmoElement") as GameObject;
            m_GizmoElements = new List<GameObject>();
            m_root = new GameObject(name);

            if (parent != null)
            {
                m_root.transform.position = parent.transform.position;
                m_root.transform.rotation = parent.transform.rotation;
                m_root.transform.localScale = parent.transform.localScale;

                m_root.transform.parent = parent;
            }

        }

        ~VPETGizmo()
        {
            dispose();
        }

        public void dispose()
        {
            foreach (GameObject gizmoElement in m_GizmoElements)
                Object.Destroy(gizmoElement);
            Object.Destroy(m_root);
            m_GizmoElements.Clear();
        }

        public Transform addElement(ref Vector3[] positions, Color color, bool loop = false)
        {
            GameObject gizmoElement = GameObject.Instantiate(m_GizmoElementPrefab, m_root.transform);
            LineRenderer lineRenderer = gizmoElement.GetComponent<LineRenderer>();

            lineRenderer.positionCount = positions.Length;
            lineRenderer.loop = loop;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.SetPositions(positions);
            m_GizmoElements.Add(gizmoElement);

            return gizmoElement.transform;
        }

        public void removeElement(GameObject element)
        {
            m_GizmoElements.Remove(element);
            Object.Destroy(element);
        }

        public void setColor(object sender, Color color)
        {
            foreach (GameObject gameObject in m_GizmoElements)
            {
                LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }

    }
}
