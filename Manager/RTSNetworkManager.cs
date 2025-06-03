using Mirror;
using UnityEngine;

public class RTSNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        // ����ѡ����ʼ�� GameFlowManager ����
        GameFlowManager.Instance.InitializeOnServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 1) ���� Mirror ��Ĭ���߼����� Player prefab
        base.OnServerAddPlayer(conn);

        // 2) �õ��� spawn ������ GetReady ���
        var player = conn.identity.GetComponent<GetReady>();

        // 3) ֪ͨ GameFlowManager ע����������
        GameFlowManager.Instance.RegisterPlayer(player);
    }
}
