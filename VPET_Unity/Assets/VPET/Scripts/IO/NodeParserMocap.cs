using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vpet
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class SceneNodeMocap : SceneNode
    {
    };

	public class NodeParserMocap
	{
		public static SceneNode ParseNode(NodeType nodeType, ref byte[] nodesByteData, ref int dataIdx)
		{
			if ( nodeType == NodeType.MOCAP)
			{
                        SceneNodeMocap sceneNodeMocap = SceneDataHandler.ByteArrayToStructure<SceneNodeMocap>(nodesByteData, ref dataIdx);
                        return sceneNodeMocap;
			}
			return null;
		}
	}
	
}
