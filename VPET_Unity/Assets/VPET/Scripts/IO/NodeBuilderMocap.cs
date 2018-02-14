using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{

	public class NodeBuilderMocap
	{
		public static GameObject BuildNode(ref SceneNode node, Transform parent, GameObject obj)
		{
	        if ( node.GetType() == typeof(SceneNodeMocap) )
	        {
				SceneNodeMocap nodeMocap = (SceneNodeMocap)Convert.ChangeType(node, typeof(SceneNodeMocap));
				return createMocap(nodeMocap, parent);
			}

			return null;
		}

        //!
        public static GameObject createMocap(SceneNodeMocap node, Transform parentTransform)
        {
            // setup as node
            GameObject objMain = NodeBuilderBasic.CreateNode(node, parentTransform);
            return objMain;
        }


	}
}