using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    public UDPListener dataSource;

    public AnimationCurve rCurve;
    public AnimationCurve gCurve;
    public AnimationCurve bCurve;

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
        float x = rCurve.Evaluate(param.x);
        float y = gCurve.Evaluate(param.y);
        float z = bCurve.Evaluate(param.z);

        animator.SetFloat("Red", x);
        animator.SetFloat("Green", y);
        animator.SetFloat("Blue", z);
    }
}
