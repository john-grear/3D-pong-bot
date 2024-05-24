using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    public float speed;

    [SerializeField] private GameManager gameManager;
    private Vector3 _startingPosition;
    private Rigidbody _rigidbody;
    private int _paddleLayer;
    private int _goalLineLayer;
    private int _wallLayer;
    private Vector3 _lastVelocity;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        // Set starting position
        _startingPosition = transform.position;
        _rigidbody = GetComponent<Rigidbody>();

        _paddleLayer = LayerMask.NameToLayer("Paddle");
        _goalLineLayer = LayerMask.NameToLayer("Goal Line");
        _wallLayer = LayerMask.NameToLayer("Wall");

        Launch();
    }

    // /// <inheritdoc cref="FixedUpdate"/>
    // /// <remarks>
    // /// </remarks>
    // /// <exception cref="NotImplementedException"></exception>
    // private void FixedUpdate()
    // {
    //     // Ensure ball never slows down and maintains correct direction
    //     var halfSpeed = speed / 2;
    //     if (_rigidbody.velocity.x > -halfSpeed && _rigidbody.velocity.x < halfSpeed)
    //     {
    //         var direction = _lastVelocity.normalized.x * -1;
    //         _rigidbody.velocity.x = Random.Range(direction * speed, direction * speed * 2);
    //     }
    // }

    /// <summary>
    /// Launches the ball with a random starting vector and angle.
    /// </summary>
    private void Launch()
    {
        _rigidbody.velocity = ChooseStartVector();
        SaveLastVelocity();
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
        var startVector = Random.Range(0, 2) == 0 ? Vector3.left * speed : Vector3.right * speed;

        // Add random angle
        startVector += new Vector3(0, 0, Random.Range(-2f, 2f));

        return startVector;
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Checks if collision is with the goal line to add a point to the opposing player
    /// and teleport the ball back to the starting position. If the ball collides with
    /// a wall or a paddle, the ball bounces off of it.
    /// </remarks>
    /// <param name="other">
    /// Collision object that is used to determine what happens with the ball.
    /// </param>
    private void OnCollisionEnter(Collision other)
    {
        var collidingLayer = other.gameObject.layer;

        // Check colliding with Goal Line
        if (collidingLayer.Equals(_goalLineLayer))
        {
            // Gets the opposing player to score a point for them
            var player = other.gameObject.GetComponent<Goal>().opposingPlayer.GetComponent<PaddleAgent>();

            // Give point
            gameManager.AddPoint(player);

            // If in real game, set timer before teleporting

            // Teleport back to starting location
            transform.position = _startingPosition;
            _rigidbody.velocity = Vector3.zero;

            // Check game over
            if (gameManager.IsGameOver())
            {
                return;
            }

            // Launch ball again
            Launch();
            return;
        }

        // Reflect the velocity vector around the normal
        var normal = other.GetContact(0).normal;
        var reflectedVelocity = Vector3.Reflect(_rigidbody.velocity, normal);

        // Ensure ball never slows down and maintains correct direction
        var slowSpeed = speed * 0.9;
        if (Mathf.Abs(reflectedVelocity.x) < slowSpeed)
        {
            var direction = Mathf.Sign(_lastVelocity.x);
            if (collidingLayer.Equals(_paddleLayer))
            {
                // Reverse direction and give more power
                direction *= -1;
                reflectedVelocity.x = Random.Range(direction * speed, direction * speed * 1.25f);
            }
            else
            {
                // Maintain speed and direction
                reflectedVelocity.x = direction * speed;
            }
        }

        // Zero out the y to ensure no vertical movement
        reflectedVelocity.y = 0;

        // Check colliding with paddle
        if (collidingLayer.Equals(_paddleLayer))
        {
            // Ensure reflected ball never goes in straight line
            reflectedVelocity.z = reflectedVelocity.z switch
            {
                > -1f and < 0f => Random.Range(-2f, -1.5f),
                > 0f and < 1f => Random.Range(1.5f, 2f),
                _ => reflectedVelocity.z
            };
        }

        // Apply the bounce factor
        _rigidbody.velocity = reflectedVelocity;

        SaveLastVelocity();
    }

    /// <summary>
    /// Saves last velocity as rigidbody.velocity to use during collisions when the ball stops.
    /// </summary>
    private void SaveLastVelocity()
    {
        _lastVelocity = _rigidbody.velocity;
    }
}