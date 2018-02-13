using System.Collections;
using System.Collections.Generic;

namespace vpet
{
	// Basic 
	// public enum NodeType { GROUP, GEO, LIGHT, CAMERA}

	// Basic, Mocap
	public enum NodeType { GROUP, GEO, LIGHT, CAMERA, MOCAP }


	public class VPETRegister  
	{
		public static void RegisterNodeParser()
		{
			SceneDataHandler.RegisterDelegate(NodeParserBasic.ParseNode);
		}
	}
}
