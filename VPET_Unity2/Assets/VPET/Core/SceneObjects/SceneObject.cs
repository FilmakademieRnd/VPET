using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class SceneObject : MonoBehaviour
    {
        //!
        //! unique id of this sceneObject
        //!
        private int id;

        //!
        //! is the sceneObject reacting to physics
        //!
        public bool physicsActive;

        //!
        //! Position of the SceneObject
        //!
        private Parameter<Vector3> position;
        //!
        //! Rotation of the SceneObject
        //!
        private Parameter<Quaternion> rotation;
        //!
        //! Scale of the SceneObject
        //!
        private Parameter<Vector3> scale;


        //!
        //! Start is called before the first frame update
        //!
        void Start()
        {
            id = Helpers.getUniqueID();
            physicsActive = false;
            position = new Parameter<Vector3>();
            position.hasChanged += printDebug;

        }

        //!
        //! Debug print of a Vector3 parameter
        //! @param   sender     Object calling the print function
        //! @param   a          Values to be passed to the print function
        //!
        private void printDebug(object sender, Parameter<Vector3>.TEventArgs a)
        {
            Debug.Log(a.value);
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            if(physicsActive)
                updateTransform();
        }

        //!
        //! updates the scene objects transforms and informs all connected parameters about the change
        //!
        private void updateTransform()
        {
            if (transform.position != position.value)
                position.setValue(transform.position);
            /*if (transform.rotation != rotation.value)
                rotation.setValue(transform.rotation);
            if (transform.localScale != scale.value)
                scale.setValue(transform.localScale);*/
        }
    }
}
