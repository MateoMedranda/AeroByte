using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

public class AStar3D : MonoBehaviour
{
    private AirGrid grid;

    private class NodeRecord
    {
        public AirNode node;
        public int gCost;
        public int hCost;
        public int fCost { get { return gCost + hCost; } }
    }

    private struct PathResult 
    {
        public List<AirNode> path;
        public Action<List<AirNode>> callback;
    }

    private ConcurrentQueue<PathResult> resultsQueue = new ConcurrentQueue<PathResult>();

    void Awake()
    {
        grid = GetComponent<AirGrid>();
    }

    void Update() 
    {
        while (resultsQueue.TryDequeue(out PathResult result)) 
        {
            result.callback?.Invoke(result.path);
        }
    }

    public void RequestPath(Vector3 startPos, Vector3 targetPos, Vector3 startingForward, Action<List<AirNode>> callback)
    {
        AirNode startNode = grid.NodeFromWorldPoint(startPos);
        AirNode targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null) 
        {
            callback?.Invoke(null);
            return;
        }

        if (!startNode.isWalkable || !targetNode.isWalkable) 
        {
            if (!targetNode.isWalkable) targetNode = GetNearestWalkableNode(targetNode);
            if (!startNode.isWalkable) startNode = GetNearestWalkableNode(startNode);
            if (startNode == null || targetNode == null) { callback?.Invoke(null); return; }
        }

        Task.Run(() => {
            List<AirNode> path = FindPathInternal(startNode, targetNode, startingForward);
            if (path != null && path.Count > 0) {
                path = SmoothPath(path);
            }
            resultsQueue.Enqueue(new PathResult { path = path, callback = callback });
        });
    }

    private List<AirNode> FindPathInternal(AirNode startNode, AirNode targetNode, Vector3 startingForward)
    {
        Dictionary<AirNode, NodeRecord> records = new Dictionary<AirNode, NodeRecord>();
        Dictionary<AirNode, AirNode> parentMap = new Dictionary<AirNode, AirNode>();
        List<NodeRecord> openSet = new List<NodeRecord>();
        HashSet<AirNode> closedSet = new HashSet<AirNode>();

        NodeRecord startRecord = new NodeRecord { node = startNode, gCost = 0, hCost = GetDistance(startNode, targetNode) };
        records[startNode] = startRecord;
        openSet.Add(startRecord);

        while (openSet.Count > 0)
        {
            NodeRecord currentRecord = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentRecord.fCost || (openSet[i].fCost == currentRecord.fCost && openSet[i].hCost < currentRecord.hCost))
                {
                    currentRecord = openSet[i];
                }
            }

            openSet.Remove(currentRecord);
            closedSet.Add(currentRecord.node);

            if (currentRecord.node == targetNode)
            {
                return RetracePath(startNode, targetNode, parentMap);
            }

            foreach (AirNode neighbor in grid.GetNeighbors(currentRecord.node))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentRecord.gCost + GetDistance(currentRecord.node, neighbor);

                // Penalty for sharp turns to encourage planes to fly straight
                Vector3 currentDir = Vector3.zero;
                if (currentRecord.node == startNode && startingForward != Vector3.zero) {
                    currentDir = startingForward.normalized;
                } else if (parentMap.ContainsKey(currentRecord.node)) {
                    currentDir = (currentRecord.node.worldPosition - parentMap[currentRecord.node].worldPosition).normalized;
                }

                if (currentDir != Vector3.zero) {
                    Vector3 nextDir = (neighbor.worldPosition - currentRecord.node.worldPosition).normalized;
                    float dot = Vector3.Dot(currentDir, nextDir);
                    if (dot < 0f) newMovementCostToNeighbor += 500; // Huge penalty for flying backwards
                    else if (dot < 0.3f) newMovementCostToNeighbor += 50; 
                }

                NodeRecord neighborRecord;
                if (!records.TryGetValue(neighbor, out neighborRecord)) {
                    neighborRecord = new NodeRecord { node = neighbor };
                    records[neighbor] = neighborRecord;
                }

                if (newMovementCostToNeighbor < neighborRecord.gCost || !openSet.Contains(neighborRecord))
                {
                    neighborRecord.gCost = newMovementCostToNeighbor;
                    neighborRecord.hCost = GetDistance(neighbor, targetNode);
                    parentMap[neighbor] = currentRecord.node;

                    if (!openSet.Contains(neighborRecord)) openSet.Add(neighborRecord);
                }
            }
        }
        return null;
    }

    private List<AirNode> RetracePath(AirNode startNode, AirNode endNode, Dictionary<AirNode, AirNode> parentMap)
    {
        List<AirNode> path = new List<AirNode>();
        AirNode currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            if (parentMap.ContainsKey(currentNode)) {
                currentNode = parentMap[currentNode];
            } else {
                break;
            }
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }

    private List<AirNode> SmoothPath(List<AirNode> originalPath)
    {
        if (originalPath == null || originalPath.Count <= 2) return originalPath;

        List<AirNode> smoothedPath = new List<AirNode>();
        smoothedPath.Add(originalPath[0]);
        
        int currentIndex = 0;
        
        while (currentIndex < originalPath.Count - 1)
        {
            int farthestVisibleIndex = currentIndex + 1;
            
            for (int i = originalPath.Count - 1; i > currentIndex; i--)
            {
                if (HasLineOfSight(originalPath[currentIndex], originalPath[i]))
                {
                    farthestVisibleIndex = i;
                    break;
                }
            }
            
            smoothedPath.Add(originalPath[farthestVisibleIndex]);
            currentIndex = farthestVisibleIndex;
        }
        
        return smoothedPath;
    }

    private bool HasLineOfSight(AirNode from, AirNode to)
    {
        float x = from.gridX;
        float y = from.gridY;
        float z = from.gridZ;

        float dx = to.gridX - from.gridX;
        float dy = to.gridY - from.gridY;
        float dz = to.gridZ - from.gridZ;

        int steps = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dz)));
        if (steps == 0) return true;

        float xInc = dx / steps;
        float yInc = dy / steps;
        float zInc = dz / steps;

        for (int i = 1; i < steps; i++)
        {
            x += xInc;
            y += yInc;
            z += zInc;

            int gridX = Mathf.RoundToInt(x);
            int gridY = Mathf.RoundToInt(y);
            int gridZ = Mathf.RoundToInt(z);
            
            if (gridX >= 0 && gridX < grid.grid.GetLength(0) &&
                gridY >= 0 && gridY < grid.grid.GetLength(1) &&
                gridZ >= 0 && gridZ < grid.grid.GetLength(2)) 
            {
                if (!grid.grid[gridX, gridY, gridZ].isWalkable) return false;
            }
        }
        return true;
    }

    int GetDistance(AirNode nodeA, AirNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        if (dstX > dstY && dstX > dstZ)
            return 14 * Mathf.Max(dstY, dstZ) + 10 * (dstX - Mathf.Max(dstY, dstZ));
        if (dstY > dstX && dstY > dstZ)
            return 14 * Mathf.Max(dstX, dstZ) + 10 * (dstY - Mathf.Max(dstX, dstZ));
            
        return 14 * Mathf.Max(dstX, dstY) + 10 * (dstZ - Mathf.Max(dstX, dstY));
    }
    
    AirNode GetNearestWalkableNode(AirNode node) 
    {
        int radius = 1;
        while(radius < 5) {
            foreach (AirNode neighbor in grid.GetNeighbors(node)) {
                 if (neighbor.isWalkable) return neighbor;
            }
            radius++;
        }
        return null;
    }
}
