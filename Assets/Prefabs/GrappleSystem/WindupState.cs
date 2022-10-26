using System;
using UnityEngine;

public class WindupState : StateMachineBehaviour
{
    private Animator stateMachine;
    public static EventHandler<EventArgs> onStateEnter;
    public static EventHandler<EventArgs> onStateUpdate;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.grappleReleased += TriggerThrow;
        stateMachine = animator;
        // grapplehook moves to throwing transform
        onStateEnter?.Invoke(this, EventArgs.Empty);
    }
    private void TriggerThrow(object sender, EventArgs e)
    {
        stateMachine.SetBool("Windup", false);
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onStateUpdate?.Invoke(this, EventArgs.Empty);
       // rotate grapplehook around axis
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       Player.grappleReleased -= TriggerThrow;
    }
    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
