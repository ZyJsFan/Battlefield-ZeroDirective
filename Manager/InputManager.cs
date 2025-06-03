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


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // ×ó¼ü¿òÑ¡
        if (Input.GetMouseButtonDown(0)) OnDragStart?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButton(0)) OnDrag?.Invoke(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) OnDragEnd?.Invoke(Input.mousePosition);

        // ÓÒ¼üÃüÁî
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f))
            {
                Debug.Log($"[InputManager] OnCommandIssued hit `{hit.collider.name}` at {hit.point}");
                OnCommandIssued?.Invoke(hit);
            }
        }
    }
}
