using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float speed;
    [NonSerialized] public Rigidbody Rigidbody;

    protected int GoalLineLayer;

    private GameManager _gameManager;
    private Vector3 _startingPosition;
    private int _paddleLayer;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    protected void Start()
    {
        // Set starting position
        _startingPosition = transform.position;
        Rigidbody = GetComponent<Rigidbody>();
        _gameManager = transform.parent.GetComponent<GameManager>();

        _paddleLayer = LayerMask.NameToLayer("Paddle");
        GoalLineLayer = LayerMask.NameToLayer("Goal Line");

        Launch();
    }

    // TODO: This needs changed to disallow the ball from bouncing out of bounds
    // TODO: Maybe better to use OnCollisionStay so the update function isn't running constantly to check
    // /// <inheritdoc cref="Update"/>
    // /// <remarks>
    // /// Sets up starting values.
    // /// </remarks>
    // private void Update()
    // {
    //     // Prevent ball from moving out of bounds
    //     if (Rigidbody.position.x > bounds || Rigidbody.position.x < bounds)
    //     {
    //         transform.position = _startingPosition;
    //     }
    // }

    /// <summary>
    /// Launches the ball with a random starting vector and angle.
    /// </summary>
    public void Launch()
    {
        Rigidbody.velocity = ChooseStartVector();
    }

    /// <summary>
    /// Randomly generates a starting vector for the ball.
    /// </summary>
    /// <returns>
    /// Vector either going left or right with a random angle added.
    /// </returns>
    protected virtual Vector3 ChooseStartVector()
    {
        // Choose left or right
        var startVector = Random.Range(0, 2) == 0 ? Vector3.left * speed : Vector3.right * speed;

        // Add random angle
        startVector += new Vector3(0, 0, Random.Range(-2f, 2f));

        return startVector;
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
    protected virtual void OnCollisionEnter(Collision other)
    {
        var collidingLayer = other.gameObject.layer;

        // Check colliding with paddle
        if (collidingLayer.Equals(_paddleLayer))
        {
            var paddle = other.gameObject;
            var contactNormal = other.contacts[0].normal;

            // Check if the collision is on the short side (x-axis)
            if (Mathf.Abs(contactNormal.x) > Mathf.Abs(contactNormal.y) &&
                Mathf.Abs(contactNormal.x) > Mathf.Abs(contactNormal.z))
            {
                // Reward for hitting front of the paddle to prevent abuse
                paddle.GetComponent<PaddleAgent>().AddReward(1f);
            }
            else
            {
                // Stops abuse of AI hitting ball with side of paddle
                paddle.GetComponent<PaddleAgent>().SetReward(0f);
            }
        }

        // Check colliding with Goal Line
        if (collidingLayer.Equals(GoalLineLayer))
        {
            // Gets the opposing player to score a point for them
            var player = other.gameObject.GetComponent<Goal>().opposingPlayer.GetComponent<PaddleAgent>();

            // Give point
            _gameManager.AddPoint(player);

            // Check game over
            if (_gameManager.IsGameOver())
            {
                return;
            }

            // If in real game, set timer before teleporting
            // TODO: Set time before teleporting

            // Teleport back to starting location
            transform.position = _startingPosition;
            Rigidbody.velocity = Vector3.zero;

            // Launch ball again
            Launch();
            return;
        }

        // Apply speed changes
        Rigidbody.velocity = CheckVectorSpeedLimit();
    }

    /// <summary>
    /// Checks the current velocity x and z and ensures they are within 90% and 110% of the ball speed.
    /// </summary>
    /// <returns>
    /// Vector that is created after limiting the speed.
    /// </returns>
    protected virtual Vector3 CheckVectorSpeedLimit()
    {
        // Check speed limit and adjust appropriately
        var sideMovement = Mathf.Abs(Rigidbody.velocity.x);
        var forwardMovement = Mathf.Abs(Rigidbody.velocity.z);

        var currentVector = Rigidbody.velocity;
        var newX = currentVector.x;
        var newZ = currentVector.z;

        // Adjust X-Axis speed
        if (sideMovement < speed * 0.9f || sideMovement > speed * 1.1f)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newX = Mathf.Sign(currentVector.x) * speed;
        }

        // Adjust Z-Axis speed
        if (forwardMovement < speed * 0.9f || forwardMovement > speed * 1.1f)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            newZ = Mathf.Sign(currentVector.z) * speed;
        }

        return new Vector3(newX, currentVector.y, newZ);
    }
}