using UnityEngine;

public class AirNode
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int gridZ;

    public AirNode(bool _isWalkable, Vector3 _worldPos, int _gridX, int _gridY, int _gridZ)
    {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        gridZ = _gridZ;
    }
}
