using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PaddleAgent : Agent
{
    public float speed;

    [NonSerialized] public int Points;

    [SerializeField] private GameManager gameManager;
    private Rigidbody _rigidbody;
    private Ball _ball;
    private Vector3 _startingPosition;
    private int _ballLayer;
    // private int _lastDirection;
    // private int _sameDirectionFrames;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _ball = gameManager.ball.GetComponent<Ball>();
        _startingPosition = transform.position;

        _ballLayer = LayerMask.NameToLayer("Ball");
    }

    /// <inheritdoc cref="CollectObservations"/>
    /// <remarks>
    /// Gets the position of the ball, it's velocity, and the distance to it as well as the positions of the two
    /// players' paddles and this paddle's current velocity.
    /// </remarks>>
    /// <param name="sensor">
    /// What stores the information that it is receiving.
    /// </param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Where both paddles are at
        sensor.AddObservation(gameManager.player1Object.transform.position);
        sensor.AddObservation(gameManager.player2Object.transform.position);

        // Current velocity
        sensor.AddObservation(_rigidbody.velocity);

        // Location of, velocity of, and distance to the ball
        var ballPosition = _ball.transform.position;
        sensor.AddObservation(ballPosition);
        sensor.AddObservation(_ball.Rigidbody.velocity);
        sensor.AddObservation(ballPosition - gameObject.transform.position);
    }

    /// <inheritdoc cref="Heuristic"/>
    /// <remarks>
    /// Manually control the agent using vertical input to control the paddle.
    /// </remarks>
    /// <param name="actionsOut">
    /// What stores the actions to be used.
    /// </param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxisRaw("Vertical");
    }

    /// <inheritdoc cref="OnActionReceived"/>
    /// <remarks>
    /// Determines what to do when receiving actions.
    /// </remarks>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActionsOut = actions.ContinuousActions;

        _rigidbody.velocity = continuousActionsOut[0] switch
        {
            > 0 => Vector3.forward * speed,
            < 0 => Vector3.back * speed,
            _ => Vector3.zero
        };

        // CheckMovement(_rigidbody.velocity);

        // Penalize paddle for moving to incentivize efficient movement to hit the ball
        if (_rigidbody.velocity != Vector3.zero)
        {
            AddReward(-0.01f);
        }
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Checks if collision is with the ball to provide rewards.
    /// </remarks>
    /// <param name="other">
    /// Potential ball object.
    /// </param>
    private void OnCollisionEnter(Collision other)
    {
        // Reward the agent for hitting the ball
        if (other.gameObject.layer.Equals(_ballLayer))
        {
            AddReward(1f);
        }
    }

    // TODO: Original function to stop jittering of AI, but needs reworked.
    // /// <summary>
    // /// Checks the movement of the paddle to ensure that it doesn't jitter around during testing.
    // /// </summary>
    // /// <param name="paddleMovement">
    // /// Velocity to check to ensure stability.
    // /// </param>
    // private void CheckMovement(Vector3 paddleMovement)
    // {
    //     if (Math.Sign(paddleMovement.x) == 0) return;
    //
    //     // Check moving same direction as before
    //     if (Math.Sign(paddleMovement.x) == _lastDirection)
    //     {
    //         // Count frames moving in the same direction to prevent jittering
    //         ++_sameDirectionFrames;
    //     }
    //     else if (_sameDirectionFrames < 10)
    //     {
    //         // Update direction and penalize paddle for moving in a different direction too soon
    //         _lastDirection = Math.Sign(paddleMovement.x);
    //         _sameDirectionFrames = 0;
    //         AddReward(-0.5f);
    //     }
    // }

    /// <summary>
    /// Resets the position of the paddle before ending the episode.
    /// </summary>
    public new void EndEpisode()
    {
        transform.position = _startingPosition;

        base.EndEpisode();
    }

    /// <inheritdoc cref="OnEpisodeBegin"/>
    /// <remarks>
    /// Resets the scoreboard and launches the ball to start the game.
    /// </remarks>
    public override void OnEpisodeBegin()
    {
        // Reset scoreboard
        Points = 0;
        gameManager.scoreboard.ResetText();

        // Launch the ball
        _ball.Launch();
    }

    /// <summary>
    /// Adds a point to the internal scoreboard.
    /// </summary>
    public void AddPoint()
    {
        Points++;
    }
}