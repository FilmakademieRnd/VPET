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
        private bool physicsActive;

        //!
        //! Position of the SceneObject
        //!
        private Parameters.Vec3 position;
        //!
        //! Rotation of the SceneObject
        //!
        private Parameters.Quat rotation;
        //!
        //! Scale of the SceneObject
        //!
        private Parameters.Vec3 scale;

        //!
        //! Event to communicate that the sceneObject position changed
        //!
        private event EventHandler<Parameters.Vec3.changeEventArgs> positionChanged;
        //!
        //! Event to communicate that the sceneObject rotation changed
        //!
        private event EventHandler<Parameters.Quat.changeEventArgs> rotationChanged;
        //!
        //! Event to communicate that the sceneObject scale changed
        //!
        private event EventHandler<Parameters.Vec3.changeEventArgs> scaleChanged;


        //!
        //! Start is called before the first frame update
        //!
        void Start()
        {
            id = Helpers.getUniqueID();
            physicsActive = false;

            position.connectToChangeValue(positionChanged);
            rotation.connectToChangeValue(rotationChanged);
            scale.connectToChangeValue(scaleChanged);
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
                positionChanged(this,new Parameters.Vec3.vec3EventArgs { _value = transform.position});
            if (transform.rotation != rotation.value)
                rotationChanged(this, new Parameters.Quat.quatEventArgs { _value = transform.rotation });
            if (transform.localScale != scale.value)
                scaleChanged(this, new Parameters.Vec3.vec3EventArgs { _value = transform.localScale });
        }
    }
}
