using UnityEngine;

public class XSpawner : MonoBehaviour
{
    public GameObject xPrefab;
    public Transform player;

    public float minDistance = 20f;
    public float maxDistance = 40f;

    public float roadWidth = 2.7f;

    private float nextSpawnZ = 30f;

    void Update()
    {
        if (player.position.z + maxDistance > nextSpawnZ)
        {
            SpawnX();
        }
    }

    void SpawnX()
    {
        float randomX = Random.Range(-roadWidth, roadWidth);

        Vector3 spawnPos = new Vector3(randomX, 1f, nextSpawnZ);

        Instantiate(xPrefab, spawnPos, Quaternion.identity);

        nextSpawnZ += Random.Range(minDistance, maxDistance);
    }
}