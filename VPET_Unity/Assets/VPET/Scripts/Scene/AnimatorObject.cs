using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorObject : MonoBehaviour
{

    public string TriggerName = "";

    public Animator animator;

    public Transform skeletonRoot;
    public Transform modelGroup;

    void Awake()
    {
        if (!animator) transform.GetComponent<Animator>();
    }

    // Use this for initialization
    void Start ()
    {
        Trigger();
        Stop();
	}
	
    public void Trigger()
    {
        if (TriggerName != "")
        {
            Play();
            animator.SetTrigger(TriggerName);
            Stop();
        }
    }

    public void Play()
    {
        animator.StopPlayback();
    }

    public void Stop()
    {
        animator.StartPlayback();
    }    

}
