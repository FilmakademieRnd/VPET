using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class Editor_UIModeChange : MonoBehaviour
    {
        private Core m_vpet;

        //UICreator3DModule UI3DModule;

        public void Awake()
        {
            m_vpet = GameObject.Find("VPET").GetComponent<Core>();
        }

        public void SetModeT()
        {
            UIManager uiMgr = m_vpet.getManager<UIManager>();
            UICreator3DModule UI3DModule = uiMgr.getModule<UICreator3DModule>();
            UI3DModule.SetModeT();
        }

        public void SetModeR()
        {
            UIManager uiMgr = m_vpet.getManager<UIManager>();
            UICreator3DModule UI3DModule = uiMgr.getModule<UICreator3DModule>();
            UI3DModule.SetModeR();
        }

        public void SetModeS()
        {
            UIManager uiMgr = m_vpet.getManager<UIManager>();
            UICreator3DModule UI3DModule = uiMgr.getModule<UICreator3DModule>();
            UI3DModule.SetModeS();
        }
    }
}