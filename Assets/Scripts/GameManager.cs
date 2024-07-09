using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Scoreboard scoreboard;
    public Ball ball; // Used in PaddleAgent to access ball

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
        scoreboard.UpdateText();
    }

    /// <summary>
    /// Gets the Goal that correlates to the given player. This removes circular dependency for the PaddleAgent
    /// to use the Goal to access the opposing player during their observations.
    /// </summary>
    /// <param name="player">
    /// The PaddleAgent to use to check which Goal they belong to.
    /// </param>
    /// <returns>
    /// The Goal that the given player is defending.
    /// </returns>
    public Goal GetGoalForPlayer(PaddleAgent player)
    {
        return new[] { scoreboard.goal1, scoreboard.goal2 }.FirstOrDefault(goal => goal.defendingPlayer.Equals(player));
    }

    /// <summary>
    /// Determines if the game is over based on points of both players.
    /// </summary>
    /// <returns>
    /// Whether the game is over.
    /// </returns>
    public bool IsGameOver()
    {
        return DetermineWinner(scoreboard.goal1, scoreboard.goal2);
    }

    /// <summary>
    /// Determines who is winning the game based on both players' points. If a winner is determined,
    /// display winner text and return true.
    /// </summary>
    /// <returns>
    /// Whether there is a winner or not.
    /// </returns>
    private static bool DetermineWinner(params Goal[] goals)
    {
        var winner = (
            from goal in goals
            let player1 = goal.defendingPlayer
            let player2 = goal.opposingPlayer
            let pointDifference = player1.Points - player2.Points
            where player1.Points >= 11 && pointDifference >= 2
            select player1).FirstOrDefault();

        if (winner == null) return false;

        // TODO: Display winner text.

        foreach (var playerGoal in goals)
        {
            playerGoal.defendingPlayer.EndEpisode();
        }

        return true;
    }
}