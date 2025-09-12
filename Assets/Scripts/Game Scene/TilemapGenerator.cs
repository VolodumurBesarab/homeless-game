using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSpawner : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase pointTile;
    [SerializeField] private TileBase roadTile;
    [SerializeField] private TileBase playerTile;
    [SerializeField] private TileBase grassTile;

    [Header("Chunk Settings")]
    [SerializeField] private int width = 32;
    [SerializeField] private int height = 32;
    [SerializeField] private int subGridWidth = 8;
    [SerializeField] private int subGridHeight = 8;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.8f;

    [Header("Player Controller")]
    [SerializeField] private PlayerController playerController; // drag & drop

    private Dictionary<Vector2Int, bool> spawnedChunks = new Dictionary<Vector2Int, bool>();
    private Vector3Int playerCell;

    private void Start()
    {
        // Спавнимо гравця після першого чанку
        Vector2Int initialChunk = Vector2Int.zero;
        GenerateChunkAt(initialChunk);

        Vector3Int spawnCell = SpawnPlayerOnCenterRoad(initialChunk);
        playerCell = spawnCell;

        if (playerController != null)
            playerController.OnPlayerSpawned(spawnCell);
    }

    private void Update()
    {
        if (playerCell == Vector3Int.zero) return;

        // Генеруємо сусідні чанки навколо гравця
        // Vector2Int playerChunk = GetPlayerChunk(playerCell);
        // for (int dx = -1; dx <= 1; dx++)
        // {
        //     for (int dy = -1; dy <= 1; dy++)
        //     {
        //         Vector2Int chunkToSpawn = playerChunk + new Vector2Int(dx, dy);
        //         GenerateChunkAt(chunkToSpawn);
        //     }
        // }
    }

    private Vector2Int GetPlayerChunk(Vector3Int cell)
    {
        int chunkX = cell.x / width;
        int chunkY = cell.y / height;
        return new Vector2Int(chunkX, chunkY);
    }

    private void GenerateChunkAt(Vector2Int chunkPos)
    {
        if (spawnedChunks.ContainsKey(chunkPos)) return;

        int offsetX = chunkPos.x * width;
        int offsetY = chunkPos.y * height;

        Vector3Int[,] points = new Vector3Int[width / subGridWidth, height / subGridHeight];
        SpawnPoints(offsetX, offsetY, points);
        ConnectPoints(offsetX, offsetY, points);
        ConnectIsolatedPoints(points);
        FillGrass(offsetX, offsetY);

        spawnedChunks[chunkPos] = true;
    }

    private void SpawnPoints(int offsetX, int offsetY, Vector3Int[,] points)
    {
        int subCountX = points.GetLength(0);
        int subCountY = points.GetLength(1);

        for (int y = 0; y < subCountY; y++)
        {
            for (int x = 0; x < subCountX; x++)
            {
                if (Random.value > spawnChance)
                {
                    points[x, y] = Vector3Int.zero;
                    continue;
                }

                int randX = Random.Range(offsetX + x * subGridWidth, offsetX + (x + 1) * subGridWidth);
                int randY = Random.Range(offsetY + y * subGridHeight, offsetY + (y + 1) * subGridHeight);

                Vector3Int cellPos = new Vector3Int(randX, randY, 0);
                tilemap.SetTile(cellPos, pointTile);
                points[x, y] = cellPos;
            }
        }
    }

    private void ConnectPoints(int offsetX, int offsetY, Vector3Int[,] points)
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

    private Vector3Int SpawnPlayerOnCenterRoad(Vector2Int chunk)
    {
        int offsetX = chunk.x * width;
        int offsetY = chunk.y * height;
        Vector3Int center = new Vector3Int(offsetX + width / 2, offsetY + height / 2, 0);

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

    private void FillGrass(int offsetX, int offsetY)
    {
        for (int x = offsetX; x < offsetX + width; x++)
        {
            for (int y = offsetY; y < offsetY + height; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (tilemap.GetTile(cell) == null)
                    tilemap.SetTile(cell, grassTile);
            }
        }
    }

    private void ConnectIsolatedPoints(Vector3Int[,] points)
{
    int subCountX = points.GetLength(0);
    int subCountY = points.GetLength(1);

    for (int y = 0; y < subCountY; y++)
    {
        for (int x = 0; x < subCountX; x++)
        {
            Vector3Int current = points[x, y];
            if (current == Vector3Int.zero) continue;

            bool connected = false;

            // перевіряємо, чи хоча б одна сусідня точка є і з'єднана
            if (y + 1 < subCountY && points[x, y + 1] != Vector3Int.zero) connected = true;
            if (x + 1 < subCountX && points[x + 1, y] != Vector3Int.zero) connected = true;
            if (y - 1 >= 0 && points[x, y - 1] != Vector3Int.zero) connected = true;
            if (x - 1 >= 0 && points[x - 1, y] != Vector3Int.zero) connected = true;

            if (!connected)
            {
                // з’єднуємо з **найближчою іншою точкою на мапі**
                Vector3Int nearest = FindNearestPoint(current, points);
                if (nearest != Vector3Int.zero)
                    DrawRoadAxisAligned(current, nearest);
            }
        }
    }
}

    private Vector3Int FindNearestPoint(Vector3Int point, Vector3Int[,] points)
    {
        Vector3Int nearest = Vector3Int.zero;
        float minDist = float.MaxValue;

        int subCountX = points.GetLength(0);
        int subCountY = points.GetLength(1);

        for (int y = 0; y < subCountY; y++)
            {
                for (int x = 0; x < subCountX; x++)
                {
                    Vector3Int other = points[x, y];
                    if (other == Vector3Int.zero || other == point) continue;

                    float dist = Vector3Int.Distance(point, other);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = other;
                    }
                }
            }
        return nearest; 
    }
}
