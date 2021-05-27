using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SceneSenderModule : NetworkManagerModule
    {
        private int m_responderId;
        public SceneSenderModule(string name) : base(name)
        {
            name = base.name;

            Dictionary<string, byte[]> responses = new Dictionary<string, byte[]>();
            SceneManager.SceneDataHandler dataHandler = ((SceneManager) manager.core.getManager(typeof(SceneManager))).sceneDataHandler;

            responses.Add("header", dataHandler.headerByteData);
            responses.Add("nodes", dataHandler.nodesByteData);
            responses.Add("objects", dataHandler.objectsByteData);
            responses.Add("characters", dataHandler.characterByteData);
            responses.Add("textures", dataHandler.texturesByteData);
            responses.Add("materials", dataHandler.materialsByteData);

            m_responderId = manager.addResponder(ref responses);
        }

        public void start(string ip, string port)
        {
            manager.startResponder(m_responderId, ip, port);
        }

        public void stop()
        {
            manager.stopResponder(m_responderId);
        }

    }
}
