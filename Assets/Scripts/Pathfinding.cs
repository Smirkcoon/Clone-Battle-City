using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding GetInstance { get; private set; }

    private GridCells<PathCell> grid;
    private List<PathCell> openList;
    private List<PathCell> closedList;

    /// <summary>
    /// Initialization of the Grid with path cells to find the path
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Pathfinding(int width, int height)
    {
        GetInstance = this;
        grid = new GridCells<PathCell>(width, height, 1, Vector3.zero, (GridCells<PathCell> g, int x, int y) => new PathCell(g, x, y));
    }
    /// <summary>
    /// Call it when need route
    /// convert Vector3 into coordinates given the size of the cell
    /// </summary>
    /// <param name="startWorldPosition"></param>
    /// <param name="endWorldPosition"></param>
    /// <returns></returns>
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);
        List<PathCell> path = FindPath(startX, startY, endX, endY);
        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathCell PathCell in path)
            {
                vectorPath.Add(new Vector3(PathCell.x, PathCell.y));
            }
            return vectorPath;
        }
    }
    /// <summary>
    /// Create an open list of PathCell for routing
    /// </summary>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <returns></returns>
    private List<PathCell> FindPath(int startX, int startY, int endX, int endY)
    {
        PathCell startCell = grid.GetGridObject(startX, startY);
        PathCell endCell = grid.GetGridObject(endX, endY);

        if (startCell == null || endCell == null)
        {
            Debug.LogError("Invalid Path");
            return null;
        }

        openList = new List<PathCell> { startCell };
        closedList = new List<PathCell>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathCell PathCell = grid.GetGridObject(x, y);
                PathCell.gCost = 99999999;
                PathCell.CalculateFCost();
                PathCell.cameFromCell = null;
            }
        }

        startCell.gCost = 0;
        startCell.hCost = CalculateDistanceCost(startCell, endCell);
        startCell.CalculateFCost();

        while (openList.Count > 0)
        {
            PathCell currentCell = GetLowestFCostCell(openList);
            if (currentCell == endCell)
            {
                // Reached final Cel
                return CalculatePath(endCell);
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);
            foreach (PathCell neighbourCell in GetNeighbourList(currentCell))
            {
                if (closedList.Contains(neighbourCell))
                    continue;

                if (!neighbourCell.isWalkable && endCell != neighbourCell)
                {
                    closedList.Add(neighbourCell);
                    continue;
                }

                int tentativeGCost = currentCell.gCost + CalculateDistanceCost(currentCell, neighbourCell);
                if (tentativeGCost < neighbourCell.gCost)
                {
                    neighbourCell.cameFromCell = currentCell;
                    neighbourCell.gCost = tentativeGCost;
                    neighbourCell.hCost = CalculateDistanceCost(neighbourCell, endCell);
                    neighbourCell.CalculateFCost();
                    if (!openList.Contains(neighbourCell))
                    {
                        openList.Add(neighbourCell);
                    }
                }
            }
        }

        // Out of Cells on the openList
        return null;
    }

    /// <summary>
    /// collecting Neighbour PathCell from the current one
    /// </summary>
    /// <param name="currentCell"></param>
    /// <returns></returns>
    private List<PathCell> GetNeighbourList(PathCell currentCell)
    {
        List<PathCell> neighbourList = new List<PathCell>();

        if (currentCell.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetCell(currentCell.x - 1, currentCell.y));
        }
        if (currentCell.x + 1 < grid.GetWidth())
        {
            // Right
            neighbourList.Add(GetCell(currentCell.x + 1, currentCell.y));
        }
        // Down
        if (currentCell.y - 1 >= 0) neighbourList.Add(GetCell(currentCell.x, currentCell.y - 1));
        // Up
        if (currentCell.y + 1 < grid.GetHeight()) neighbourList.Add(GetCell(currentCell.x, currentCell.y + 1));

        return neighbourList;
    }
    /// <summary>
    /// getting a PathCell by coordinates
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public PathCell GetCell(int x, int y) {
        return grid.GetGridObject(x, y);
    }

    private List<PathCell> CalculatePath(PathCell endCell)
    {
        List<PathCell> path = new List<PathCell>();
        path.Add(endCell);
        PathCell currentCell = endCell;
        while (currentCell.cameFromCell != null)
        {
            path.Add(currentCell.cameFromCell);
            currentCell = currentCell.cameFromCell;
        }
        path.Reverse();
        return path;
    }
    /// <summary>
    /// Calculate Distance Cost
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int CalculateDistanceCost(PathCell a, PathCell b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }
    /// <summary>
    /// getting a PathCell with Lowest F Cost
    /// </summary>
    /// <param name="PathCellList"></param>
    /// <returns></returns>
    private PathCell GetLowestFCostCell(List<PathCell> PathCellList)
    {
        PathCell lowestFCostCell = PathCellList[0];
        for (int i = 1; i < PathCellList.Count; i++)
        {
            if (PathCellList[i].fCost < lowestFCostCell.fCost)
            {
                lowestFCostCell = PathCellList[i];
            }
        }
        return lowestFCostCell;
    }

}
