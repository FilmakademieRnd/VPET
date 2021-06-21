using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace vpet
{
    public class DummyModule : MonoBehaviour
    {
        private Core m_vpet;

        public void Awake()
        {
            m_vpet = GameObject.Find("VPET").GetComponent<Core>();
        }
        public void send()
        {
            SceneManager sceneManager = m_vpet.getManager<SceneManager>();
            NetworkManager networkManager = m_vpet.getManager<NetworkManager>();

            SceneParserModule sceneParserModule = sceneManager.getModule<SceneParserModule>();
            SceneSenderModule sceneSenderModule = networkManager.getModule<SceneSenderModule>();

            sceneParserModule.ParseScene();
            sceneSenderModule.sendScene("127.0.0.1", "5555");
        }

        public void receive()
        {
            SceneManager sceneManager = m_vpet.getManager<SceneManager>();
            NetworkManager networkManager = m_vpet.getManager<NetworkManager>();

            SceneReceiverModule sceneReceiverModule = networkManager.getModule<SceneReceiverModule>();
            SceneCreatorModule sceneCreatorModule = sceneManager.getModule<SceneCreatorModule>();

            sceneReceiverModule.receiveScene("127.0.0.1", "5555");
        }
    }
}
