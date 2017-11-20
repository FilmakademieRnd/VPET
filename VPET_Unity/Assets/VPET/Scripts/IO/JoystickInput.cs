/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using System.Collections;

//!
//! script receiving input from one or two (vcs) joystick
//!
namespace vpet
{
    public class JoystickInput : MonoBehaviour
    {
        public float speed = 100f;
        public float speedFov = 1f;
        public float fov = 30f;
        public float aspect = 1.77777f;

        private float left, right, bottom, top;

        private ServerAdapter serverAdapter;

        private float x_axis = 0f;
        private float y_axis = 0f;

        public Vector3 getTranslation()
        {
            x_axis = Input.GetAxis("Horizontal2") * speed;
            y_axis = Input.GetAxis("Vertical2") * speed;

            Vector3 pos = Vector3.zero;

            if (Input.GetButton("Fire2")) //  x z
            {
                pos = new Vector3(0, y_axis, 0);
                //print( "Move X Z. Pos: " + pos.ToString() );
            }
            else if (Input.GetButton("Fire3")) // fov
            {
                fov = Mathf.Clamp(fov + y_axis * Time.deltaTime * speedFov, 1f, 70f);
                print("Fov: " + fov.ToString());
                serverAdapter.sendFov(fov, left, right, bottom, top);
            }
            else // y
            {
                pos = new Vector3(x_axis, 0, -y_axis);
                //print("Move Y. Pos: " + pos.ToString());
            }

            return pos * Time.deltaTime;
        }

        //!
        //! Use this for initialization
        //!
        void Start()
        {
            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();

            left = -1.0f * aspect;
            right = 1.0f * aspect;
            bottom = -1.0f;
            top = 1.0f;
        }


        //// Update is called once per frame
        //void Update_()
        //{
        //    if (sceneObject != null)
        //    {
        //        x_axis = Input.GetAxis("Horizontal2") * speed;
        //        y_axis = Input.GetAxis("Vertical2") * speed;

        //        //if (x_axis != 0)
        //        //    print("Horizontal:" + x_axis);
        //        //if (y_axis != 0)
        //        //    print("Vertical:" + y_axis);

        //        if ((Mathf.Abs(x_axis) + Mathf.Abs(y_axis)) > threshold)
        //        {
        //            Vector3 pos = Vector3.zero;

        //            if (Input.GetButton("Fire2")) //  x z
        //            {
        //                pos = new Vector3(-x_axis, 0, -y_axis);
        //                //print( "Move X Z. Pos: " + pos.ToString() );
        //            }
        //            else if (Input.GetButton("Fire3")) // fov
        //            {
        //                fov = Mathf.Clamp(fov + y_axis * Time.deltaTime * speedFov, 1f, 70f);
        //                //print("Fov: " + fov.ToString());
        //                serverAdapter.sendFov(fov, left, right, bottom, top);
        //            }
        //            else // y
        //            {
        //                pos = new Vector3(0, y_axis, 0);
        //                //print("Move Y. Pos: " + pos.ToString());
        //            }

        //            pos = worldTransform.localPosition + pos * Time.deltaTime;
        //            sceneObject.translate(pos);
        //        }

        //        //if ( Mathf.Abs(y_axis2) > threshold)
        //        //{
        //        //    speed = Mathf.Clamp(speed+-y_axis2*Time.deltaTime*30f, 0.1f, 2000f);
        //        //    //print("Speed: " + speed.ToString());
        //        //}

        //        // zoom
        //        //if ( Mathf.Abs( x_axis2 ) > 10*threshold )
        //        //{
        //        //    fov = Mathf.Clamp(fov + x_axis2 * Time.deltaTime * speed * 0.01f , 10f, 70f);
        //        //    //print( "Fov: " + fov.ToString() );
        //        //    serverAdapter.sendFov( fov, left, right, bottom, top );

        //        //    //float scale = Mathf.Max( 0.01f, worldTransform.localScale.x + x_axis2 * speedScale * Time.deltaTime );
        //        //    //sceneObject.scale( new Vector3(scale,scale, scale) );
        //        //}


        //        /*
        //        if (Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.R))
        //        {
        //            print("Record");
        //        }


        //        if (Input.GetButton("Fire3") )
        //        {
        //            print("Fire3");
        //        }
        //        */
        //    }
        //}

    }
}