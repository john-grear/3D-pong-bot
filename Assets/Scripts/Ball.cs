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
    private bool _scored;

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
        _scored = false;
        Rigidbody.linearVelocity = ChooseStartVector();
    }

    /// <summary>
    /// Sets the linear velocity of the ball to zero to wait for game to begin.
    /// </summary>
    public void StopMoving()
    {
        Rigidbody.linearVelocity = Vector3.zero;
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
        // Prevent collisions with goal and something else from messing with launch functionality
        if (_scored) return;
        
        var collidingObject = other.gameObject;
        var collidingLayer = collidingObject.layer;

        CheckBallSpeedLimit();

        // Check colliding with Goal
        if (!collidingLayer.Equals(_goalLayer)) return;

        // Gets the opposing player to score a point for them
        var goal = collidingObject.GetComponent<Goal>();
        var player = goal.opposingPlayer;

        // Give point
        _gameManager.AddPoint(player);

        // Teleport back to starting location and stop the ball from moving
        transform.position = StartingPosition;
        if (!_gameManager.isTraining) StopMoving();
    }

    /// <inheritdoc cref="OnCollisionExit"/>
    /// <remarks>
    /// Checks the speed limit of the ball before exiting a collision.
    /// </remarks>
    /// <param name="other">
    /// Collision object that is used to determine what happens with the ball.
    /// </param>
    protected void OnCollisionExit(Collision other)
    {
        // Prevent collisions with goal and something else from messing with launch functionality
        if (_scored) return;
        
        var collidingObject = other.gameObject;
        var collidingLayer = collidingObject.layer;

        CheckBallSpeedLimit();

        // Check colliding with Goal
        if (!collidingLayer.Equals(_goalLayer)) return;

        // Disable collision functionality since ball has scored a goal
        _scored = true;
        
        StopMoving();

        // Check game over
        if (_gameManager.IsGameOver()) return;

        // Launch ball again immediately if training, else start a 3 second cooldown before launching again
        if (_gameManager.isTraining) Launch();
        else StartCoroutine(CountdownToStartGame(3));
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
    /// Start a countdown to resume the game.
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
        _gameManager.timer.gameObject.SetActive(true);
        _gameManager.timer.text = countdown == 0 ? "Start!" : $"{countdown}";

        // Wait one second
        yield return new WaitForSeconds(1);

        // If more time, keep waiting
        if (--countdown > 0) yield return CountdownToStartGame(countdown);
        else
        {
            _gameManager.timer.gameObject.SetActive(false);
            Launch();
        }
    }
}