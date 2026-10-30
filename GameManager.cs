using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Road Prefabs (0 = düz yol önerilir)")]
    public GameObject[] roadPrefabs;

    [Header("References")]
    public Transform player;

    [Header("Spawn Settings")]
    public int startRoadCount = 6;     // başlangıçta kaç yol
    public float spawnDistance = 70f;  // oyuncu yaklaşınca spawn
    public int maxRoadCount = 10;      // sahnede max yol

    private List<GameObject> activeRoads = new List<GameObject>();
    private Transform lastSpawnPoint;
    private int lastIndex = -1;

    void Start()
    {
        if (roadPrefabs.Length == 0)
        {
            Debug.LogError("Road prefab eklenmemiş!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player atanmadı!");
            return;
        }

        SpawnStartRoads();
    }

    void Update()
    {
        if (lastSpawnPoint == null) return;

        float dist = Vector3.Distance(player.position, lastSpawnPoint.position);

        if (dist < spawnDistance)
        {
            SpawnRoad();
        }
    }

    // başlangıç yolları
    void SpawnStartRoads()
    {
        // ilk yol düz olsun (array 0)
        GameObject firstRoad = Instantiate(roadPrefabs[0], Vector3.zero, Quaternion.identity);
        activeRoads.Add(firstRoad);

        lastSpawnPoint = firstRoad.transform.Find("SpawnPoint");

        if (lastSpawnPoint == null)
        {
            Debug.LogError("Prefab içinde SpawnPoint yok!");
            return;
        }

        for (int i = 1; i < startRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    // yeni yol spawn
    void SpawnRoad()
    {
        GameObject prefab = GetRandomRoad();

        GameObject newRoad = Instantiate(prefab, lastSpawnPoint.position, lastSpawnPoint.rotation);
        activeRoads.Add(newRoad);

        lastSpawnPoint = newRoad.transform.Find("SpawnPoint");

        if (lastSpawnPoint == null)
        {
            Debug.LogError("Yeni prefab SpawnPoint yok!");
            return;
        }

        // performans için eski yolları sil
        if (activeRoads.Count > maxRoadCount)
        {
            Destroy(activeRoads[0]);
            activeRoads.RemoveAt(0);
        }
    }

    // akıllı random yol seçimi
    GameObject GetRandomRoad()
    {
        if (roadPrefabs.Length == 1)
            return roadPrefabs[0];

        int index = Random.Range(0, roadPrefabs.Length);

        // aynı prefab üst üste gelmesin
        int safety = 0;
        while (index == lastIndex && safety < 10)
        {
            index = Random.Range(0, roadPrefabs.Length);
            safety++;
        }

        lastIndex = index;
        return roadPrefabs[index];
    }
}
