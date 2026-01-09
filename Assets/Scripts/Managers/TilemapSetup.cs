using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSetup : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap overlayTilemap;

    public Tilemap propsBehindTilemap;
    public Tilemap propsFrontTilemap;

    [Header("Overlay Tiles")]
    public TileBase[] overlayTiles;
    [Range(0f, 1f)] public float overlayChance = 0.15f;

    [Header("Props Behind Player")]
    public TileBase[] propsBehindTiles;
    [Range(0f, 1f)] public float propsBehindChance = 0.05f;

    [Header("Props In Front of Player")]
    public TileBase[] propsFrontTiles;
    [Range(0f, 1f)] public float propsFrontChance = 0.03f;

    [Header("Random Offset")]
    [Range(0f, 0.5f)] public float maxOffset = 0.25f;

    void Awake()
    {
        GenerateAll();
    }

    [ContextMenu("Generate All")]
    public void GenerateAll()
    {
        overlayTilemap.ClearAllTiles();
        propsBehindTilemap.ClearAllTiles();
        propsFrontTilemap.ClearAllTiles();

        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(pos))
                continue;

            if (!IsFullySurrounded(pos))
                continue;

            TryPlaceOverlay(pos);
            TryPlaceProp(pos);
        }
    }

    // ---------- OVERLAY ----------
    void TryPlaceOverlay(Vector3Int pos)
    {
        if (Random.value > overlayChance)
            return;

        PlaceTile(
            overlayTilemap,
            overlayTiles,
            pos,
            true
        );
    }

    // ---------- PROPS ----------
    void TryPlaceProp(Vector3Int pos)
    {
        float roll = Random.value;

        if (roll < propsBehindChance)
        {
            PlaceTile(
                propsBehindTilemap,
                propsBehindTiles,
                pos,
                false
            );
        }
        else if (roll < propsBehindChance + propsFrontChance)
        {
            PlaceTile(
                propsFrontTilemap,
                propsFrontTiles,
                pos,
                false
            );
        }
    }

    // ---------- CORE PLACEMENT ----------
    void PlaceTile(Tilemap tilemap, TileBase[] tiles, Vector3Int pos, bool allowRotation)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        TileBase tile = tiles[Random.Range(0, tiles.Length)];
        tilemap.SetTile(pos, tile);

        int rot = allowRotation
            ? Random.Range(0, 4) * 90
            : 0;

        Vector3 offset = new Vector3(
            Random.Range(-maxOffset, maxOffset),
            Random.Range(-maxOffset, maxOffset),
            0f
        );

        Matrix4x4 matrix =
            Matrix4x4.TRS(
                offset,
                Quaternion.Euler(0, 0, rot),
                Vector3.one
            );

        tilemap.SetTransformMatrix(pos, matrix);
    }

    // ---------- SURROUND CHECK ----------
    bool IsFullySurrounded(Vector3Int pos)
    {
        Vector3Int[] neighbors =
        {
            pos + Vector3Int.up,
            pos + Vector3Int.down,
            pos + Vector3Int.left,
            pos + Vector3Int.right,

            pos + new Vector3Int(1, 1, 0),
            pos + new Vector3Int(-1, 1, 0),
            pos + new Vector3Int(1, -1, 0),
            pos + new Vector3Int(-1, -1, 0)
        };

        foreach (var n in neighbors)
        {
            if (!groundTilemap.HasTile(n))
                return false;
        }

        return true;
    }
}
