using System.Collections;
using System.Linq;
using TMPro;
using Unity.MLAgents.Policies;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera gameCamera;
    public Vector3 cameraGamePosition;

    public SceneManager sceneManager;
    public Scoreboard scoreboard;
    public TextMeshPro timer;
    public GameObject gameOverCanvas;
    public TextMeshPro gameOverText;

    public Ball ball; // Used in PaddleAgent to access ball
    public Difficulty difficulty;
    public bool isTraining;
    public bool isGameOver;

    private Vector3 _cameraMenuPosition;
    private Vector3 _cameraPositionToMoveTo;

    /// <inheritdoc cref="Start"/>
    /// <remarks>
    /// Disable canvas containing play again and quit buttons, game over text, the timer text, and scoreboard
    /// so that all objects have a clear starting point to enable what is needed later.
    /// </remarks>
    private void Start()
    {
        gameOverCanvas.SetActive(false);
        timer.gameObject.SetActive(false);
        scoreboard.gameObject.SetActive(!isTraining);

        _cameraMenuPosition = gameCamera.transform.position;
    }

    /// <summary>
    /// Moves the camera, ball, and paddles to their game starting positions smoothly using Vector3.MoveTowards
    /// before starting the countdown to start the game.
    /// </summary>
    public void StartGame()
    {
        // Disable menu before starting game
        sceneManager.DisableMenu();

        // Enable scoreboard for non-training games
        scoreboard.gameObject.SetActive(true);
        scoreboard.ResetText();
        isTraining = false;

        // Get players and reset points for both
        var player1 = scoreboard.goal1.defendingPlayer;
        var player2 = scoreboard.goal1.opposingPlayer;
        player1.points = 0;
        player2.points = 0;

        // Set player 1 to be played by player, not CPU
        var player1Behavior = player1.GetComponent<BehaviorParameters>();
        player1Behavior.Model = null;
        player1Behavior.BehaviorType = BehaviorType.HeuristicOnly;

        // Stops ball from moving
        ball.StopMoving();

        // Move camera, ball, and paddles into position
        _cameraPositionToMoveTo = cameraGamePosition;
        StartCoroutine(MoveAllObjectsToStartPosition(player1, player2));

        // Start game in 5 seconds
        StartCoroutine(CountdownToStartGame(5));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    /// <returns></returns>
    private IEnumerator MoveAllObjectsToStartPosition(PaddleAgent player1, PaddleAgent player2)
    {
        bool cameraInPosition = false, player1InPosition = false, player2InPosition = false, ballInPosition = false;
        while (!(cameraInPosition && player1InPosition && player2InPosition && ballInPosition))
        {
            // Move each paddle, ball, and camera into position using LERP
            if (!cameraInPosition) cameraInPosition = MoveIntoPosition(gameCamera.transform, _cameraPositionToMoveTo);
            if (!player1InPosition) player1InPosition = MoveIntoPosition(player1.transform, player1.startingPosition);
            if (!player2InPosition) player2InPosition = MoveIntoPosition(player2.transform, player2.startingPosition);
            if (!ballInPosition) ballInPosition = MoveIntoPosition(ball.transform, ball.startingPosition);

            yield return null; // Wait for the next frame
        }
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
    private bool MoveIntoPosition(Transform objectToMove, Vector3 position)
    {
        // Move the object towards the position over time
        var positionToMoveTo = Vector3.MoveTowards(
            objectToMove.position, position, Time.deltaTime * transform.localScale.sqrMagnitude
        );
        objectToMove.position = positionToMoveTo;

        // If too far away, return false to continue moving closer
        if (Vector3.Distance(positionToMoveTo, position) > 0.01f) return false;

        // Set objectToMove's position exactly if close enough to the given position
        objectToMove.position = position;
        return true;
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
        timer.gameObject.SetActive(true);
        timer.text = $"{countdown}";

        // Wait one second
        yield return new WaitForSeconds(1);

        // If more time, keep waiting
        if (--countdown > 0) yield return CountdownToStartGame(countdown);
        else
        {
            timer.text = "Start!";
            isGameOver = false;
            ball.Launch();

            yield return new WaitForSeconds(1);

            timer.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Moves the ball and paddles to their starting positions smoothly using Vector3.MoveTowards and moves the
    /// camera to the menu position to display the menu correctly again.
    /// </summary>
    public void QuitToMainMenu()
    {
        // Disable game over text and buttons
        gameOverCanvas.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        
        // Get players and reset points for both
        var player1 = scoreboard.goal1.defendingPlayer;
        var player2 = scoreboard.goal1.opposingPlayer;
        
        // Move camera, ball, and paddles into position
        _cameraPositionToMoveTo = _cameraMenuPosition;
        StartCoroutine(MoveAllObjectsToStartPosition(player1, player2));
        
        // Wait a couple seconds
        StartCoroutine(CountdownToMainMenu());

        // Disable scoreboard and enable all buttons and text for main menu
        sceneManager.EnableMenu();
        scoreboard.gameObject.SetActive(false);

        // TODO: Set both players to hard AI
        // var player1Behavior = player1.GetComponent<BehaviorParameters>();
        // player1Behavior.Model = null;
        // player1Behavior.BehaviorType = BehaviorType.HeuristicOnly;
        // var player2Behavior = player1.GetComponent<BehaviorParameters>();
        // player2Behavior.Model = null;
        // player2Behavior.BehaviorType = BehaviorType.HeuristicOnly;

        // Stops ball from moving
        ball.StopMoving();
    }

    /// <summary>
    /// Wait for a couple seconds before moving on to display the main menu.
    /// </summary>
    /// <returns></returns>
    private static IEnumerator CountdownToMainMenu()
    {
        // Wait one second
        yield return new WaitForSeconds(2);
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
            var pointDifference = player1.points - player2.points;
            if (pointDifference < 2 || player1.points < 11) continue;
            winner = player1;
            winnerIsPlayer1 = i == 0;
        }

        if (winner == null) return false;

        // Toggle Game Over status
        isGameOver = true;

        // Display winner / loser text and play again button
        gameOverText.text = winnerIsPlayer1 ? "You Win!" : "You Lose!";
        gameOverCanvas.SetActive(true);

        // Only end episode of agents if training
        if (!isTraining) return true;

        foreach (var playerGoal in goals)
            playerGoal.defendingPlayer.EndEpisode();

        return true;
    }
}