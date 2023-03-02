using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    public UDPListener dataSource;

    private Animator animator;
    private Vector3 animationParameters = Vector3.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
        try
        {
            dataSource.DataReceived += SetAnimationParameters;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void Update()
    {
        ModifyAnimation(animationParameters);
    }

    void SetAnimationParameters(object sender, Vector3 param)
    {
        animationParameters = param;
    }

    void ModifyAnimation(Vector3 param)
    {
        animator.SetFloat("Red", param.x);
        animator.SetFloat("Green", param.y);
        animator.SetFloat("Blue", param.z);
    }
}
