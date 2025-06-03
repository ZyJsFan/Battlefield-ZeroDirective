using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private void OnEnable()
    {
        // ������ק��ʼ�����̣������¼�
        InputManager.OnDragStart += LogDragStart;
        InputManager.OnDrag += LogDragging;
        InputManager.OnDragEnd += LogDragEnd;

        // �����Ҽ������¼�
        InputManager.OnCommandIssued += LogCommandIssued;
    }

    private void OnDisable()
    {
        InputManager.OnDragStart -= LogDragStart;
        InputManager.OnDrag -= LogDragging;
        InputManager.OnDragEnd -= LogDragEnd;
        InputManager.OnCommandIssued -= LogCommandIssued;
    }

    private void LogDragStart(Vector2 screenPos)
    {
       // Debug.Log($"[DebugLogger] ��ק��ʼ at {screenPos}");
    }

    private void LogDragging(Vector2 screenPos)
    {
       // Debug.Log($"[DebugLogger] ��ק�� at {screenPos}");
    }

    private void LogDragEnd(Vector2 screenPos)
    {
        //Debug.Log($"[DebugLogger] ��ק���� at {screenPos}");
    }

    private void LogCommandIssued(RaycastHit hit)
    {
        var hitPoint = hit.point;
        var hitName = hit.collider != null ? hit.collider.name : "None";
        Debug.Log($"[DebugLogger] ���� issued at {hitPoint}, hit object: {hitName}");
    }
}
