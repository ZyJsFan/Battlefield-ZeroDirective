using Mirror;
using UnityEngine;

public class RTSNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        // （可选）初始化 GameFlowManager 单例
        GameFlowManager.Instance.InitializeOnServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 1) 先让 Mirror 的默认逻辑生成 Player prefab
        base.OnServerAddPlayer(conn);

        // 2) 拿到刚 spawn 出来的 GetReady 组件
        var player = conn.identity.GetComponent<GetReady>();

        // 3) 通知 GameFlowManager 注册这个新玩家
        GameFlowManager.Instance.RegisterPlayer(player);
    }
}
