using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;      // щоб знати координати гравця
    [SerializeField] private TileBase playerTile;  // тайл плеєра
    [SerializeField] private Vector3 offset = new Vector3(0,0,-10);

    private Vector3Int playerCell;

    private void LateUpdate()
    {
        // шукаємо плеєра на Tilemap, якщо ще не знаємо
        if (playerCell == Vector3Int.zero)
        {
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    if (tilemap.GetTile(cell) == playerTile)
                    {
                        playerCell = cell;
                        break;
                    }
                }
                if (playerCell != Vector3Int.zero) break;
            }
        }

        if (playerCell != Vector3Int.zero)
        {
            Vector3 worldPos = tilemap.CellToWorld(playerCell) + new Vector3(0.5f, 0.5f, 0) + offset;
            transform.position = worldPos;
        }
    }
}
