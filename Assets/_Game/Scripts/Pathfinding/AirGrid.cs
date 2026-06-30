using UnityEngine;
using System.Collections.Generic;

public class AirGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public LayerMask obstacleMask;
    public Vector3 gridWorldSize;
    public float nodeRadius;
    public AirNode[,,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY, gridSizeZ;

    [Header("Debug")]
    public bool displayGridGizmos;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new AirNode[gridSizeX, gridSizeY, gridSizeZ];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2 - Vector3.forward * gridWorldSize.z / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius);
                    // Use a slightly smaller sphere to avoid false positives on adjacent nodes
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius * 0.9f, obstacleMask));
                    grid[x, y, z] = new AirNode(walkable, worldPoint, x, y, z);
                }
            }
        }
    }

    public AirNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y - transform.position.y + gridWorldSize.y / 2) / gridWorldSize.y;
        float percentZ = (worldPosition.z - transform.position.z + gridWorldSize.z / 2) / gridWorldSize.z;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);
        
        // Ensure within bounds
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || z < 0 || z >= gridSizeZ) return null;
        
        return grid[x, y, z];
    }

    public List<AirNode> GetNeighbors(AirNode node)
    {
        List<AirNode> neighbors = new List<AirNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY && checkZ >= 0 && checkZ < gridSizeZ)
                    {
                        neighbors.Add(grid[checkX, checkY, checkZ]);
                    }
                }
            }
        }

        return neighbors;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, gridWorldSize);
        if (grid != null && displayGridGizmos)
        {
            foreach (AirNode n in grid)
            {
                Gizmos.color = n.isWalkable ? new Color(0, 1, 0, 0.2f) : new Color(1, 0, 0, 0.5f);
                if (!n.isWalkable) Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
