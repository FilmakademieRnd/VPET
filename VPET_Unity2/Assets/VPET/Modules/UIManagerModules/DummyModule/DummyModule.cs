using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace vpet
{
    public class DummyModule : MonoBehaviour
    {
        private VPET m_vpet;

        public void Awake()
        {
            m_vpet = GameObject.Find("VPET").GetComponent<VPET>();
        }
        public void send()
        {
            SceneManager sceneManager = (SceneManager) m_vpet.getManager(typeof(SceneManager));
            NetworkManager networkManager = (NetworkManager) m_vpet.getManager(typeof(NetworkManager));

            SceneParserModule sceneParserModule = (SceneParserModule) sceneManager.getModule(typeof(SceneParserModule));
            SceneSenderModule sceneSenderModule = (SceneSenderModule) networkManager.getModule(typeof(SceneSenderModule));

            sceneParserModule.ParseScene();
            sceneSenderModule.sendScene("172.18.2.101", "5555");
        }

        public void receive()
        {
            SceneManager sceneManager = (SceneManager)m_vpet.getManager(typeof(SceneManager));
            NetworkManager networkManager = (NetworkManager)m_vpet.getManager(typeof(NetworkManager));

            SceneReceiverModule sceneReceiverModule = (SceneReceiverModule) networkManager.getModule(typeof(SceneReceiverModule));
            SceneCreatorModule sceneCreatorModule = (SceneCreatorModule) sceneManager.getModule(typeof(SceneCreatorModule));

            sceneReceiverModule.receiveScene("172.18.1.177", "5555");
        }
    }
}
