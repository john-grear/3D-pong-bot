using System;
using UnityEngine;

public class PaddleAgent : MonoBehaviour // Agent // Need to import MLAgent plugin like in NeuralBird
{
    public float speed;

    [NonSerialized] public int Points;
    
    private Rigidbody _rigidbody;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <inheritdoc cref="Update"/>
    /// <remarks>
    /// Moves the paddle forward and backward when pressing up / left and down / right, respectively.
    /// </remarks> 
    private void Update()
    {
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            _rigidbody.velocity = Vector3.forward * speed;
        }
        else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            _rigidbody.velocity = Vector3.back * speed;
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Adds a point to the internal scoreboard.
    /// </summary>
    public void AddPoint()
    {
        Points++;
    }
}