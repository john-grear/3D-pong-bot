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
    private PaddleAgent _whoHitLast;
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

    /// <summary>
    /// Launches the ball with a random starting vector and angle.
    /// </summary>
    private void Launch()
    {
        var ballVector = ChooseStartVector();
        _rigidbody.velocity = AddRandomAngle(ballVector);
        SaveLastVelocity();
    }

    /// <summary>
    /// Randomly generated a vector going left or right.
    /// </summary>
    /// <returns>
    /// Vector either going left or right.
    /// </returns>
    private Vector3 ChooseStartVector()
    {
        return Random.Range(0, 1) == 0 ? Vector3.left * speed : Vector3.right * speed;
    }

    /// <inheritdoc cref="OnCollisionEnter"/>
    /// <remarks>
    /// Checks if collision is with the goal line to add a point to the opposing player
    /// and teleport the ball back to the starting position. If the ball collides with
    /// a wall or a paddle, the ball bounces away from it with varying speed and angle.
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
            // Give point
            gameManager.AddPoint(_whoHitLast);
            
            // If in real game, set timer before teleporting

            // Teleport back to starting location
            transform.position = _startingPosition;
            
            // Launch ball again
            Launch();
        }
        else if (collidingLayer.Equals(_paddleLayer))
        {
            _whoHitLast = other.gameObject.GetComponent<PaddleAgent>();

            var whoIsWinning = gameManager.WhoIsWinning();

            // Game is tied or player who hit last is winning
            if (whoIsWinning == null || whoIsWinning.Equals(_whoHitLast))
            {
                // Set neutral speed threshold
                SetNewSpeedAndAngle(false);
            }
            else
            {
                // Player who hit last is losing
                // Set higher speed threshold
                SetNewSpeedAndAngle(true);
            }
        }
        else if (collidingLayer.Equals(_wallLayer))
        {
            // Set neutral speed threshold
            SetNewSpeedAndAngle(false);
        }

        SaveLastVelocity();
    }

    /// <summary>
    /// Generates a random speed and random angle to change the ball's velocity to use in collisions.
    /// </summary>
    /// <param name="useHigherSpeedThreshold">
    /// When true, this will increase the cap of the random speed value.
    /// </param>
    /// <returns>
    /// Sets a random speed value and adds a random angle to the ball's velocity.
    /// </returns>
    private void SetNewSpeedAndAngle(bool useHigherSpeedThreshold)
    {
        _lastVelocity.Normalize();
        _lastVelocity *= -GenerateRandomSpeed(useHigherSpeedThreshold);
        _rigidbody.velocity = AddRandomAngle(_lastVelocity);
    }

    /// <summary>
    /// Generates a random float based on the speed value of the ball.
    /// </summary>
    /// <param name="useHigherSpeedThreshold">
    /// When true, this will increase the cap of the random speed value.
    /// </param>
    /// <returns>
    /// The random speed value generated.
    /// </returns>
    private float GenerateRandomSpeed(bool useHigherSpeedThreshold)
    {
        return useHigherSpeedThreshold ? Random.Range(speed, speed * 2) : Random.Range(speed / 2, speed);
    }

    /// <summary>
    /// Generates a random float for an angle of the ball when bouncing.
    /// </summary>
    /// <returns>
    /// The random angle value generated.
    /// </returns>
    private static Vector3 AddRandomAngle(Vector3 vector)
    {
        var angle = Random.Range(0.05f, 0.45f);

        if (Random.Range(0, 1) == 0)
        {
            angle *= -1;
        }
        
        return new Vector3(vector.x, vector.y, angle);
    }

    /// <summary>
    /// Saves last velocity as rigidbody.velocity to use during collisions when the ball stops.
    /// </summary>
    private void SaveLastVelocity()
    {
        _lastVelocity = _rigidbody.velocity;
    }
}