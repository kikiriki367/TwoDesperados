using UnityEngine;

public class SaveManager : MonoBehaviour
{
  public static SaveManager Instance { get; private set; }

  [HideInInspector] public bool portraitMode;
  [HideInInspector] public bool musicEnabled = true, sfxEnabled = true;
  [HideInInspector] public int boardSizeSelected, winConSelected;
  [HideInInspector] public int themeActive;
  //Stats
  [HideInInspector] public int[] playerScores;
  const string SCORES = "HIGH_SCORES";
  [HideInInspector] public int numberOfPlayedGames;
  [HideInInspector] public float averageTime;


  void Awake()
  {
    if (Instance) DestroyImmediate(gameObject);
    else
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    LoadGame();
    portraitMode = true;
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.R)) PlayerPrefs.DeleteAll();
  }

  public void ChangeBoardSize(int change)
  {
    if (boardSizeSelected + change < 0 || boardSizeSelected + change > 4 || boardSizeSelected + change < winConSelected) return;
    boardSizeSelected += change;
    PlayerPrefs.SetInt("BOARD_SIZE", boardSizeSelected);
  }

  public void ChangeWinCon(int change)
  {
    if (winConSelected + change < 0 || winConSelected + change > 4 || winConSelected + change > boardSizeSelected) return;
    winConSelected += change;
    PlayerPrefs.SetInt("WIN_CON", winConSelected);
  }

  public void ChangeActiveTheme(int themeIndex)
  {
    themeActive = themeIndex;
    PlayerPrefs.SetInt("ACTIVE_THEME", themeIndex);
  }

  public void ChangeMusicToggle()
  {
    musicEnabled = !musicEnabled;
    PlayerPrefs.SetInt("MUSIC", (musicEnabled ? 1 : 0));
  }

  public void ChangeSFXToggle()
  {
    sfxEnabled = !sfxEnabled;
    PlayerPrefs.SetInt("SFX", (sfxEnabled ? 1 : 0));
  }

  public void ChangePortraitMode(bool mode)
  {
    portraitMode = mode;
    PlayerPrefs.SetInt("PORTRAIT_MODE", (portraitMode ? 1 : 0));
  }

  public void SetScore(int player, float time)
  {
    int playerWon = player + 1;
    playerScores[playerWon] += 1;

    averageTime = (averageTime * numberOfPlayedGames + time) / (numberOfPlayedGames+1);
    numberOfPlayedGames++;

    SaveGame();
  }

  public void SaveGame()
  {
    PlayerPrefs.SetInt("ACTIVE_SKIN", themeActive);
    PlayerPrefs.SetInt("BOARD_SIZE", boardSizeSelected);
    PlayerPrefs.SetInt("WIN_CON", winConSelected);

    PlayerPrefs.SetInt("GAMES_PLAYED", numberOfPlayedGames);
    PlayerPrefs.SetFloat("AVERAGE_TIME", averageTime);
    for (int i = 0; i < playerScores.Length; i++) PlayerPrefs.SetInt($"{SCORES}_{i}", playerScores[i]);
  }

  public void LoadGame()
  {
    portraitMode = PlayerPrefs.GetInt("PORTRAIT_MODE", 1) != 0;
    musicEnabled = PlayerPrefs.GetInt("MUSIC", 1) != 0;
    sfxEnabled = PlayerPrefs.GetInt("SFX", 1) != 0;

    themeActive = PlayerPrefs.GetInt("ACTIVE_SKIN", 0);
    boardSizeSelected = PlayerPrefs.GetInt("BOARD_SIZE", 0);
    winConSelected = PlayerPrefs.GetInt("WIN_CON", 0);

    numberOfPlayedGames = PlayerPrefs.GetInt("GAMES_PLAYED", 0);
    averageTime = PlayerPrefs.GetFloat("AVERAGE_TIME", 0);
    for (int i = 0; i < playerScores.Length; i++) playerScores[i] = PlayerPrefs.GetInt($"{SCORES}_{i}", 0);
  }

  public void ResetStats()
  {
    PlayerPrefs.DeleteKey("GAMES_PLAYED");
    PlayerPrefs.DeleteKey("AVERAGE_TIME");
    for (int i = 0; i < playerScores.Length; i++) PlayerPrefs.DeleteKey($"{SCORES}_{i}");
  }
}
