using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSpawner : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase pointTile;
    [SerializeField] private TileBase roadTile;
    [SerializeField] private TileBase playerTile;

    [Header("Map Size")]
    [SerializeField] private int width = 32;
    [SerializeField] private int height = 32;

    [Header("Subgrid Settings")]
    [SerializeField] private int subGridWidth = 8;
    [SerializeField] private int subGridHeight = 8;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.8f;

    [Header("Player Controller")]
    [SerializeField] private PlayerController playerController; // drag & drop

    private Vector3Int[,] points;

    private void Start()
    {
        GenerateChunk();
        Vector3Int spawnCell = SpawnPlayerOnCenterRoad();

        // повідомляємо PlayerController де спавнити гравця
        if (playerController != null)
            playerController.OnPlayerSpawned(spawnCell);
    }

    public void GenerateChunk()
    {
        tilemap.ClearAllTiles();
        SpawnPoints();
        ConnectPoints();
    }

    private void SpawnPoints()
    {
        int subCountX = width / subGridWidth;
        int subCountY = height / subGridHeight;
        points = new Vector3Int[subCountX, subCountY];

        for (int y = 0; y < subCountY; y++)
        {
            for (int x = 0; x < subCountX; x++)
            {
                if (Random.value > spawnChance)
                {
                    points[x, y] = Vector3Int.zero;
                    continue;
                }

                int startX = x * subGridWidth;
                int startY = y * subGridHeight;

                int randX = Random.Range(startX, startX + subGridWidth);
                int randY = Random.Range(startY, startY + subGridHeight);

                Vector3Int cellPos = new Vector3Int(randX, randY, 0);
                tilemap.SetTile(cellPos, pointTile);
                points[x, y] = cellPos;
            }
        }
    }

    private void ConnectPoints()
    {
        int subCountX = points.GetLength(0);
        int subCountY = points.GetLength(1);

        for (int y = 0; y < subCountY; y++)
        {
            for (int x = 0; x < subCountX; x++)
            {
                Vector3Int current = points[x, y];
                if (current == Vector3Int.zero) continue;

                if (y + 1 < subCountY && points[x, y + 1] != Vector3Int.zero)
                    DrawRoadAxisAligned(current, points[x, y + 1]);

                if (x + 1 < subCountX && points[x + 1, y] != Vector3Int.zero)
                    DrawRoadAxisAligned(current, points[x + 1, y]);
            }
        }
    }

    private void DrawRoadAxisAligned(Vector3Int start, Vector3Int end)
    {
        bool horizontalFirst = Random.value > 0.5f;

        if (horizontalFirst)
        {
            Vector3Int corner = new Vector3Int(end.x, start.y, 0);
            DrawLine(start, corner);
            DrawLine(corner, end);
        }
        else
        {
            Vector3Int corner = new Vector3Int(start.x, end.y, 0);
            DrawLine(start, corner);
            DrawLine(corner, end);
        }
    }

    private void DrawLine(Vector3Int start, Vector3Int end)
    {
        int x = start.x;
        int y = start.y;

        int dx = end.x - start.x;
        int dy = end.y - start.y;

        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        while (x != end.x || y != end.y)
        {
            Vector3Int pos = new Vector3Int(x, y, 0);
            if (tilemap.GetTile(pos) == null)
                tilemap.SetTile(pos, roadTile);

            if (x != end.x) x += stepX;
            else if (y != end.y) y += stepY;
        }
    }

    public Vector3Int SpawnPlayerOnCenterRoad()
    {
        Vector3Int center = new Vector3Int(width / 2, height / 2, 0);

        int searchRadius = 1;
        while (true)
        {
            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    Vector3Int checkCell = new Vector3Int(center.x + dx, center.y + dy, 0);
                    if (tilemap.GetTile(checkCell) == roadTile)
                    {
                        tilemap.SetTile(checkCell, playerTile);
                        return checkCell;
                    }
                }
            }
            searchRadius++;
            if (searchRadius > 100)
            {
                Debug.LogError("Не знайдено дорогу для спавну плеєра!");
                return Vector3Int.zero;
            }
        }
    }
}
