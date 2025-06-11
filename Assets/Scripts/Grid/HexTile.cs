using UnityEngine;

public class HexTile : MonoBehaviour
{
    // Hex coordinates (cube coordinates)
    public Vector3Int HexCoordinates { get; private set; }

    // Reference to the grid manager
    private GridManager gridManager;

    // Tile properties
    public enum TerrainType
    {
        Plains,
        Forest,
        Mountain,
        Water,
        Desert
    }

    public TerrainType Terrain { get; set; } = TerrainType.Plains;
    public int MovementCost { get; set; } = 1;
    public bool IsWalkable { get; set; } = true;

    // Unit reference
    public Unit OccupyingUnit { get; set; }

    // Visual elements
    private SpriteRenderer spriteRenderer;
    private GameObject highlightObject;

    public void Initialize(Vector3Int hexCoord, GridManager manager)
    {
        HexCoordinates = hexCoord;
        gridManager = manager;

        // Get or add required components
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            GameObject spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(transform);
            spriteObj.transform.localPosition = Vector3.zero;
            spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
        }

        // Set initial appearance
        UpdateAppearance();
    }

    public void UpdateAppearance()
    {
        // This will be expanded later when we integrate the Tiles and Hexes 2D asset
        // For now, we'll just set a basic color based on terrain type
        if (spriteRenderer != null)
        {
            switch (Terrain)
            {
                case TerrainType.Plains:
                    spriteRenderer.color = Color.green;
                    break;
                case TerrainType.Forest:
                    spriteRenderer.color = new Color(0, 0.5f, 0);
                    break;
                case TerrainType.Mountain:
                    spriteRenderer.color = Color.grey;
                    break;
                case TerrainType.Water:
                    spriteRenderer.color = Color.blue;
                    break;
                case TerrainType.Desert:
                    spriteRenderer.color = Color.yellow;
                    break;
            }
        }
    }

    public void Highlight(bool active, Color color)
    {
        // Create highlight object if it doesn't exist
        if (highlightObject == null)
        {
            highlightObject = new GameObject("Highlight");
            highlightObject.transform.SetParent(transform);
            highlightObject.transform.localPosition = new Vector3(0, 0.1f, 0); // Slightly above the tile
            SpriteRenderer highlightRenderer = highlightObject.AddComponent<SpriteRenderer>();
            highlightRenderer.sprite = spriteRenderer.sprite; // Use same sprite as the tile
            highlightRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // Render above the tile
        }

        // Get the highlight renderer
        SpriteRenderer renderer = highlightObject.GetComponent<SpriteRenderer>();

        // Set active state and color
        highlightObject.SetActive(active);
        if (active)
        {
            renderer.color = new Color(color.r, color.g, color.b, 0.5f); // Semi-transparent
        }
    }

    // Check if this tile is occupied
    public bool IsOccupied()
    {
        return OccupyingUnit != null;
    }

    // Set the occupying unit
    public void SetUnit(Unit unit)
    {
        OccupyingUnit = unit;
    }

    // Clear the occupying unit
    public void ClearUnit()
    {
        OccupyingUnit = null;
    }
}