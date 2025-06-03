using Mirror;
using UnityEngine;

public class ServerInitializer : NetworkBehaviour
{
    [Header("Ҫ�ڷ���������ʱ���ɵ�Ԥ�����б�")]
    public GameObject[] startupPrefabs;

    [Header("��Ӧ������λ��(�����ձ�ʾĬ��Ԥ�����Դ�λ��)")]
    public Transform[] spawnPoints;

    /// <summary>
    /// ֻ�ڷ�������ִ��һ�Σ���������Ԥ���岢 NetworkServer.Spawn
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        int count = Mathf.Min(startupPrefabs.Length,
                              spawnPoints != null ? spawnPoints.Length : startupPrefabs.Length);

        for (int i = 0; i < count; i++)
        {
            // 1. ʵ����
            Vector3 pos = spawnPoints != null && spawnPoints.Length > i
                          ? spawnPoints[i].position
                          : Vector3.zero;
            Quaternion rot = spawnPoints != null && spawnPoints.Length > i
                             ? spawnPoints[i].rotation
                             : Quaternion.identity;

            GameObject go = Instantiate(startupPrefabs[i], pos, rot);

            // 2. ����ѡ������ǿ�ѡ�е�λ����ʼ����������������netId=0��
            var sel = go.GetComponent<Selectable>();
            if (sel != null)
            {
                sel.InitializeOwner(0u);  // 0u �������������ҡ�����������
            }

            // 3. �������ɣ����пͻ��˶����յ�
            NetworkServer.Spawn(go);
        }

        Debug.Log($"[ServerInitializer] Spawned {count} startup prefabs on server.");
    }
}
