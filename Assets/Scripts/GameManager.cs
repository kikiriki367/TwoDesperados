using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
  [Header("Board Values")]
  bool gameActive = false;
  [SerializeField] GameObject board;
  [SerializeField] GameObject fieldPrefab;
  List<Field> fieldList = new();
  int boardFieldSize;
  int winConditionLength;
  int boardFieldsNumber;

  [System.Serializable]
  public class Field
  {
    public GameObject fieldBtn;
    public int playerValue;
    public int fieldIndex;
  }

  Vector2[] lines = {
            new Vector2(-1, 0), new Vector2(1, 0),  // left/right
            new Vector2(0, -1), new Vector2(0, 1),  // down/up
            new Vector2(-1, -1), new Vector2(1, 1), // D-L/U-R
            new Vector2(1, -1), new Vector2(-1, 1)  // D-R/U-L
  };

  [Header("Scoreboard Values")]
  [SerializeField] TextMeshProUGUI p1MovesTxt, p2MovesTxt, timerTxt;
  int p1MovesNum, p2MovesNum;
  float timer;

  [Header("Player Values")]
  int activePlayer = 1;
  int turnsTaken;
  Transform winTile1, winTile2;

  [Header("Theme options")]
  [SerializeField] Sprite baseTileSkin;
  [SerializeField] Sprite[] playerSkins;
  int selectedTheme;

  [Header("End Panel")]
  [SerializeField] RectTransform endLine;
  [SerializeField] GameObject endPanel;
  [SerializeField] TextMeshProUGUI endTitleTxt;
  [SerializeField] TextMeshProUGUI endTimeTxt;

  [Header("Misc")]
  [SerializeField] GameObject settingsPanel;
  [SerializeField] Toggle musicToggle, sfxToggle;
  [SerializeField] GameObject bgImg;
  [SerializeField] ParticleSystem particleX, particleO;

  public void Start()
  {
    if ((!SaveManager.Instance.portraitMode && GetComponent<CanvasScaler>().referenceResolution.x == 1080f)
      || (SaveManager.Instance.portraitMode && GetComponent<CanvasScaler>().referenceResolution.x == 1920f))
    {
      AdjustBackground(SaveManager.Instance.portraitMode);
      AdjustOrientation(SaveManager.Instance.portraitMode);
    }

    boardFieldSize = SaveManager.Instance.boardSizeSelected + 3;
    winConditionLength = SaveManager.Instance.winConSelected + 3;
    selectedTheme = SaveManager.Instance.themeActive;

    SpawnBoard();
    gameActive = true;
  }

  private void Update()
  {
    if (!gameActive) return;
    timer += Time.deltaTime;

    if (Mathf.FloorToInt(timer) != Mathf.FloorToInt(timer - Time.deltaTime)) timerTxt.text = TimeToString(timer);
  }

  string TimeToString(float time)
  {
    int min = Mathf.FloorToInt(time / 60);
    int sec = Mathf.FloorToInt(time % 60);

    return string.Format("{0:00}:{1:00}", min, sec);
  }

  #region BoardSetup

  void SpawnBoard()
  {
    var boardLayout = board.GetComponent<GridLayoutGroup>();
    boardLayout.constraintCount = boardFieldSize;

    //Tile resize
    var tileSize = (board.GetComponent<RectTransform>().rect.width - (boardLayout.padding.top + boardLayout.padding.bottom) - boardFieldSize * (boardLayout.spacing.x + 1)) / boardFieldSize;
    boardLayout.cellSize = new Vector2(tileSize, tileSize);

    //Board resize
    //board.GetComponent<RectTransform>().sizeDelta = new Vector2(boardFieldSize * (boardLayout.spacing.x + 1 + fieldPrefab.GetComponent<RectTransform>().rect.width),
    //                                                            boardFieldSize * (boardLayout.spacing.y + 1 + fieldPrefab.GetComponent<RectTransform>().rect.height));

    boardFieldsNumber = boardFieldSize * boardFieldSize;
    for (int i = 0; i < boardFieldsNumber; i++)
    {
      var newFieldBtn = Instantiate(fieldPrefab, Vector3.zero, Quaternion.identity);
      newFieldBtn.transform.SetParent(board.transform, false);
      newFieldBtn.name = $"Field_{i}";

      var newField = new Field();
      newField.fieldBtn = newFieldBtn;
      newField.fieldIndex = i;


      newFieldBtn.GetComponent<Button>().onClick.AddListener(() => FieldPressed(newField));
      newFieldBtn.GetComponent<Button>().onClick.AddListener(() => TileSoundPressed());
      fieldList.Add(newField);
    }
  }

  void ClearBoard()
  {
    for (int i = 0; i < fieldList.Count; i++)
    {
      var btn = fieldList[i].fieldBtn.GetComponent<Button>();
      btn.interactable = true;
      btn.image.sprite = baseTileSkin;
      fieldList[i].playerValue = 0;
    }
  }

  #endregion

  #region Gameplay

  public void FieldPressed(Field field)
  {
    if (!gameActive) return;
    var fieldBtn = field.fieldBtn.GetComponent<Button>();
    fieldBtn.interactable = false;

    if (activePlayer == 1)
    {
      p1MovesNum++;
      p1MovesTxt.text = p1MovesNum.ToString();
      fieldBtn.image.sprite = playerSkins[selectedTheme*2];
    }
    else
    {
      p2MovesNum++;
      p2MovesTxt.text = p2MovesNum.ToString();
      fieldBtn.image.sprite = playerSkins[selectedTheme * 2 + 1];
    }
    field.playerValue = activePlayer;

    turnsTaken++;
    if(!EndGameChecks(field)) TurnEnd();
  }

  void TurnEnd()
  {
    activePlayer *= -1;
  }

  public bool EndGameChecks(Field field)
  {
    if (CheckTileForVictory(field, Vector2.zero) >= winConditionLength)
    {
      StartCoroutine(EndingStarted(activePlayer));
      gameActive = false;
      return true;
    }
    if (turnsTaken >= boardFieldsNumber)
    {
      StartCoroutine(EndingStarted(0));
      gameActive = false;
      return true;
    }

    return false;
  }

  int CheckTileForVictory(Field currentField, Vector2 dir)
  {
    int sameLength;

    if (dir == Vector2.zero)
    {
      for (int i = 0; i < lines.Length; i += 2)
      {
        int length1 = CheckTileForVictory(currentField, lines[i]);
        int length2 = CheckTileForVictory(currentField, lines[i + 1]);
        sameLength = 1 + length1 + length2;
        if (sameLength >= winConditionLength)
        {
          winTile1 = fieldList[currentField.fieldIndex + (int)(length1 * lines[i].x) + (int)(length1 * boardFieldSize * lines[i].y)].fieldBtn.transform;
          winTile2 = fieldList[currentField.fieldIndex + (int)(length2 * lines[i + 1].x) + (int)(length2 * boardFieldSize * lines[i + 1].y)].fieldBtn.transform;
          return sameLength;
        }
      }
    }
    else
    {
      int nextX = currentField.fieldIndex % boardFieldSize + (int)dir.x;
      int nextY = currentField.fieldIndex / boardFieldSize + (int)dir.y;

      if (nextX < 0 || nextX >= boardFieldSize || nextY < 0 || nextY >= boardFieldSize)
      {
        return 0;
      }

      int nextIndex = nextX + (nextY * boardFieldSize);
      if (fieldList[nextIndex].playerValue == currentField.playerValue) return 1 + CheckTileForVictory(fieldList[nextIndex], dir);
    }

    return 0;
  }

  IEnumerator EndingStarted(int playerWinner)
  {
    SaveManager.Instance.SetScore(playerWinner, timer);
    if(playerWinner != 0)
    {
      DrawEndLine();
      yield return new WaitForSeconds(0.7f);
    }
    EndGame(playerWinner);
  }

  void DrawEndLine()
  {
    endLine.gameObject.SetActive(true);
    endLine.position = (winTile1.position + winTile2.position) / 2f;
    endLine.sizeDelta = new Vector2(Vector3.Distance(winTile1.position, winTile2.position), endLine.sizeDelta.y);

    Vector3 direction = winTile1.position - winTile2.position;
    endLine.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

    AudioManager.Instance.PlaySound("Woosh", 0.03f);
  }

  void EndGame(int playerWinner)
  {
    endPanel.SetActive(true);
    switch (playerWinner)
    {
      case -1:
        endTitleTxt.text = "Player 1 wins";
        endTitleTxt.color = Color.blue;
        break;
      case 0:
        endTitleTxt.text = "Draw";
        endTitleTxt.color = Color.white;
        break;
      case 1:
        endTitleTxt.text = "Player 2 wins";
        endTitleTxt.color = Color.red;
        break;
      default: Debug.LogError($"WRONG PLAYER INT {playerWinner}"); break;
    }
    endTimeTxt.text = TimeToString(timer);
  }

  #endregion

  #region InteractionsUI


  void AdjustBackground(bool mode)
  {
    var bgScale = 1920f / 1080f * 2;
    if (mode)
    {
      bgImg.transform.localScale = new(bgImg.transform.localScale.x / bgScale, bgImg.transform.localScale.y);
      var shape = particleX.shape;
      shape.scale = new Vector3(shape.scale.x / bgScale, shape.scale.y * bgScale, 1f);
      shape = particleO.shape;
      shape.scale = new Vector3(shape.scale.x / bgScale, shape.scale.y * bgScale, 1f);
    }
    else
    {
      bgImg.transform.localScale = new(bgImg.transform.localScale.x * bgScale, bgImg.transform.localScale.y);
      var shape = particleX.shape;
      shape.scale = new Vector3(shape.scale.x * bgScale, shape.scale.y / bgScale, 1f);
      shape = particleO.shape;
      shape.scale = new Vector3(shape.scale.x * bgScale, shape.scale.y / bgScale, 1f);
    }
  }

  void AdjustOrientation(bool mode)
  {
    var cs = GetComponent<CanvasScaler>();
    float baseline = 1920f;
    if (mode)
    {
      Screen.orientation = ScreenOrientation.Portrait;
      cs.referenceResolution = new Vector2(1080f, baseline);
      cs.matchWidthOrHeight = 0;
    }
    else
    {
      Screen.orientation = ScreenOrientation.LandscapeLeft;

      cs.referenceResolution = new Vector2(baseline, 1080f);
      cs.matchWidthOrHeight = 1;
    }
  }

  public void ButtonSoundPressed() => AudioManager.Instance.PlaySound("Click1", 0.03f);
  public void TileSoundPressed() => AudioManager.Instance.PlaySound("Click2", 0.03f);


  public void ReplayBtn()
  {
    ClearBoard();
    ClearUI();
    endLine.gameObject.SetActive(false);
    activePlayer = 1;
    endPanel.SetActive(false);
    gameActive = true;
  }

  void ClearUI()
  {
    turnsTaken = 0;
    p1MovesNum = 0;
    p2MovesNum = 0;
    p1MovesTxt.text = p1MovesNum.ToString();
    p2MovesTxt.text = p2MovesNum.ToString();
    timer = 0;
    timerTxt.text = TimeToString(timer);
  }

  //Settings
  public void MenuBtn() => SceneManager.LoadScene(0);

  public void ToggleSettings()
  {
    musicToggle.SetIsOnWithoutNotify(SaveManager.Instance.musicEnabled);
    sfxToggle.SetIsOnWithoutNotify(SaveManager.Instance.sfxEnabled);
    settingsPanel.SetActive(!settingsPanel.activeSelf);
  }
  public void ToggleMusic()
  {
    AudioManager.Instance.MuteList(AudioManager.Instance.music);
    SaveManager.Instance.ChangeMusicToggle();
  }
  public void ToggleSFX()
  {
    AudioManager.Instance.MuteList(AudioManager.Instance.sfx);
    SaveManager.Instance.ChangeSFXToggle();
  }

  #endregion
}
