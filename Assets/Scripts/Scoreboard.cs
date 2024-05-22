using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public TMP_Text scoreboard;

    /// <summary>
    /// Updates the text to reflect the players scores.
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    public void UpdateText(PaddleAgent player1, PaddleAgent player2)
    {
        // Set text to player1.points - player2.points
        scoreboard.SetText($"{player1.Points} - {player2.Points}");
    }
}