using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainUI : MonoBehaviour
{
  [SerializeField] GameObject mainPanel, playPanel, StatsPanel, settingsPanel, quitPanel;
  GameObject previousPanel, activePanel;

  [SerializeField] Image[] themeSelectedImage;
  [SerializeField] TextMeshProUGUI boardTxt, winConTxt;

  [SerializeField] Toggle musicToggle, sfxToggle;

  [SerializeField] GameObject bgImg;
  [SerializeField] ParticleSystem particleX, particleO;

  private void Start()
  {
    if ((!SaveManager.Instance.portraitMode && GetComponent<CanvasScaler>().referenceResolution.x == 1080f) 
      || (SaveManager.Instance.portraitMode && GetComponent<CanvasScaler>().referenceResolution.x == 1920f))
    {
      AdjustBackground(SaveManager.Instance.portraitMode);
      AdjustOrientation(SaveManager.Instance.portraitMode);
    }
  }

  public void ButtonSoundPressed() => AudioManager.Instance.PlaySound("Click1", 0.03f);

  //Main Menu
  public void PlayBtn()
  {
    SwapPanels(mainPanel, playPanel);

    boardTxt.text = $"Board Size:{SaveManager.Instance.boardSizeSelected + 3}";
    winConTxt.text = $"Win Con:{SaveManager.Instance.winConSelected + 3}";
    themeSelectedImage[SaveManager.Instance.themeActive].enabled = true;
  }
  public void StatsBtn() => SwapPanels(mainPanel, StatsPanel);
  public void SettingsBtn()
  {
    musicToggle.SetIsOnWithoutNotify(SaveManager.Instance.musicEnabled);
    sfxToggle.SetIsOnWithoutNotify(SaveManager.Instance.sfxEnabled);
    SwapPanels(mainPanel, settingsPanel);
  }
  public void QuitPanelBtn() => SwapPanels(mainPanel, quitPanel);
  //Play Menu
  public void StartBtn() => SceneManager.LoadScene(1);
  public void BoardSizeChange(int i)
  {
    SaveManager.Instance.ChangeBoardSize(i);
    boardTxt.text = $"Board Size:{SaveManager.Instance.boardSizeSelected + 3}";
  }
  public void WinConChange(int i)
  {
    SaveManager.Instance.ChangeWinCon(i);
    winConTxt.text = $"Win Con:{SaveManager.Instance.winConSelected + 3}";
  }
  public void ThemeSelect(int i)
  {
    themeSelectedImage[SaveManager.Instance.themeActive].enabled = false;
    SaveManager.Instance.ChangeActiveTheme(i);
    themeSelectedImage[SaveManager.Instance.themeActive].enabled = true;
  }
  //Settings
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
  //Misc
  public void BackBtn() => SwapPanels(activePanel, previousPanel);
  public void QuitBtn() => Application.Quit();

  
  public void ChangeOrientationBtn()
  {
    SaveManager.Instance.ChangePortraitMode(!SaveManager.Instance.portraitMode);
    AdjustBackground(SaveManager.Instance.portraitMode);
    AdjustOrientation(SaveManager.Instance.portraitMode);
  }

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

  void SwapPanels(GameObject oldPanel, GameObject newPanel)
  {
    previousPanel = oldPanel;
    activePanel = newPanel;
    oldPanel.SetActive(false);
    newPanel.SetActive(true);
  }
}
