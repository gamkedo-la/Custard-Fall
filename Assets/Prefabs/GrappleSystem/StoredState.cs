using System;
using UnityEngine;
public class StoredState : StateMachineBehaviour
{
    public static EventHandler<EventArgs> onStateEnter;
    private Animator stateMachine;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"Grapple, StoredState");
        Player.grapplePressed += TriggerWindUp;
        stateMachine = animator;
        // listen for controller input event
        onStateEnter?.Invoke(this, EventArgs.Empty);
    }
    private void TriggerWindUp(object sender, EventArgs e)
    {
        stateMachine.SetBool("WindUp", true);
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        Player.grapplePressed -= TriggerWindUp;
    }
}
