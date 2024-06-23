using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float speed;
    [NonSerialized] public Rigidbody Rigidbody;

    [SerializeField] private GameManager gameManager;
    private Vector3 _startingPosition;
    private int _goalLayer;

    private bool _collidingWithWall;
    private bool _collidingWithPaddle;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        // Set starting position
        var ballTransform = transform;
        _startingPosition = ballTransform.position;
        speed *= ballTransform.parent.localScale.x;
        Rigidbody = GetComponent<Rigidbody>();

        _goalLayer = LayerMask.NameToLayer("Goal");

        Launch();
    }

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
    private Vector3 ChooseStartVector()
    {
        // Choose left or right
        var startVector = Random.Range(0, 2) == 0
            ? Vector3.left * speed
            : Vector3.right * speed;

        // Add random angle
        startVector += new Vector3(0, 0, Random.Range(-2f, 2f));

        return startVector;
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Checks if collision is with the Goal to add a point to the opposing player
    /// and teleport the ball back to the starting position. If the ball collides with
    /// a wall or a paddle, the ball bounces off of it. The speed is then limited to a range.
    /// </remarks>
    /// <param name="other">
    /// Collision object that is used to determine what happens with the ball.
    /// </param>
    private void OnCollisionEnter(Collision other)
    {
        var collidingLayer = other.gameObject.layer;

        // Check colliding with Goal
        if (collidingLayer.Equals(_goalLayer))
        {
            // Gets the opposing player to score a point for them
            var player = other.gameObject.GetComponent<Goal>().opposingPlayer.GetComponent<PaddleAgent>();
        
            // Give point
            gameManager.AddPoint(player);
        
            // If in real game, set timer before teleporting
            // TODO: Set time before teleporting
        
            // Teleport back to starting location
            transform.position = _startingPosition;
            Rigidbody.velocity = Vector3.zero;
        
            // Check game over
            if (gameManager.IsGameOver())
            {
                return;
            }
        
            // Launch ball again
            Launch();
            return;
        }

        CheckBallSpeedLimit();
    }

    /// <inheritdoc cref="OnCollisionExit"/>
    /// <remarks>
    /// Checks the speed limit of the ball before exiting a collision.
    /// </remarks>
    /// <param name="other"></param>
    private void OnCollisionExit(Collision other)
    {
        CheckBallSpeedLimit();
    }

    /// <summary>
    /// The speed of the ball on colliding is limited to a range of 90% to 110% of the speed value.
    /// </summary>
    private void CheckBallSpeedLimit()
    {
        // Check speed limit and adjust appropriately
        var currentVector = Rigidbody.velocity;
        var newX = currentVector.x;
        var newZ = currentVector.z;
        var sideMovement = Mathf.Abs(newX);
        var forwardMovement = Mathf.Abs(newZ);

        var lowSpeed = speed * 0.9f;
        var highSpeed = speed * 1.1f;

        // Adjust X-Axis speed
        if (sideMovement < lowSpeed || sideMovement > highSpeed)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            if (currentVector.x == 0)
            {
                Debug.Log("0 X");
                currentVector.x = 1;
            }

            newX = Mathf.Sign(currentVector.x) * speed;
        }

        // Adjust Z-Axis speed
        if (forwardMovement < lowSpeed || forwardMovement > highSpeed)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            if (currentVector.z == 0)
            {
                Debug.Log("0 Z");
                currentVector.z = 1;
            }

            newZ = Mathf.Sign(currentVector.z) * speed;
        }

        // Apply speed changes
        Rigidbody.velocity = new Vector3(newX, currentVector.y, newZ);
    }
}