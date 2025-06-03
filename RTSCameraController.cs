using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RTSCameraController : NetworkBehaviour
{

    [SerializeField] 
    private PlayerData playerData;



    private bool isLocked = false;
    private Vector3 lockedPosition;
    private Camera cam;

    void Awake()
    {
        // ���沢���Ͻ�������� Camera
        cam = GetComponent<Camera>();
        cam.enabled = false;
        enabled = false;
    }



    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // ������Ҿ�����ͨ����̬�¼��õ����� GetReady
        GetReady.OnLocalPlayerReady += HandleLocalPlayerReady;
        GameFlowManager.OnGameStartEvent += HandleGameStart;
    }

    private void HandleLocalPlayerReady(GetReady local)
    {
        // ���ġ���Ӫ���䡱�¼���Ҳ������Ϸ��ʽ��ʼ
        //local.OnFactionAssignedEvent += HandleGameStart;
    }

    private void HandleGameStart()
    {
        cam.enabled = true;
        enabled = true;
    }




    void Update()
    {


        if (Input.GetKeyDown(KeyCode.E))
        {
            isLocked = !isLocked;
            if (isLocked)
            {
                lockedPosition = transform.position;
            }
        }

        if (isLocked)
        {
            transform.position = lockedPosition;
            return;
        }



        Vector3 pos = transform.position;

    
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;

 
        if (Input.GetKey("w")) //|| Input.mousePosition.y >= Screen.height - playerData.panBorderThickness
            pos += forward * playerData.panSpeed * Time.deltaTime;
        if (Input.GetKey("s")) //|| Input.mousePosition.y <= playerData.panBorderThickness
            pos -= forward * playerData.panSpeed * Time.deltaTime;
        if (Input.GetKey("d")) //|| Input.mousePosition.x >= Screen.width - playerData.panBorderThickness
            pos += right * playerData.panSpeed * Time.deltaTime;
        if (Input.GetKey("a"))  //|| Input.mousePosition.x <= playerData.panBorderThickness
            pos -= right * playerData.panSpeed * Time.deltaTime;



        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * playerData.scrollSpeed * Time.deltaTime;
        if(pos.y > playerData.minY+1 && pos.y < playerData.maxY-1)
        {
            pos.z += scroll * playerData.scrollSpeed * Time.deltaTime;
        }



        pos.x = Mathf.Clamp(pos.x, playerData.xMin, playerData.xMax);
        pos.y = Mathf.Clamp(pos.y, playerData.minY, playerData.maxY);
        pos.z = Mathf.Clamp(pos.z, playerData.zMin, playerData.zMax);

        transform.position = pos;
    }
}
