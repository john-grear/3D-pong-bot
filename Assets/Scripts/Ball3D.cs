using UnityEngine;
using Random = UnityEngine.Random;

public class Ball3D : Ball
{
    /// <summary>
    /// Randomly generates a starting vector for the 3D ball.
    /// </summary>
    /// <returns>
    /// Vector either going left or right, with a random y-axis and z-axis angle added.
    /// </returns>
    protected override Vector3 ChooseStartVector()
    {
        // Choose left or right
        var randomXDirection = Random.Range(0, 2) == 0 ? -speed : speed;
        var randomYDirection = Random.Range(0, 2) == 0 ? -1 : 1 * Random.Range(2f, 8f);
        var randomZDirection = Random.Range(0, 2) == 0 ? -1 : 1 * Random.Range(2f, 8f);

        // Add random angle
        return new Vector3(randomXDirection, randomYDirection, randomZDirection);
    }

    /// <inheritdoc cref="CheckVectorSpeedLimit"/>
    /// <remarks>
    /// Checks the current velocity x, y, and z and ensures they are within 90% and 110% of the
    /// ball speed.
    /// </remarks>
    /// <returns>
    /// Vector that is created after limiting the speed.
    /// </returns>
    protected override Vector3 CheckVectorSpeedLimit()
    {
        // Check speed limit and adjust appropriately
        var vectorWithoutYAxisChecked = base.CheckVectorSpeedLimit();

        var verticalMovement = Mathf.Abs(Rigidbody.velocity.y);

        var newY = vectorWithoutYAxisChecked.y;

        // Adjust Y-Axis speed
        if (verticalMovement < speed * 0.9f || verticalMovement > speed * 1.1f)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newY = Mathf.Sign(vectorWithoutYAxisChecked.y) * speed;
        }

        return new Vector3(vectorWithoutYAxisChecked.x, newY, vectorWithoutYAxisChecked.z);
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Checks if collision is with the goal line to add a point to the opposing player
    /// and teleport the ball back to the starting position. If the ball collides with
    /// a wall or a paddle, the ball bounces off of it. The speed of the ball on colliding is limited
    /// to a range of 90% to 110% of the speed value.
    /// </remarks>
    /// <param name="other">
    /// Collision object that is used to determine what happens with the ball.
    /// </param>
    protected override void OnCollisionEnter(Collision other)
    {
        var collidingLayer = other.gameObject.layer;

        // Don't edit goal functionality for 3D
        if (collidingLayer.Equals(GoalLineLayer))
        {
            base.OnCollisionEnter(other);
            return;
        }

        // Check speed limit and adjust appropriately
        var sideMovement = Mathf.Abs(Rigidbody.velocity.x);
        var verticalMovement = Mathf.Abs(Rigidbody.velocity.y);
        var forwardMovement = Mathf.Abs(Rigidbody.velocity.z);

        var currentVector = Rigidbody.velocity;
        var newX = currentVector.x;
        var newY = currentVector.y;
        var newZ = currentVector.z;

        // Adjust X-Axis speed
        if (sideMovement < speed * 0.9f || sideMovement > speed * 1.1f)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newX = Mathf.Sign(currentVector.x) * speed;
        }

        // Adjust Y-Axis speed
        if (verticalMovement < speed * 0.9f || verticalMovement > speed * 1.1f)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newY = Mathf.Sign(currentVector.y) * speed;
        }

        // Adjust Z-Axis speed
        if (forwardMovement < speed * 0.9f || forwardMovement > speed * 1.1f)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newZ = Mathf.Sign(currentVector.z) * speed;
        }

        // Apply speed changes
        Rigidbody.velocity = new Vector3(newX, newY, newZ);
    }
}