using Mirror;
using UnityEngine;

public class ServerInitializer : NetworkBehaviour
{
    [Header("要在服务器启动时生成的预制体列表")]
    public GameObject[] startupPrefabs;

    [Header("对应的生成位置(可留空表示默认预制体自带位置)")]
    public Transform[] spawnPoints;

    /// <summary>
    /// 只在服务器端执行一次：生成所有预制体并 NetworkServer.Spawn
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        int count = Mathf.Min(startupPrefabs.Length,
                              spawnPoints != null ? spawnPoints.Length : startupPrefabs.Length);

        for (int i = 0; i < count; i++)
        {
            // 1. 实例化
            Vector3 pos = spawnPoints != null && spawnPoints.Length > i
                          ? spawnPoints[i].position
                          : Vector3.zero;
            Quaternion rot = spawnPoints != null && spawnPoints.Length > i
                             ? spawnPoints[i].rotation
                             : Quaternion.identity;

            GameObject go = Instantiate(startupPrefabs[i], pos, rot);

            // 2. （可选）如果是可选中单位，初始化归属到服务器（netId=0）
            var sel = go.GetComponent<Selectable>();
            if (sel != null)
            {
                sel.InitializeOwner(0u);  // 0u 代表服务器“玩家”（或中立）
            }

            // 3. 网络生成，所有客户端都会收到
            NetworkServer.Spawn(go);
        }

        Debug.Log($"[ServerInitializer] Spawned {count} startup prefabs on server.");
    }
}
