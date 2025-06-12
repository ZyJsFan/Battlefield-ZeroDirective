using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static event Action<Vector2> OnDragStart;
    public static event Action<Vector2> OnDrag;
    public static event Action<Vector2> OnDragEnd;
    public static event Action<RaycastHit> OnCommandIssued;

    // ���룺�ų� ��Region�� ��һ��
    private int commandMask;

    void Awake()
    {
        // ȷ����� Region Layer ���ָ�����һ��
        commandMask = ~LayerMask.GetMask("Region");
        Debug.Log($"[InputManager] commandMask = {commandMask}");
    }

    void Update()
    {
        // �����ѡ
        if (Input.GetMouseButtonDown(0)) OnDragStart?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButton(0)) OnDrag?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) OnDragEnd?.Invoke(Input.mousePosition);

        // �Ҽ�����
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // ���� commandMask��Region ��� Collider �ᱻ�ų�
            if (Physics.Raycast(ray, out var hit, 1000f, commandMask))
            {
                Debug.Log($"[InputManager] OnCommandIssued hit `{hit.collider.name}` at {hit.point}");
                OnCommandIssued?.Invoke(hit);
            }
            else
            {
                Debug.Log("[InputManager] Raycast hit nothing (Region layer excluded)");
            }
        }
    }
}
