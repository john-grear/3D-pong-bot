using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public TMP_Text scoreboard;
    public Goal goal1;
    public Goal goal2;

    /// <summary>
    /// Updates the text to reflect the players scores.
    /// </summary>
    public void UpdateText()
    {
        // Set text to player1.points - player2.points
        scoreboard.SetText($"{goal1.defendingPlayer.points} - {goal2.defendingPlayer.points}");
    }

    /// <summary>
    /// Updates the text to the default 0 - 0.
    /// </summary>
    public void ResetText()
    {
        scoreboard.SetText("0 - 0");
    }
}