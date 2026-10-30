using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class RoadAutoSetupTool
{
    private const string SpawnPointName = "SpawnPoint";

    // Yol baþlangýç/bitiþ dilimini bulmak için tolerans.
    // Eðer yanlýþ nokta bulursa 0.25 yerine 0.5 deneyebilirsin.
    private const float SliceTolerance = 0.5f;

    // Bitiþ yönünü bulmak için sondan ne kadar önce örnek alýnacak.
    private const float DirectionSampleDistance = 2.0f;

    [MenuItem("Tools/Road Tools/Set Start To Origin And Create SpawnPoint")]
    public static void SetupRoad()
    {
        GameObject roadRoot = Selection.activeGameObject;

        if (roadRoot == null)
        {
            Debug.LogError("Önce Hierarchy'den yol root objesini seçmelisin.");
            return;
        }

        MeshFilter[] meshFilters = roadRoot.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            Debug.LogError("Seçilen objenin altýnda MeshFilter bulunamadý.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(roadRoot, "Road Auto Setup");

        List<Vector3> vertices = CollectVerticesInRootLocal(roadRoot, meshFilters);

        if (vertices.Count == 0)
        {
            Debug.LogError("Vertex bulunamadý.");
            return;
        }

        float minZ = GetMinZ(vertices);
        float maxZ = GetMaxZ(vertices);

        Vector3 startCenter = AverageVerticesNearZ(vertices, minZ, SliceTolerance);
        Vector3 endCenter = AverageVerticesNearZ(vertices, maxZ, SliceTolerance);

        // Baþlangýcý local (0,0,0) yapmak için tüm mesh child'larýný ters yönde kaydýrýyoruz.
        MoveRoadChildrenByOffset(roadRoot, -startCenter);

        // Mesh hareket ettiði için bitiþ noktasý da ayný offset kadar deðiþti.
        Vector3 correctedEndCenter = endCenter - startCenter;

        // Bitiþ yönünü hesaplamak için vertexleri yeniden oku.
        meshFilters = roadRoot.GetComponentsInChildren<MeshFilter>();
        vertices = CollectVerticesInRootLocal(roadRoot, meshFilters);

        minZ = GetMinZ(vertices);
        maxZ = GetMaxZ(vertices);

        correctedEndCenter = AverageVerticesNearZ(vertices, maxZ, SliceTolerance);

        float sampleZ = maxZ - DirectionSampleDistance;
        Vector3 beforeEndCenter = AverageVerticesNearZ(vertices, sampleZ, SliceTolerance);

        Vector3 endDirection = correctedEndCenter - beforeEndCenter;

        // Aþaðý eðim yüzünden SpawnPoint'in X rotasyonu bozulmasýn.
        // Sadece yatay düzlemde yön hesaplýyoruz.
        endDirection.y = 0f;

        if (endDirection.sqrMagnitude < 0.0001f)
        {
            endDirection = Vector3.forward;
        }

        endDirection.Normalize();

        Transform spawnPoint = roadRoot.transform.Find(SpawnPointName);

        if (spawnPoint == null)
        {
            GameObject sp = new GameObject(SpawnPointName);
            spawnPoint = sp.transform;
            Undo.RegisterCreatedObjectUndo(sp, "Create SpawnPoint");
        }

        spawnPoint.SetParent(roadRoot.transform, false);

        spawnPoint.localPosition = correctedEndCenter;
        spawnPoint.localRotation = Quaternion.LookRotation(endDirection, Vector3.up);
        spawnPoint.localScale = Vector3.one;

        // Root'u temiz býrakýyoruz.
        roadRoot.transform.localPosition = Vector3.zero;
        roadRoot.transform.localRotation = Quaternion.identity;
        roadRoot.transform.localScale = Vector3.one;

        EditorUtility.SetDirty(roadRoot);
        EditorUtility.SetDirty(spawnPoint.gameObject);

        Debug.Log(
            "Road setup tamamlandý.\n" +
            "Road: " + roadRoot.name + "\n" +
            "Start artýk local (0,0,0).\n" +
            "SpawnPoint Local Position: " + spawnPoint.localPosition + "\n" +
            "SpawnPoint Local Rotation: " + spawnPoint.localEulerAngles
        );
    }

    private static List<Vector3> CollectVerticesInRootLocal(GameObject root, MeshFilter[] meshFilters)
    {
        List<Vector3> result = new List<Vector3>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null)
                continue;

            if (mf.transform.name == SpawnPointName)
                continue;

            Vector3[] meshVertices = mf.sharedMesh.vertices;

            foreach (Vector3 v in meshVertices)
            {
                Vector3 worldPoint = mf.transform.TransformPoint(v);
                Vector3 rootLocalPoint = root.transform.InverseTransformPoint(worldPoint);
                result.Add(rootLocalPoint);
            }
        }

        return result;
    }

    private static void MoveRoadChildrenByOffset(GameObject root, Vector3 offset)
    {
        foreach (Transform child in root.transform)
        {
            if (child.name == SpawnPointName)
                continue;

            child.localPosition += offset;
        }
    }

    private static float GetMinZ(List<Vector3> vertices)
    {
        float value = float.MaxValue;

        foreach (Vector3 v in vertices)
        {
            if (v.z < value)
                value = v.z;
        }

        return value;
    }

    private static float GetMaxZ(List<Vector3> vertices)
    {
        float value = float.MinValue;

        foreach (Vector3 v in vertices)
        {
            if (v.z > value)
                value = v.z;
        }

        return value;
    }

    private static Vector3 AverageVerticesNearZ(List<Vector3> vertices, float targetZ, float tolerance)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Vector3 v in vertices)
        {
            if (Mathf.Abs(v.z - targetZ) <= tolerance)
            {
                sum += v;
                count++;
            }
        }

        if (count > 0)
            return sum / count;

        Vector3 closest = vertices[0];
        float closestDistance = Mathf.Abs(vertices[0].z - targetZ);

        foreach (Vector3 v in vertices)
        {
            float d = Mathf.Abs(v.z - targetZ);

            if (d < closestDistance)
            {
                closest = v;
                closestDistance = d;
            }
        }

        return closest;
    }
}