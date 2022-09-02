using UnityEngine;
using UnityEngine.UI;

public class destroyMenu : MonoBehaviour
{
    private Button m_button;
    // Start is called before the first frame update
    void Start()
    {
        Transform button = transform.GetChild(0).Find("PanelMenu").Find("Button");
        m_button = button.GetComponent<Button>();
        m_button.onClick.AddListener(DestroyThis);
    }

    private void DestroyThis()
    {
        Destroy(transform.gameObject);
    }
}
