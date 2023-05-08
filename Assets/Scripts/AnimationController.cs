using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    public UDPListener dataSource;

    // Curves used to control output values given an input
    public AnimationCurve f1Curve;
    public AnimationCurve f2Curve;
    public AnimationCurve f3Curve;

    private Animator animator;
    private Vector3 animationParameters = Vector3.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
        try
        {
            dataSource.DataReceived += SetAnimationParameters; // Listen for data from the server, use it to adjust animation rig
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void Update()
    {
        ModifyAnimation(animationParameters); // Animator parameters can only be changed on the main thread, so it is done here
    }

    // Buffer animation parameters to be handled in the main thread
    void SetAnimationParameters(object sender, Vector3 param)
    {
        animationParameters = param;
    }

    // Adjust animation parameters
    void ModifyAnimation(Vector3 param)
    {
        // Final parameter values are input into curves for better control
        float x = f1Curve.Evaluate(param.x);
        float y = f2Curve.Evaluate(param.y);
        float z = f3Curve.Evaluate(param.z);

        // Set animator parameters
        animator.SetFloat("feature1", x);
        animator.SetFloat("feature2", y);
        animator.SetFloat("feature3", z);
    }
}
