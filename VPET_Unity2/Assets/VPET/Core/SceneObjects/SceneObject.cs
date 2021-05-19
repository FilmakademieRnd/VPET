using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET SceneObject, connecting Unity and VPET functionalty 
    //! around 3D scene specific objects.
    //!
    public class SceneObject : MonoBehaviour
    {
        //!
        //! unique id of this sceneObject
        //!
        public int id;

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
        public virtual void Start()
        {
            id = Helpers.getUniqueID();
            physicsActive = false;

            position = new Parameter<Vector3>();
            rotation = new Parameter<Quaternion>();
            scale = new Parameter<Vector3>();

            //position.hasChanged += printDebug;
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
        public virtual void Update()
        {
            // ToDo: implement a clever way to figure out when the transforms need to be updated 
            if(physicsActive)
                updateTransform();
        }

        //!
        //! updates the scene objects transforms and informs all connected parameters about the change
        //!
        private void updateTransform()
        {
            if (transform.position != position.value)
                position.value = transform.position;
            if (transform.rotation != rotation.value)
                rotation.value = transform.rotation;
            if (transform.localScale != scale.value)
                scale.value = transform.localScale;
        }
    }
}
