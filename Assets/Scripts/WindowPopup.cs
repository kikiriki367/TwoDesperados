using UnityEngine;
using System.Collections;

public class WindowPopup : MonoBehaviour
{
  [SerializeField] AnimationCurve transitionCurve;
  [SerializeField] float aniamtionTime = 0.7f;

  private void OnEnable() => StartCoroutine(AnimatePanel());

  IEnumerator AnimatePanel()
  {
    float timer = 0f;
    Vector3 vec = new(1, 1, 0);
    while (timer < aniamtionTime)
    {
      timer += Time.deltaTime;
      float percentage = timer / aniamtionTime;
      transform.localScale = vec * transitionCurve.Evaluate(percentage);

      yield return null;
    }
    transform.localScale = vec * transitionCurve.Evaluate(1f);
    AudioManager.Instance.PlaySound("Pop", 0.03f);
  }
}
