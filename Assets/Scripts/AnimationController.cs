using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    public UDPListener dataSource;

    // Curves used to control output values given an input
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
        float x = rCurve.Evaluate(param.x);
        float y = gCurve.Evaluate(param.y);
        float z = bCurve.Evaluate(param.z);

        // Set animator parameters
        animator.SetFloat("Red", x);
        animator.SetFloat("Green", y);
        animator.SetFloat("Blue", z);
    }
}
