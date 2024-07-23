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

    /// <inheritdoc cref="CheckBallSpeedLimit"/>
    /// <remarks>
    /// Checks the current velocity x, y, and z and ensures they are within 90% and 110% of the
    /// ball speed.
    /// </remarks>
    /// <returns>
    /// Vector that is created after limiting the speed.
    /// </returns>
    protected override void CheckBallSpeedLimit()
    {
        // Check speed limit and adjust appropriately
        base.CheckBallSpeedLimit();

        var currentVector = rigidbody.linearVelocity;
        var newY = currentVector.y;
        var verticalMovement = Mathf.Abs(rigidbody.linearVelocity.y);
        
        var lowSpeed = speed * 0.9f;
        var highSpeed = speed * 1.1f;

        // Adjust Y-Axis speed
        if (verticalMovement < lowSpeed || verticalMovement > highSpeed)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newY = Mathf.Sign(newY) * speed;
        }

        rigidbody.linearVelocity = new Vector3(currentVector.x, newY, currentVector.z);
    }
}