using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// 通用的服务器端单位生成器：
/// 可在任何需要的时候（比如 GameFlowManager、事件触发处）调用 Spawn… 方法。
/// </summary>
public class UnitSpawner : NetworkBehaviour
{
    /// <summary>
    /// 单个生成：指定客户端、预制体、位置和朝向。
    /// 返回生成的 GameObject 引用（已在网络上 Spawn）。
    /// </summary>
    [Server]
    public GameObject SpawnUnitForClient(NetworkConnectionToClient conn, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 1. 在服务器上实例化
        var go = Instantiate(prefab, position, rotation);

        // 2. 给它的 Selectable 初始化归属
        var sel = go.GetComponent<Selectable>();
        if (sel != null)
        {
            var playerIdentity = conn.identity;
            sel.InitializeOwner(playerIdentity.netId);
        }
            

        NetworkServer.Spawn(go, conn);

        return go;
    }


    [Server]
    public List<GameObject> SpawnUnitsForClient(NetworkConnectionToClient conn, GameObject prefab, Transform[] spawnPoints)
    {
        var list = new List<GameObject>();
        foreach (var sp in spawnPoints)
        {
            var go = SpawnUnitForClient(conn, prefab, sp.position, sp.rotation);
            list.Add(go);
        }
        return list;
    }


    [Server]
    public List<GameObject> SpawnUnits(NetworkConnectionToClient conn, GameObject[] prefabs, Vector3[] positions, Quaternion[] rotations)
    {
        var list = new List<GameObject>();
        int count = Mathf.Min(prefabs.Length, Mathf.Min(positions.Length, rotations.Length));
        for (int i = 0; i < count; i++)
        {
            var go = SpawnUnitForClient(conn, prefabs[i], positions[i], rotations[i]);
            list.Add(go);
        }
        return list;
    }
}
