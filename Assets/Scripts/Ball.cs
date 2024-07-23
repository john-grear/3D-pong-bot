using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float speed;

    [NonSerialized] public Rigidbody Rigidbody;
    [NonSerialized] public Vector3 StartingPosition;

    private GameManager _gameManager;
    private int _goalLayer;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        // Set starting position
        var ballTransform = transform;
        StartingPosition = ballTransform.position;
        speed *= ballTransform.parent.localScale.x;
        Rigidbody = GetComponent<Rigidbody>();
        _gameManager = transform.parent.GetComponent<GameManager>();

        _goalLayer = LayerMask.NameToLayer("Goal");

        Launch();
    }

    /// <summary>
    /// Launches the ball with a random starting vector and angle.
    /// </summary>
    public void Launch()
    {
        Rigidbody.linearVelocity = ChooseStartVector();
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
    protected virtual void OnCollisionEnter(Collision other)
    {
        var collidingObject = other.gameObject;
        var collidingLayer = collidingObject.layer;

        // Check colliding with Goal
        if (collidingLayer.Equals(_goalLayer))
        {
            // Gets the opposing player to score a point for them
            var goal = collidingObject.GetComponent<Goal>();
            var player = goal.opposingPlayer;

            // Give point
            _gameManager.AddPoint(player);

            // Teleport back to starting location
            transform.position = StartingPosition;
            Rigidbody.linearVelocity = Vector3.zero;

            // Check game over
            if (_gameManager.IsGameOver())
            {
                return;
            }

            // Set a countdown before launching ball again
            if (!_gameManager.isTraining) StartCoroutine(CountdownToStartGame(3));

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
    protected void OnCollisionExit()
    {
        CheckBallSpeedLimit();
    }

    /// <summary>
    /// The speed of the ball on colliding is limited to a range of 90% to 110% of the speed value.
    /// </summary>
    protected virtual void CheckBallSpeedLimit()
    {
        // Check speed limit and adjust appropriately
        var currentVector = Rigidbody.linearVelocity;
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
            if (newX == 0) currentVector.x = 1;

            newX = Mathf.Sign(currentVector.x) * speed;
        }

        // Adjust Z-Axis speed
        if (forwardMovement < lowSpeed || forwardMovement > highSpeed)
        {
            // TODO: Add momentum speed inside here when momentum mode is enabled and increment momentum speed
            if (newZ == 0) currentVector.z = 1;

            newZ = Mathf.Sign(currentVector.z) * speed;
        }

        // Apply speed changes
        Rigidbody.linearVelocity = new Vector3(newX, currentVector.y, newZ);
    }

    /// <summary>
    /// Start a countdown to resume the game using the scoreboard (TODO: Maybe another display later)
    /// to display time left before game starts.
    /// </summary>
    /// <param name="countdown">
    /// Seconds left before game starts.
    /// </param>
    /// <returns>
    /// Normal delay for countdown.
    /// </returns>
    private IEnumerator CountdownToStartGame(int countdown)
    {
        // Update countdown display
        _gameManager.scoreboard.scoreboard.text = countdown == 0 ? "Start!" : $"{countdown}";

        // Wait one second
        yield return new WaitForSeconds(1);

        // If more time, keep waiting
        if (--countdown > 0) yield return CountdownToStartGame(countdown);

        _gameManager.scoreboard.UpdateText();
        Launch();
    }
}