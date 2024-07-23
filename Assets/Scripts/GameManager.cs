using System.Collections;
using System.Linq;
using TMPro;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Camera gameCamera;
    public Vector3 cameraStartingPosition;
    public SceneManager sceneManager;
    public Scoreboard scoreboard;
    public TextMeshPro gameOverText;
    public Button playAgainButton;
    public Ball ball; // Used in PaddleAgent to access ball
    public Difficulty difficulty;
    public bool isTraining;
    public bool isGameOver;

    /// <summary>
    /// Moves the camera in toward the game field, if not there already, as well as moving the paddles
    /// and ball into their starting positions. This is all using LERP to get a smooth movement into
    /// position before starting the countdown to start the game.
    /// </summary>
    public void StartGame()
    {
        // Disable play again button and game over text
        playAgainButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        // Disable menu before starting game
        sceneManager?.DisableMenu();

        // Enable scoreboard
        scoreboard.gameObject.SetActive(true);
        scoreboard.ResetText();
        isTraining = false;

        // Get players and reset points for both
        var player1 = scoreboard.goal1.defendingPlayer;
        var player2 = scoreboard.goal1.opposingPlayer;
        player1.Points = 0;
        player2.Points = 0;

        // Set player 1 to be played by player, not CPU
        var player1Behavior = player1.GetComponent<BehaviorParameters>();
        player1Behavior.Model = null;
        player1Behavior.BehaviorType = BehaviorType.HeuristicOnly;

        bool cameraInPosition = false, player1InPosition = false, player2InPosition = false, ballInPosition = false;
        ball.Rigidbody.linearVelocity = Vector3.zero;
        Debug.Log(ball.gameObject.transform.position);
        Debug.Log(ball.StartingPosition);

        // Simultaneously move all pieces into position
        while (true)
        {
            // Keep looping to move all pieces back into starting position before starting the game
            if (cameraInPosition && player1InPosition && player2InPosition && ballInPosition) break;

            // Move each paddle, ball, and camera into position using LERP
            if (!cameraInPosition) cameraInPosition = MoveIntoPosition(gameCamera.transform, cameraStartingPosition);
            if (!player1InPosition) player1InPosition = MoveIntoPosition(player1.transform, player1.StartingPosition);
            if (!player2InPosition) player2InPosition = MoveIntoPosition(player2.transform, player2.StartingPosition);
            if (!ballInPosition) ballInPosition = MoveIntoPosition(ball.transform, ball.StartingPosition);
        }

        Debug.Log(ball.gameObject.transform.position);
        Debug.Log(ball.StartingPosition);

        // ball.transform.position = ball.StartingPosition;

        StartCoroutine(CountdownToStartGame(5));
    }

    /// <summary>
    /// Start a countdown to start the game, using the scoreboard (TODO: Maybe another display later)
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
        scoreboard.scoreboard.text = countdown == 0 ? "Start!" : $"{countdown}";

        // Wait one second
        yield return new WaitForSeconds(1);

        // If more time, keep waiting
        if (--countdown > 0) yield return CountdownToStartGame(countdown);

        isGameOver = false;
        scoreboard.ResetText();
        ball.Launch();
    }

    /// <summary>
    /// Slowly transition the given objectToMove to the given position over time.
    /// </summary>
    /// <param name="objectToMove">
    /// Transform of the object being moved into position.
    /// </param>
    /// <param name="position">
    /// Where the objectToMove is being moved to.
    /// </param>
    /// <returns>
    /// Whether the objectToMove is at the position after moving.
    /// </returns>
    private static bool MoveIntoPosition(Transform objectToMove, Vector3 position)
    {
        // Move the object towards the position using LERP
        // objectToMove.position = Vector3.Lerp(
        //     objectToMove.position, position, Time.deltaTime
        // );
        objectToMove.position = Vector3.MoveTowards(objectToMove.position, position, Time.deltaTime);

        // Calculate distance to position after move
        var distance = Vector3.Distance(objectToMove.position, position);

        // If too far away, return false to continue moving closer
        if (distance > 0.01f) return false;

        // Set objectToMove's position exactly if close enough to the given position
        objectToMove.position = position;
        return true;
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
    private bool DetermineWinner(params Goal[] goals)
    {
        var winnerIsPlayer1 = false;
        PaddleAgent winner = null;
        for (var i = 0; i < goals.Length; i++)
        {
            var goal = goals[i];
            var player1 = goal.defendingPlayer;
            var player2 = goal.opposingPlayer;
            var pointDifference = player1.Points - player2.Points;
            if (pointDifference < 2 || player1.Points < 11) continue;
            winner = player1;
            winnerIsPlayer1 = i == 0;
        }

        if (winner == null) return false;

        // Toggle Game Over status
        isGameOver = true;

        // Display winner / loser text and play again button
        if (winnerIsPlayer1)
        {
            gameOverText.text = "You Win!";
            gameOverText.gameObject.SetActive(true);
            playAgainButton.gameObject.SetActive(true);
        }
        else
        {
            gameOverText.text = "You Lose!";
            gameOverText.gameObject.SetActive(true);
            playAgainButton.gameObject.SetActive(true);
        }

        // Only end episode of agents if training
        if (isTraining)
        {
            foreach (var playerGoal in goals)
                playerGoal.defendingPlayer.EndEpisode();

            return true;
        }

        ball.Rigidbody.linearVelocity = Vector3.zero;

        return true;
    }
}