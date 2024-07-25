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
        base.Heuristic(actionsOut);

        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[1] = Input.GetAxisRaw("Horizontal") switch
        {
            > 0 => 1,
            < 0 => -1,
            _ => 0
        };
    }

    /// <inheritdoc cref="OnActionReceived"/>
    /// <remarks>
    /// Determines what to do when receiving actions.
    /// </remarks>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActionsOut = actions.DiscreteActions;

        if (IsAgent)
        {
            // Agent plays with different controls and smoothing
            var targetVerticalVelocity = discreteActionsOut[0] switch
            {
                2 => Vector3.up * speed,
                1 => Vector3.down * speed,
                _ => Vector3.zero
            };

            // Agent plays with different controls and smoothing
            var targetHorizontalVelocity = discreteActionsOut[1] switch
            {
                2 => Vector3.forward * speed,
                1 => Vector3.back * speed,
                _ => Vector3.zero
            };

            var targetVelocity = targetVerticalVelocity + targetHorizontalVelocity;

            // Smoothly transition to the target velocity
            Rigidbody.velocity = Vector3.Lerp(
                Rigidbody.velocity, targetVelocity, smoothingFactor * Time.deltaTime
            );
        }
        else
        {
            var newVerticalVelocity = discreteActionsOut[0] switch
            {
                1 => Vector3.up * speed,
                -1 => Vector3.down * speed,
                _ => Vector3.zero
            };

            var newHorizontalVelocity = discreteActionsOut[1] switch
            {
                1 => Vector3.forward * speed,
                -1 => Vector3.back * speed,
                _ => Vector3.zero
            };

            Rigidbody.velocity = newVerticalVelocity + newHorizontalVelocity;
        }
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Override the OnCollisionEnter to remove the side hitting abuse from PaddleAgent.
    /// </remarks>
    /// <param name="other"></param>
    protected override void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.layer.Equals(BallLayer)) return;

        AddReward(1f);
    }
}