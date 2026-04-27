using UnityEngine;
using TMPro;

public class StatsPanel : MonoBehaviour
{
  [SerializeField] TextMeshProUGUI p1Wins, draws, p2Wins, gamesPlayed, avgTime;

  private void OnEnable()
  {
    SaveManager.Instance.LoadGame();
    UpdateTexts();
  }

  void UpdateTexts()
  {
    p1Wins.text = SaveManager.Instance.playerScores[0].ToString();
    draws.text = SaveManager.Instance.playerScores[1].ToString();
    p2Wins.text = SaveManager.Instance.playerScores[2].ToString();

    gamesPlayed.text = SaveManager.Instance.numberOfPlayedGames.ToString();

    var time = SaveManager.Instance.averageTime;
    int min = Mathf.FloorToInt(time / 60);
    int sec = Mathf.FloorToInt(time % 60);
    avgTime.text = string.Format("{0:00}:{1:00}", min, sec);
  }

  public void ResetStats()
  {
    SaveManager.Instance.ResetStats();
    SaveManager.Instance.LoadGame();
    UpdateTexts();
  }
}
