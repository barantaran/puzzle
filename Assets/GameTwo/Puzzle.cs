using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Puzzle : MonoBehaviour
{   //Enum of all the tags in the puzzle
    public enum Tags
    {
        Puzzle,
        PuzzleCube
    }
    public enum Dimension
    {
        X,
        Y,
        Z
    }
    public class MatrixCell
    {
        public int X;
        public int Y;
        public int Z;

        public MatrixCell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    //Three-dimansional integer array to store the puzzle
    public int[,,] PuzzleMatrix;

    public MatrixCell[] Figure;

    //Puzzle size
    public int Size;
    public enum DebugGizmo
    {
        Sphere,
        Cube
    }
    public class DebugMark
    {
        public DebugGizmo Gizmo;
        public Vector3 Position;
        public Color Color;
        public float Size;
        public int Duration;
    }

    public List<DebugMark> DebugMarks = new List<DebugMark>();

    public float RayCastDistance = 50f;

    //Cell cuve prefab
    public GameObject CellCube;

    //Highlighted cell cube GO which is under the mouse cursor right now
    GameObject HighlightedCellCube;

    //Highlighted cell original color which should be restored when the mouse cursor leaves the cell
    Color HighlightedCellOriginalColor;

    // Start is called before the first frame update
    void Start()
    {

        //Find by tag the puzzle game object and move it to the center of the puzzle according to its size
        GameObject puzzleCore = GameObject.FindGameObjectWithTag(Tags.Puzzle.ToString());
        puzzleCore.transform.position = new Vector3(Size / 2, Size / 2, Size / 2);

        //initialize the puzzle
        PuzzleMatrix = new int[Size, Size, Size];

        //Fill the puzzle with 1 or 0 on random cells
        //for (int i = 0; i < Size; i++)
        //{
        //    for (int j = 0; j < Size; j++)
        //    {
        //        for (int k = 0; k < Size; k++)
        //        {
        //            puzzle[i, j, k] = Random.Range(0, 2);
        //        }
        //    }
        //}

        ////Display the puzzle
        //DisplayPuzzle();
    }

    void Update()
    {
        //Fill random cell in puzzle with 0 or 1 on any key press
        if (Input.anyKeyDown)
        {
            //Gets closest empty cell to the center of the puzzle
            MatrixCell closestCell = FindClosestZeroCell(PuzzleMatrix);
            
            //Fill the cell with 1
            PuzzleMatrix[closestCell.X, closestCell.Y, closestCell.Z] = 1;

            //Get randomly -1 or 1
            int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1;

            MatrixCell[] lookupCells = GetCellsAlongDimension(closestCell, GetRandomDimension(), randomDirection);
            //Add debug marks to debug look up cells excluding the first cell
            for (int i = 1; i < lookupCells.Length; i++)
            {
                MatrixCell cell = lookupCells[i];
                DebugMark debugMark = new DebugMark();
                debugMark.Gizmo = DebugGizmo.Cube;
                debugMark.Position = new Vector3(cell.X, cell.Y, cell.Z);
                debugMark.Color = Color.yellow;
                debugMark.Size = 1f;
                debugMark.Duration = 250;
                DebugMarks.Add(debugMark);
            }

            //Destroy all the cubes in Scene by tag PuzzleCube
            GameObject[] cubes = GameObject.FindGameObjectsWithTag(Tags.PuzzleCube.ToString());
            foreach (GameObject cube in cubes)
            {
                Destroy(cube);
            }

            //Display the puzzle
            DisplayPuzzle();
        }

        HighlightUnderMouse();
    }

    /// <summary>
    /// Display the puzzle, spawn a cube for each cell
    /// </summary>
    void DisplayPuzzle()
    {
        //Spawn a cube for each cell
        for (int i = 0; i < Size; i++)
        {
            //Calculate the x position of the cube
            float x = i;

            for (int j = 0; j < Size; j++)
            {
                //Calculate the y position of the cube
                float y = j;

                for (int k = 0; k < Size; k++)
                {
                    //Calculate the z position of the cube
                    float z = k;

                    if (PuzzleMatrix[i, j, k] == 0)
                    {
                        continue;
                    }

                    //Spawn a cube for the cell
                    GameObject cube = Instantiate(CellCube, new Vector3(x, y, z), Quaternion.identity);

                    //Set the cube's parent to this game object
                    //cube.transform.parent = transform;

                    //define the cell
                    MatrixCell cell = new MatrixCell(i, j, k);

                    //Set the cube's name to its position in the puzzle
                    cube.name = GenerateCellName(cell);
                }
            }
        }
    }

    private static string GenerateCellName(MatrixCell cell)
    {
        return cell.X + "," + cell.Y + "," + cell.Z;
    }

    /// <summary>
    /// Generates a random axis represented by a Vector3 object.
    /// </summary>
    /// <returns>A Vector3 representing a random axis.</returns>
    Vector3 GetRandomAxis()
    {
        // Generate a random number between 0 and 2 (inclusive)
        int randomIndex = UnityEngine.Random.Range(0, 3);

        // Select the corresponding axis based on the random index
        switch (randomIndex)
        {
            case 0:
                return Vector3.up;
            case 1:
                return Vector3.right;
            case 2:
                return Vector3.forward;
            default:
                return Vector3.up; // Default to Vector3.up if index is out of range
        }
    }

    ///<!--<summary>-->
    ///Returns random dimension
    ///
    public Dimension GetRandomDimension()
    {
        // Generate a random number between 0 and 2 (inclusive)
        int randomIndex = UnityEngine.Random.Range(0, 3);

        // Select the corresponding axis based on the random index
        switch (randomIndex)
        {
            case 0:
                return Dimension.X;
            case 1:
                return Dimension.Y;
            case 2:
                return Dimension.Z;
            default:
                return Dimension.X; // Default to Vector3.up if index is out of range
        }

    }

    /// <summary>
    /// Return empty cell in puzzle as close to the center as possible
    ///</summary>
    public MatrixCell FindClosestZeroCell(int[,,] matrix)
    {
        int centerX = matrix.GetLength(0) / 2;
        int centerY = matrix.GetLength(1) / 2;
        int centerZ = matrix.GetLength(2) / 2;

        int minDistance = int.MaxValue;
        MatrixCell closestCell = null;

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                for (int z = 0; z < matrix.GetLength(2); z++)
                {
                    if (matrix[x, y, z] == 0)
                    {
                        int distance = Math.Abs(x - centerX) + Math.Abs(y - centerY) + Math.Abs(z - centerZ);
                        if (distance < minDistance)
                        {
                            minDistance = distance;

                            //Store the closest cell
                            closestCell = new MatrixCell(x, y, z);
                        }
                    }
                }
            }
        }

        return closestCell;
    }

    ///<summary>
    ///Get all non-diagonal neighbors of a cell
    ///</summary>
    public List<int> GetNonDiagonalNeighbors(int x, int y, int z, int[,,] matrix)
    {
        int xSize = matrix.GetLength(0);
        int ySize = matrix.GetLength(1);
        int zSize = matrix.GetLength(2);

        List<int> neighbors = new List<int>();

        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                for (int k = z - 1; k <= z + 1; k++)
                {
                    // Skip the current cell and diagonal neighbors
                    if ((i == x && j == y) || (i == x && k == z) || (j == y && k == z))
                        continue;

                    // Check if the neighbor is within valid indices
                    if (i >= 0 && i < xSize && j >= 0 && j < ySize && k >= 0 && k < zSize)
                    {
                        int neighborType = matrix[i, j, k];
                        neighbors.Add(neighborType);
                    }
                }
            }
        }

        return neighbors;
    }

    //<summary>
    ///Get cell position from indices
    ///
    public Vector3 GetCellPositionFromIndex(MatrixCell cell)
    {
        //Returns cell position converting indices to floats
        return new Vector3(cell.X, cell.Y, cell.Z);
    }

    /// <summary>
    /// Return the cell indices from Vector3 position
    /// </summary>
    public MatrixCell GetCellFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);

        //Returns cell
        return new MatrixCell(x, y, z);


    }

    /// <summary>
    /// Returns indicies of first non-empty cell from the look up points
    /// </summary>
    /// <param name="lookUpPoints"></param>
    /// <returns></returns>
    public MatrixCell GetNonEmptyCellFromPoints(Vector3[] lookUpPoints)
    {
       //Iterate through all the look up points
       foreach (Vector3 point in lookUpPoints)
        {
            //Get the cell from the point
            MatrixCell cell = GetCellFromPosition(point);
            if(!InBounds(cell))
            {
                continue;
            }
            //Check if the puzzle cell is > 0
            if (PuzzleMatrix[cell.X, cell.Y, cell.Z] > 0)
            {
                //Return the cell
                return cell;
            }
        }

       //Return null if no non-empty cell is found
       return null;
    }   

    /// <summary>
    /// Returns Vector3 array of points along the ray from the camera through the mouse position
    /// each step on distance
    /// 
    public Vector3[] GetPointsAlongRay(Ray lookupRay, float step, float distance)
    {


        //Create array of points
        Vector3[] points = new Vector3[Mathf.RoundToInt(distance / step)];

        //Iterate through the points
        for (int i = 0; i < points.Length; i++)
        {
            //Get the position of the point
            Vector3 point = lookupRay.origin + lookupRay.direction * i * step;

            //Add the point to the array
            points[i] = point;
        }

        //Return the array of points
        return points;
    }

    /// <summary>
    /// Returns string name of cell which is under mouse position
    /// </summary>
    /// 
    public string GetCellNameUnderMouse()
    {
        //Get the ray from the camera through the mouse position
        Ray lookupRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3[] lookUpPoints = GetPointsAlongRay(lookupRay, 0.5f, RayCastDistance + Size);
        MatrixCell cell = GetNonEmptyCellFromPoints(lookUpPoints);

        if (cell == null)
        {
            return null;
        }

        //Return the name of the cell
        return GenerateCellName(cell);
    }

    /// <summary>
    /// Checks if indicies are within the bounds of the puzzle
    /// 
    public bool InBounds(MatrixCell cell)
    {
        return cell.X >= 0 && cell.X < Size && cell.Y >= 0 && cell.Y < Size && cell.Z >= 0 && cell.Z < Size;

    }

    /// <summary>
    /// Find cube under mouse using GetCellNameUnderMouse position and change its color
    /// </summary>
    /// 
    public void HighlightUnderMouse()
    {
        string cellName = GetCellNameUnderMouse();
        if (cellName == null)
        {
            return;
        }

        //Get game object with the name of the cell
        GameObject cellCube = GameObject.Find(cellName);

        //If found, change its color
        if (cellCube != null)
        {
            if (HighlightedCellCube != null)
            {
                HighlightedCellCube.GetComponent<Renderer>().material.color = HighlightedCellOriginalColor;
            }

            HighlightedCellCube = cellCube;
            HighlightedCellOriginalColor = cellCube.GetComponent<Renderer>().material.color;
            
            cellCube.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    /// 
    /// <summary>
    /// Returns indicies of all cells along the axis from the given indices in given direction
    /// </summary>
    /// 
    public int[] GetIndicesAlongAxis(int[] indices, int axis, int direction)
    {
        //Get the size of the puzzle along the axis
        int size = PuzzleMatrix.GetLength(axis);

        //Create list of indices
        List<int> indicesAlongAxis = new List<int>();

        //Iterate through the cells along the axis
        for (int i = indices[axis] + direction; i >= 0 && i < size; i += direction)
        {
            //Create array of indices
            int[] newIndices = new int[3];

            //Copy the indices
            indices.CopyTo(newIndices, 0);

            //Set the index along the axis
            newIndices[axis] = i;

            //Add the indices to the list
            indicesAlongAxis.Add(newIndices[axis]);
        }

        //Return the list of indices
        return indicesAlongAxis.ToArray();
    }

    public MatrixCell[] GetCellsAlongDimension(MatrixCell cell, Dimension dimension, int direction)
    {
        int xSize = PuzzleMatrix.GetLength(0);
        int ySize = PuzzleMatrix.GetLength(1);
        int zSize = PuzzleMatrix.GetLength(2);

        // Calculate max range along the specified dimension taking into account the direction
        int maxRange = 0;
        switch (dimension)
        {
            case Dimension.X:
                maxRange = direction > 0 ? xSize - cell.X : cell.X + 1;
                break;
            case Dimension.Y:
                maxRange = direction > 0 ? ySize - cell.Y : cell.Y + 1;
                break;
            case Dimension.Z:
                maxRange = direction > 0 ? zSize - cell.Z : cell.Z + 1;
                break;
        }

        // Initialize the 3D array of indices
        MatrixCell[] cells = new MatrixCell[maxRange];

        // Add indices to the 3D array in direction specified
        for (int i = 0; i < maxRange; i++)
        {
            switch (dimension)
            {
                case Dimension.X:
                    cells[i] = new MatrixCell(cell.X + i * direction, cell.Y, cell.Z);
                    break;
                case Dimension.Y:
                    cells[i] = new MatrixCell(cell.X, cell.Y + i * direction, cell.Z);
                    break;
                case Dimension.Z:
                    cells[i] = new MatrixCell(cell.X, cell.Y, cell.Z + i * direction);
                    break;
            }
        }

        return cells;
    }

    public int[] GetFirstNonEmptyCellAlongAxis(int[] indices, int axis)
    {
        //Get the size of the puzzle along the axis
        int size = PuzzleMatrix.GetLength(axis);

        //Iterate through the cells along the axis
        for (int i = indices[axis]; i < size; i++)
        {
            //Create array of indices
            int[] newIndices = new int[3];

            //Copy the indices
            indices.CopyTo(newIndices, 0);

            //Set the index along the axis
            newIndices[axis] = i;

            //Check if the cell is empty
            if (PuzzleMatrix[newIndices[0], newIndices[1], newIndices[2]] == 1)
            {
                //Return the indices of the cell
                return newIndices;
            }
        }

        //Return null if no non-empty cell is found
        return null;
    }

    private void OnDrawGizmos()
    {
        //Draws all DebugMarks, removes marks from the list if duration is exceeded
        for (int i = 0; i < DebugMarks.Count; i++)
        {
            //DebugMark mark = ;
            Gizmos.color = DebugMarks[i].Color;

            switch (DebugMarks[i].Gizmo)
            {
                case DebugGizmo.Cube:
                    Gizmos.DrawWireCube(DebugMarks[i].Position, new Vector3(DebugMarks[i].Size, DebugMarks[i].Size, DebugMarks[i].Size));
                    break;
                case DebugGizmo.Sphere:
                    Gizmos.DrawWireSphere(DebugMarks[i].Position, DebugMarks[i].Size);
                    break;
            }

            DebugMarks[i].Duration--;
            
            if (DebugMarks[i].Duration <= 0)
            {
                DebugMarks.RemoveAt(i);
            }
        }
    }
}
