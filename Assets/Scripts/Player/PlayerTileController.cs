using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase roadTile;
    [SerializeField] private TileBase playerTile;

    private Vector3Int playerCell;

    private bool initialized = false;

    // викликається GridSpawner після спавну плеєра
    public void OnPlayerSpawned(Vector3Int cell)
    {
        playerCell = cell;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        Vector3Int direction = Vector3Int.zero;

        if (Keyboard.current.wKey.wasPressedThisFrame) direction = Vector3Int.up;
        if (Keyboard.current.sKey.wasPressedThisFrame) direction = Vector3Int.down;
        if (Keyboard.current.aKey.wasPressedThisFrame) direction = Vector3Int.left;
        if (Keyboard.current.dKey.wasPressedThisFrame) direction = Vector3Int.right;

        if (direction != Vector3Int.zero)
            TryMove(direction);
    }

    private void TryMove(Vector3Int direction)
    {
        Vector3Int nextCell = playerCell + direction;

        if (tilemap.GetTile(nextCell) == roadTile)
        {
            tilemap.SetTile(playerCell, roadTile);   // стара клітинка стає дорогою
            tilemap.SetTile(nextCell, playerTile);   // нова клітинка — гравець
            playerCell = nextCell;
        }
    }
}
