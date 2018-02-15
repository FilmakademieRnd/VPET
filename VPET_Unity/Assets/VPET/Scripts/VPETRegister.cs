using System.Collections;
using System.Collections.Generic;

namespace vpet
{
	// Basic 
	// public enum NodeType { GROUP, GEO, LIGHT, CAMERA}

	// Basic, Mocap
	public enum NodeType { GROUP, GEO, LIGHT, CAMERA, MOCAP }

	public static class VPETRegister  
	{
		public static void RegisterNodeParser()
		{
			SceneDataHandler.RegisterDelegate(NodeParserBasic.ParseNode);
			SceneDataHandler.RegisterDelegate(NodeParserMocap.ParseNode);
		}

		public static void RegisterNodeBuilder()
		{
			SceneLoader.RegisterDelegate(NodeBuilderBasic.BuildNode);
			SceneLoader.RegisterDelegate(NodeBuilderMocap.BuildNode);
		}

		public static void RegisterObjectSender()
		{
			ServerAdapter.RegisterSender(ObjectSenderBasic.Instance);
			ServerAdapter.RegisterSender(ObjectSenderKatana.Instance);
		}


	}
}
