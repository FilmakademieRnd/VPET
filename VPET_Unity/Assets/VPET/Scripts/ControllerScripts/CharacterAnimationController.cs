using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace vpet
{
    public class CharacterAnimationController : MonoBehaviour
    {
        private Animator animator;
        public Quaternion[] animationState;

        // Start is called before the first frame update
        void Start()
        {
            animator = this.GetComponent<Animator>();
            animationState = new Quaternion[25];
        }

        // Update is called once per frame
        void OnAnimatorIK(int layerIndex)
        {
            for(int i = 0; i < 25; i++)
            {
                animator.SetBoneLocalRotation((HumanBodyBones)i, animationState[i]);
            }
        }
    }
}
