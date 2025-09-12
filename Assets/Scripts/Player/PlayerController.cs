using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase roadTile;
    [SerializeField] private TileBase playerTile;
    [SerializeField] private Camera mainCamera;

    private Vector3Int playerCell;
    private bool initialized = false;

    [Header("Camera Settings")]
    [SerializeField] private float cameraSmoothTime = 0.1f; // швидкість плавності камери
    private Vector3 cameraVelocity = Vector3.zero;

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

    private void LateUpdate()
    {
        if (!initialized || mainCamera == null) return;

        Vector3 targetPos = tilemap.CellToWorld(playerCell) + new Vector3(0.5f, 0.5f, -10);
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPos, ref cameraVelocity, cameraSmoothTime);
    }

    private void TryMove(Vector3Int direction)
    {
        Vector3Int nextCell = playerCell + direction;

        if (tilemap.GetTile(nextCell) == roadTile)
        {
            tilemap.SetTile(playerCell, roadTile); // стара клітинка стає дорогою
            tilemap.SetTile(nextCell, playerTile); // нова клітинка — гравець
            playerCell = nextCell;
        }
    }
}
