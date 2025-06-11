using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float hexSize = 1f;
    
    [Header("Prefabs")]
    public GameObject hexTilePrefab;
    
    // Dictionary to store our hex tiles
    private Dictionary<Vector3Int, HexTile> hexTiles = new Dictionary<Vector3Int, HexTile>();
    
    // Constants for hex calculations
    private readonly float sqrt3 = Mathf.Sqrt(3);
    
    public void InitializeGrid()
    {
        Debug.Log("Initializing hex grid...");
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        // Clear any existing grid
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        hexTiles.Clear();

        Debug.Log($"Creating grid with width {gridWidth} and height {gridHeight}");

        // For a proper 2D hex grid with point-top orientation
        int halfWidth = gridWidth / 2;
        int halfHeight = gridHeight / 2;

        for (int r = -halfHeight; r <= halfHeight; r++)
        {
            // In a point-top hex grid, each row is offset
            int qStart = -halfWidth - Mathf.Min(0, r);
            int qEnd = halfWidth - Mathf.Max(0, r);

            for (int q = qStart; q <= qEnd; q++)
            {
                int s = -q - r; // Maintain the cube coordinate constraint: q + r + s = 0
                Vector3Int hexCoord = new Vector3Int(q, r, s);
                CreateHexTile(hexCoord);
            }
        }
    }

    private void CreateHexTile(Vector3Int hexCoord)
    {
        // Convert hex coordinates to world position
        Vector3 worldPos = HexToWorldPosition(hexCoord);
        
        // Instantiate the hex tile prefab
        GameObject tileObject = Instantiate(hexTilePrefab, worldPos, Quaternion.identity, transform);
        tileObject.name = $"Hex_{hexCoord.x}{hexCoord.y}{hexCoord.z}";
        
        // Add the HexTile component if it doesn't exist
        HexTile hexTile = tileObject.GetComponent<HexTile>();
        if (hexTile == null)
        {
            hexTile = tileObject.AddComponent<HexTile>();
        }
        
        // Initialize the hex tile
        hexTile.Initialize(hexCoord, this);
        
        // Store in our dictionary
        hexTiles[hexCoord] = hexTile;
    }

    // Convert hex coordinates to world position (pointy-top orientation)
    public Vector3 HexToWorldPosition(Vector3Int hexCoord)
    {
        // For point-top hexagons, we need to adjust the spacing
        // These constants determine the spacing between hexes
        float horizontalSpacing = hexSize * 0.75f;
        float verticalSpacing = hexSize * 0.866f; // sqrt(3)/2

        float x = horizontalSpacing * (2 * hexCoord.x + hexCoord.y);
        float z = verticalSpacing * hexCoord.y;

        return new Vector3(x, 0, z);
    }


    // Convert world position to hex coordinates
    public Vector3Int WorldToHexPosition(Vector3 worldPos)
    {
        float q = (sqrt3/3f * worldPos.x - 1f/3f * worldPos.z) / hexSize;
        float r = (2f/3f * worldPos.z) / hexSize;
        
        return CubeRound(new Vector3(q, -q-r, r));
    }
    
    // Round floating point cube coordinates to the nearest hex
    private Vector3Int CubeRound(Vector3 cubeCoord)
    {
        float rx = Mathf.Round(cubeCoord.x);
        float ry = Mathf.Round(cubeCoord.y);
        float rz = Mathf.Round(cubeCoord.z);
        
        float xDiff = Mathf.Abs(rx - cubeCoord.x);
        float yDiff = Mathf.Abs(ry - cubeCoord.y);
        float zDiff = Mathf.Abs(rz - cubeCoord.z);
        
        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }
        
        return new Vector3Int(Mathf.RoundToInt(rx), Mathf.RoundToInt(ry), Mathf.RoundToInt(rz));
    }
    
    // Get a hex tile at specific coordinates
    public HexTile GetTileAt(Vector3Int hexCoord)
    {
        if (hexTiles.TryGetValue(hexCoord, out HexTile tile))
        {
            return tile;
        }
        return null;
    }
    
    // Get all neighbors of a hex
    public List<HexTile> GetNeighbors(Vector3Int hexCoord)
    {
        List<HexTile> neighbors = new List<HexTile>();
        
        // The 6 directions in cube coordinates
        Vector3Int[] directions = {
            new Vector3Int(1, -1, 0),
            new Vector3Int(1, 0, -1),
            new Vector3Int(0, 1, -1),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(0, -1, 1)
        };
        
        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborCoord = hexCoord + dir;
            HexTile neighbor = GetTileAt(neighborCoord);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    public void ClearGrid()
    {
        // Destroy all hex tiles
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Clear the dictionary
        hexTiles.Clear();
    }

}