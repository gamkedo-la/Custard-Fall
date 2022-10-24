using System;
using UnityEngine;
public class StoredState : StateMachineBehaviour
{
    public static EventHandler<EventArgs> onStateEnter;
    private Animator stateMachine;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"Grapple, StoredState");
        Player.grappleWindup += TriggerWindUp;
        stateMachine = animator;
        // put the grapple hook away
        // listen for controller input event
        onStateEnter?.Invoke(this, EventArgs.Empty);
    }
    private void TriggerWindUp(object sender, EventArgs e)
    {
        stateMachine.SetTrigger("WindUp");
    }
    private void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        Player.grappleWindup -= TriggerWindUp;
    }
}
