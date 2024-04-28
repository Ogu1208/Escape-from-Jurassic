using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackIsDone : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController attackIsDonePlayer = animator.GetComponentInParent<PlayerController>();
        SlimeAI attackIsDoneSlime = animator.GetComponent<SlimeAI>();
        TurtleAI attackIsDoneTurtle = animator.GetComponent<TurtleAI>();
        
        if (attackIsDonePlayer != null)
        {
            attackIsDonePlayer.AttackIsDone();
        }

        if (attackIsDoneSlime != null)
        {
            attackIsDoneSlime.AttackIsDone();
        }

        if (attackIsDoneTurtle != null)
        {
            attackIsDoneTurtle.AttackIsDone();
        }
    }
}
