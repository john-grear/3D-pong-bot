using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Scoreboard scoreboard;
    public GameObject player1Object;
    public GameObject player2Object;
    public GameObject ball;

    private PaddleAgent _player1;
    private PaddleAgent _player2;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Sets up starting values.
    /// </remarks>
    private void Start()
    {
        _player1 = player1Object.GetComponent<PaddleAgent>();
        _player2 = player2Object.GetComponent<PaddleAgent>();
    }

    /// <summary>
    /// Adds a point to the given player and updates the scoreboard.
    /// </summary>
    /// <param name="player">
    /// Determines which player will receive a point.
    /// </param>
    public void AddPoint(PaddleAgent player)
    {
        player.AddPoint();

        // Update scoreboard above game
        scoreboard.UpdateText(_player1, _player2);
    }

    /// <summary>
    /// Determines if the game is over based on points of both players.
    /// </summary>
    /// <returns>
    /// Whether the game is over.
    /// </returns>
    public bool IsGameOver()
    {
        return DetermineWinnerAndSetRewards(_player1, _player2) ||
               DetermineWinnerAndSetRewards(_player2, _player1);
    }

    /// <summary>
    /// Determines who is winning the game based on both players' points. If a winner is determined,
    /// set the rewards for each winner and return true.
    /// </summary>
    /// <returns>
    /// Whether there is a winner or not.
    /// </returns>
    private static bool DetermineWinnerAndSetRewards(PaddleAgent player1, PaddleAgent player2)
    {
        var pointDifference = Math.Abs(player1.Points - player2.Points);
        if (player2.Points <= player1.Points || player2.Points < 11 || pointDifference < 2) return false;

        player1.EndEpisode();
        player2.EndEpisode();
        return true;
    }
}