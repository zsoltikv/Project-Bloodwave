using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSetup : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap overlayTilemap;

    [Header("Overlay Tiles")]
    public TileBase[] overlayTiles;
    [Range(0f, 1f)] public float overlayChance = 0.15f;

    [Header("Props Behind (GameObjects)")]
    public GameObject[] propsBehindPrefabs;
    [Range(0f, 1f)] public float propsBehindChance = 0.05f;
    public Transform propsBehindParent;

    [Header("Props Front (GameObjects)")]
    public GameObject[] propsFrontPrefabs;
    [Range(0f, 1f)] public float propsFrontChance = 0.03f;
    public Transform propsFrontParent;
    public Transform playerFeet;

    [Header("Props Front Spacing")]
    public float minFrontPropDistance = 0.8f;

    [Header("Props Behind Spacing")]
    public float minBehindToFrontDistance = 0.6f;

    [Header("Random Offset")]
    [Range(0f, 0.5f)] public float maxOffset = 0.25f;

    // runtime tracking
    List<Vector3> spawnedFrontPropPositions = new List<Vector3>();

    void Awake()
    {
        GenerateAll();
    }

    [ContextMenu("Generate All")]
    public void GenerateAll()
    {
        groundTilemap.CompressBounds();

        overlayTilemap.ClearAllTiles();

        ClearChildren(propsBehindParent);
        ClearChildren(propsFrontParent);

        spawnedFrontPropPositions.Clear();

        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (groundTilemap.GetTile(pos) == null)
                continue;

            if (!IsFullySurrounded(pos))
                continue;

            TryPlaceOverlay(pos);
            TryPlaceProps(pos);
        }
    }

    // ---------- OVERLAY ----------
    void TryPlaceOverlay(Vector3Int pos)
    {
        if (Random.value > overlayChance)
            return;

        PlaceOverlayTile(pos);
    }

    void PlaceOverlayTile(Vector3Int pos)
    {
        if (overlayTiles == null || overlayTiles.Length == 0)
            return;

        TileBase tile = overlayTiles[Random.Range(0, overlayTiles.Length)];
        overlayTilemap.SetTile(pos, tile);

        int rot = Random.Range(0, 4) * 90;

        Vector3 offset = new Vector3(
            Random.Range(-maxOffset, maxOffset),
            Random.Range(-maxOffset, maxOffset),
            0f
        );

        Matrix4x4 matrix = Matrix4x4.TRS(
            offset,
            Quaternion.Euler(0, 0, rot),
            Vector3.one
        );

        overlayTilemap.SetTransformMatrix(pos, matrix);
    }

    // ---------- PROPS ----------
    void TryPlaceProps(Vector3Int pos)
    {
        float roll = Random.value;

        if (roll < propsBehindChance)
        {
            SpawnBehindProp(pos);
        }
        else if (roll < propsBehindChance + propsFrontChance)
        {
            SpawnFrontProp(pos);
        }
    }

    // ---------- BEHIND PROP ----------
    void SpawnBehindProp(Vector3Int cellPos)
    {
        if (propsBehindPrefabs == null || propsBehindPrefabs.Length == 0)
            return;

        Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellPos);

        if (IsTooCloseToFrontProps(worldPos))
            return;

        GameObject prefab =
            propsBehindPrefabs[Random.Range(0, propsBehindPrefabs.Length)];

        Instantiate(
            prefab,
            worldPos,
            Quaternion.identity,
            propsBehindParent
        );
    }

    // ---------- FRONT PROP ----------
    void SpawnFrontProp(Vector3Int cellPos)
    {
        if (propsFrontPrefabs == null || propsFrontPrefabs.Length == 0)
            return;

        Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellPos);

        if (!CanSpawnFrontProp(worldPos))
            return;

        GameObject prefab =
            propsFrontPrefabs[Random.Range(0, propsFrontPrefabs.Length)];

        GameObject instance =
            Instantiate(
                prefab,
                worldPos,
                Quaternion.identity,
                propsFrontParent
            );

        spawnedFrontPropPositions.Add(worldPos);
    }

    // ---------- SPACING ----------
    bool CanSpawnFrontProp(Vector3 position)
    {
        foreach (var existingPos in spawnedFrontPropPositions)
        {
            if (Vector3.Distance(existingPos, position) < minFrontPropDistance)
                return false;
        }

        return true;
    }

    // ---------- HELPERS ----------
    void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    bool IsTooCloseToFrontProps(Vector3 position)
    {
        foreach (var frontPos in spawnedFrontPropPositions)
        {
            if (Vector3.Distance(frontPos, position) < minBehindToFrontDistance)
                return true;
        }

        return false;
    }

    // ---------- SURROUND CHECK ----------
    bool IsFullySurrounded(Vector3Int pos)
    {
        Vector3Int[] neighbors =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,

            new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, -1, 0)
        };

        foreach (var dir in neighbors)
        {
            if (groundTilemap.GetTile(pos + dir) == null)
                return false;
        }

        return true;
    }
}
