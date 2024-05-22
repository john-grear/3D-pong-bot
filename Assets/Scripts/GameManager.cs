using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Scoreboard scoreboard;
    public GameObject player1Object;
    public GameObject player2Object;

    private PaddleAgent _player1;
    private PaddleAgent _player2;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Set variables for both players.
    /// </remarks>
    private void Start()
    {
        _player1 = player1Object.GetComponent<PaddleAgent>();
        _player2 = player2Object.GetComponent<PaddleAgent>();
    }

    /// <summary>
    /// Adds a point to the given player and updates the scoreboard.
    /// </summary>
    /// <param name="player"></param>
    public void AddPoint(PaddleAgent player)
    {
        player.AddPoint();

        // Update scoreboard above game
        scoreboard.UpdateText(_player1, _player2);
    }

    /// <summary>
    /// Determines who is winning the game based on both players' points.
    /// </summary>
    /// <returns>
    /// PaddleAgent object of the winning player. Null if the game is tied.
    /// </returns>
    public PaddleAgent WhoIsWinning()
    {
        if (_player1.Points > _player2.Points)
        {
            return _player1;
        }

        return _player2.Points > _player1.Points ? _player2 : null;
    }
}