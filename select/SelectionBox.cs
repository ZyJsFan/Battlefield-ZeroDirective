using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SelectionBox : MonoBehaviour
{
    public Canvas parentCanvas;             // 在 Inspector 拖入上面创建的 Canvas
    private RectTransform canvasRect;       // Canvas 的 RectTransform
    private RectTransform rt;               // 这个 SelectionBox Image 的 RectTransform

    private Vector2 startScreen;            // 记录屏幕坐标起点
    private Vector2 startLocal;             // 记录转换后的 UI 本地坐标起点

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasRect = parentCanvas.GetComponent<RectTransform>();
        gameObject.SetActive(false);

        InputManager.OnDragStart += BeginDrag;
        InputManager.OnDrag += DuringDrag;
        InputManager.OnDragEnd += EndDrag;
    }

    void OnDestroy()
    {
        InputManager.OnDragStart -= BeginDrag;
        InputManager.OnDrag -= DuringDrag;
        InputManager.OnDragEnd -= EndDrag;
    }

    private void BeginDrag(Vector2 screenPos)
    {
        startScreen = screenPos;
        // 屏幕坐标 → Canvas 本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, parentCanvas.worldCamera, out startLocal);

        // 激活 Image，重置 RectTransform
        gameObject.SetActive(true);
        rt.anchoredPosition = startLocal;
        rt.sizeDelta = Vector2.zero;
    }

    private void DuringDrag(Vector2 screenPos)
    {
        // 实时更新大小和位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, parentCanvas.worldCamera, out Vector2 currLocal);

        Vector2 delta = currLocal - startLocal;
        rt.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        rt.anchoredPosition = startLocal + delta * 0.5f;
    }

    private void EndDrag(Vector2 screenPos)
    {
        // 隐藏选框
        gameObject.SetActive(false);
        // 通知 SelectionProcessor：用原始屏幕坐标开始/结束
        SelectionProcessor.Instance.ProcessScreenRect(startScreen, screenPos);
    }
}
