using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace vpet
{
    public class VPETOnBoardingModule : MonoBehaviour
    {
        public TMP_InputField ipInput;
        public TMP_InputField portInput;

        //left side context sensitive action bar with icon buttons
            //select camera
        //checkbox load from server
        //checkbox load textures?
        //dropdown choose roles
        //button connect/start
        
        private Core m_vpet;
        public Menu settingsMenu;

        public void Awake()
        {
            m_vpet = GameObject.Find("VPET").GetComponent<Core>();
            
            ipInput.text = "127.0.0.1";
            portInput.text = "5555";
        }
        
        public void send()
        {
            m_vpet.settings.isServer = true;
            SceneManager sceneManager = m_vpet.getManager<SceneManager>();
            NetworkManager networkManager = m_vpet.getManager<NetworkManager>();

            SceneParserModule sceneParserModule = sceneManager.getModule<SceneParserModule>();
            SceneSenderModule sceneSenderModule = networkManager.getModule<SceneSenderModule>();

            sceneParserModule.ParseScene();
            sceneSenderModule.sendScene(ipInput.text, portInput.text);
            
            settingsMenu.HideMenu();
        }

        public void receive()
        {
            m_vpet.settings.isServer = false;
            SceneManager sceneManager = m_vpet.getManager<SceneManager>();
            NetworkManager networkManager = m_vpet.getManager<NetworkManager>();
            networkManager.settings.m_serverIP = ipInput.text;

            SceneReceiverModule sceneReceiverModule = networkManager.getModule<SceneReceiverModule>();
            SceneCreatorModule sceneCreatorModule = sceneManager.getModule<SceneCreatorModule>();

            sceneReceiverModule.receiveScene(ipInput.text, portInput.text);
            
            settingsMenu.HideMenu();
        }
    }
}
