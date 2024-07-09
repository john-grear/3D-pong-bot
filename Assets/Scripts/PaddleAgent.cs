using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PaddleAgent : Agent
{
    public float speed;
    public float smoothingFactor = 3f;

    [NonSerialized] public int Points;

    protected Rigidbody Rigidbody;
    protected GameManager GameManager;
    protected bool IsAgent;
    protected int BallLayer;

    private Ball _ball;
    private Goal _goal;
    private Vector3 _startingPosition;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();

        var paddleTransform = transform;
        GameManager = paddleTransform.parent.GetComponent<GameManager>();
        _startingPosition = paddleTransform.position;
        speed *= paddleTransform.parent.localScale.x;

        _ball = GameManager.ball;
        BallLayer = LayerMask.NameToLayer("Ball");
        _goal = GameManager.GetGoalForPlayer(this);

        // Check agent is either in inference mode or it's in heuristic mode with a model set
        // Used to give different controls to player and agent
        var behaviorParameters = GetComponent<BehaviorParameters>();
        IsAgent = !behaviorParameters.IsInHeuristicMode() ||
                  (behaviorParameters.IsInHeuristicMode() && behaviorParameters.Model != null);
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
        var currentPosition = transform.position;
        sensor.AddObservation(currentPosition);
        sensor.AddObservation(_goal.opposingPlayer.transform.position);

        // Current velocity
        sensor.AddObservation(Rigidbody.velocity);

        // Location of, velocity of, and distance to the ball
        var ballPosition = _ball.transform.position;
        sensor.AddObservation(ballPosition);
        sensor.AddObservation(_ball.Rigidbody.velocity);
        var distanceToBall = Vector3.Distance(currentPosition, ballPosition);
        sensor.AddObservation(distanceToBall);
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
        var discreteActionsOut = actionsOut.DiscreteActions;

        discreteActionsOut[0] = Input.GetAxisRaw("Vertical") switch
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
            var targetVelocity = discreteActionsOut[0] switch
            {
                2 => Vector3.forward * speed,
                1 => Vector3.back * speed,
                _ => Vector3.zero
            };

            // Smoothly transition to the target velocity
            var newVelocity = Vector3.Lerp(
                Rigidbody.velocity, targetVelocity, smoothingFactor * Time.deltaTime
            );

            Rigidbody.velocity = newVelocity;
        }
        else
        {
            var newVelocity = discreteActionsOut[0] switch
            {
                1 => Vector3.forward * speed,
                -1 => Vector3.back * speed,
                _ => Vector3.zero
            };

            Rigidbody.velocity = newVelocity;
        }
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Checks if collision is with the ball to provide rewards.
    /// </remarks>
    /// <param name="other">
    /// Object colliding into.
    /// </param>
    protected virtual void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.layer.Equals(BallLayer)) return;

        var contactNormal = other.contacts[0].normal;

        // Check if the collision is on the short side of the paddle
        if ((Mathf.Abs(contactNormal.z) > 0.75))
        {
            // Stops abuse of AI hitting ball with side of paddle, breaking score system and making ball escape
            SetReward(0f);
            return;
        }

        // Reward the agent for hitting the ball
        AddReward(1f);
    }

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
        GameManager.scoreboard.ResetText();

        if (!_ball) Start();

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