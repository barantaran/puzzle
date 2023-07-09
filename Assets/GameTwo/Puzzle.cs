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

    //Three-dimansional integer array to store the puzzle
    public int[,,] puzzle;

    //Puzzle size
    public int Size;

    //Cell cuve prefab
    public GameObject CellCube;

    // Start is called before the first frame update
    void Start()
    {

        //Find by tag the puzzle game object and move it to the center of the puzzle according to its size
        GameObject puzzleCore = GameObject.FindGameObjectWithTag(Tags.Puzzle.ToString());
        puzzleCore.transform.position = new Vector3(Size / 2, Size / 2, Size / 2);

        //initialize the puzzle
        puzzle = new int[Size, Size, Size];

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

                    if (puzzle[i, j, k] == 0)
                    {
                        continue;
                    }

                    //Spawn a cube for the cell
                    GameObject cube = Instantiate(CellCube, new Vector3(x, y, z), Quaternion.identity);

                    //Set the cube's parent to this game object
                    //cube.transform.parent = transform;

                    //Set the cube's name to its position in the puzzle
                    cube.name = GenerateCellNameFromIndicies(i, j, k);


                }
            }
        }
    }

    private static string GenerateCellNameFromIndicies(int i, int j, int k)
    {
        return i + "," + j + "," + k;
    }

    // Update is called once per frame
    void Update()
    {
        //Fill random cell in puzzle with 0 or 1 on any key press
        if (Input.anyKeyDown)
        {
            //Gets closest empty cell to the center of the puzzle
            int[] closestCell = FindClosestZeroCell(puzzle);

            //Fill the cell with 1
            puzzle[closestCell[0], closestCell[1], closestCell[2]] = 1;

            //Get randomly -1 or 1
            int randomDirection = UnityEngine.Random.Range(0, 2) * 2 - 1;

            Debug.DrawRay(new Vector3(closestCell[0], closestCell[1], closestCell[2]), GetRandomAxis() * randomDirection * Size, Color.red, 1);

            //Destroy all the cubes in Scene by tag PuzzleCube
            GameObject[] cubes = GameObject.FindGameObjectsWithTag(Tags.PuzzleCube.ToString());
            foreach (GameObject cube in cubes)
            {
                Destroy(cube);
            }

            //Display the puzzle
            DisplayPuzzle();
        }


        //string cellNameUnderMouse = GetCellNameUnderMouse();

        //if(cellNameUnderMouse != null)
        //{
        //    Debug.Log(cellNameUnderMouse);
        //}

        ChangeColorUnderMouse();
    }

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

    /// <summary>
    /// Return emty cell in puzzle as close to the center as possible
    ///</summary>
    public int[] FindClosestZeroCell(int[,,] matrix)
    {
        int centerX = matrix.GetLength(0) / 2;
        int centerY = matrix.GetLength(1) / 2;
        int centerZ = matrix.GetLength(2) / 2;

        int minDistance = int.MaxValue;
        int[] closestCell = null;

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
                            closestCell = new int[] { x, y, z };
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
    private void OnDrawGizmos()
    {
        //Draw ray from camera through mouse position at distance 10 for delta time
        Gizmos.color = Color.red;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.DrawRay(ray.origin, ray.direction * 10);

        //draw shpere each 0.5f on ray
        Gizmos.color = Color.green;
        for (int i = 0; i < 50; i++)
        {
            Gizmos.DrawWireSphere(ray.origin + ray.direction * i * 0.5f, 0.1f);
        }
    }

    /// <summary>
    /// Return the cell indices from Vector3 position
    /// </summary>
    public int[] GetCellIndicesFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        int z = Mathf.RoundToInt(position.z);

        return new int[] { x, y, z };
    }

    public int[] GetFirstNonEmptyCell(Vector3[] lookUpPoints)
    {
       //Iterate through all the look up points
       foreach (Vector3 point in lookUpPoints)
        {
            //Get the cell indices from the point
            int[] indices = GetCellIndicesFromPosition(point);
            if(CheckIndices(indices) == false)
            {
                continue;
            }
            //Check if the cell is empty
            if (puzzle[indices[0], indices[1], indices[2]] == 1)
            {
                //Return the indices of the cell
                return indices;
            }
        }

       //Return null if no non-empty cell is found
       return null;
    }   

    /// <summary>
    /// Returns Vector3 array of points along the ray from the camera through the mouse position
    /// each step on distance
    /// 
    public Vector3[] GetLookUpPoints(float step, float distance)
    {
        //Get the ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Create array of points
        Vector3[] points = new Vector3[Mathf.RoundToInt(distance / step)];

        //Iterate through the points
        for (int i = 0; i < points.Length; i++)
        {
            //Get the position of the point
            Vector3 point = ray.origin + ray.direction * i * step;

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
        Vector3[] lookUpPoints = GetLookUpPoints(0.5f, 30f);
        int[] indices = GetFirstNonEmptyCell(lookUpPoints);

        if (indices == null)
        {
            return null;
        }

        //Return the name of the cell
        return GenerateCellNameFromIndicies(indices[0], indices[1], indices[2]);
    }

    /// <summary>
    /// Checks if indicies are within the bounds of the puzzle
    /// 
    public bool CheckIndices(int[] indices)
    {
        if (indices[0] < 0 || indices[0] >= puzzle.GetLength(0) ||
                       indices[1] < 0 || indices[1] >= puzzle.GetLength(1) ||
                                  indices[2] < 0 || indices[2] >= puzzle.GetLength(2))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Find cube under mouse using GetCellNameUnderMouse position and change its color
    /// </summary>
    /// 

    public void ChangeColorUnderMouse()
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
            cellCube.GetComponent<Renderer>().material.color = Color.red;
        }
    }

}
