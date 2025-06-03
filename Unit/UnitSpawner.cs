using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// ͨ�õķ������˵�λ��������
/// �����κ���Ҫ��ʱ�򣨱��� GameFlowManager���¼������������� Spawn�� ������
/// </summary>
public class UnitSpawner : NetworkBehaviour
{
    /// <summary>
    /// �������ɣ�ָ���ͻ��ˡ�Ԥ���塢λ�úͳ���
    /// �������ɵ� GameObject ���ã����������� Spawn����
    /// </summary>
    [Server]
    public GameObject SpawnUnitForClient(NetworkConnectionToClient conn, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 1. �ڷ�������ʵ����
        var go = Instantiate(prefab, position, rotation);

        // 2. ������ Selectable ��ʼ������
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
