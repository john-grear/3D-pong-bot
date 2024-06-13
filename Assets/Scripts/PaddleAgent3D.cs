using Unity.MLAgents.Actuators;
using UnityEngine;

public class PaddleAgent3D : PaddleAgent
{
    /// <inheritdoc cref="Heuristic"/>
    /// <remarks>
    /// Manually control the agent using vertical and horizontal input to control the paddle.
    /// </remarks>
    /// <param name="actionsOut">
    /// What stores the actions to be used.
    /// </param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxisRaw("Vertical");
        continuousActionsOut[1] = Input.GetAxisRaw("Horizontal");
    }

    /// <inheritdoc cref="OnActionReceived"/>
    /// <remarks>
    /// Determines what to do when receiving actions.
    /// </remarks>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActionsOut = actions.ContinuousActions;

        var directionToMove = continuousActionsOut[0] switch
        {
            > 0 => Vector3.up,
            < 0 => Vector3.down,
            _ => Vector3.zero
        };

        directionToMove += continuousActionsOut[1] switch
        {
            > 0 => Vector3.forward,
            < 0 => Vector3.back,
            _ => Vector3.zero
        };

        Rigidbody.velocity = directionToMove.normalized * speed;

        // Penalize paddle for moving to incentivize efficient movement to hit the ball
        if (Rigidbody.velocity != Vector3.zero)
        {
            AddReward(-0.01f);
        }
    }
}