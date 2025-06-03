using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SelectionBox : MonoBehaviour
{
    public Canvas parentCanvas;             // �� Inspector �������洴���� Canvas
    private RectTransform canvasRect;       // Canvas �� RectTransform
    private RectTransform rt;               // ��� SelectionBox Image �� RectTransform

    private Vector2 startScreen;            // ��¼��Ļ�������
    private Vector2 startLocal;             // ��¼ת����� UI �����������

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
        // ��Ļ���� �� Canvas ��������
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, parentCanvas.worldCamera, out startLocal);

        // ���� Image������ RectTransform
        gameObject.SetActive(true);
        rt.anchoredPosition = startLocal;
        rt.sizeDelta = Vector2.zero;
    }

    private void DuringDrag(Vector2 screenPos)
    {
        // ʵʱ���´�С��λ��
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, parentCanvas.worldCamera, out Vector2 currLocal);

        Vector2 delta = currLocal - startLocal;
        rt.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        rt.anchoredPosition = startLocal + delta * 0.5f;
    }

    private void EndDrag(Vector2 screenPos)
    {
        // ����ѡ��
        gameObject.SetActive(false);
        // ֪ͨ SelectionProcessor����ԭʼ��Ļ���꿪ʼ/����
        SelectionProcessor.Instance.ProcessScreenRect(startScreen, screenPos);
    }
}
