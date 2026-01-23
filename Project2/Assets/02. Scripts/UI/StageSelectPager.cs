using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelectPager : MonoBehaviour
{
    [Header("Pager")]
    [SerializeField] private RectTransform content;
    [SerializeField] private int pageCount = 3;
    [SerializeField] private float slideSpeed = 12f;

    private int index = 0;
    private float pageWidth;

    private void Start()
    {
        RectTransform viewport = content.parent as RectTransform;
        pageWidth = viewport.rect.width;

        Snap();
    }

    private void Update()
    {
        Vector2 target = new Vector2(-pageWidth * index, content.anchoredPosition.y);
        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, target, Time.unscaledDeltaTime * slideSpeed);
    }

    public void Next()
    {
        index = Mathf.Clamp(index + 1, 0, pageCount - 1);
    }

    public void Prev()
    {
        index = Mathf.Clamp(index - 1, 0, pageCount - 1);
    }

    public void SelectStage(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void Snap()
    {
        content.anchoredPosition = new Vector2(-pageWidth * index, content.anchoredPosition.y);
    }
}
