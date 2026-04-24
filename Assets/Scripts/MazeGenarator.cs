using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class MazeGenarator : MonoBehaviour
{
    [SerializeField]
    private MazeCell _mazeCellPrefab;

    [SerializeField] 
    private int _mazeWidth;
    
    [SerializeField]
    private int _mazeDepth;
    
    private MazeCell[,]  _mazeGrid;
    
    [SerializeField] 
    private GameObject _pickupPrefab;
    
    [SerializeField] 
    [Range(0, 1)] private float _spawnChance = 0.1f; // 10% chance
    
    [SerializeField]
    private GameObject _exitPrefab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
{
    // --- PHASE 1: ENVIRONMENT SETUP ---
    // Automatically scale and center the Plane under the maze
    GameObject floorPlane = GameObject.Find("Plane");
    if (floorPlane != null)
    {
        // Divide by 10 because Plane base size is 10x10
        floorPlane.transform.localScale = new Vector3(_mazeWidth / 10f, 1f, _mazeDepth / 10f);
    
        // Center the plane under the grid. 
        // We subtract 0.5f because cells are centered on whole integers.
        float centerX = (_mazeWidth / 2f) - 0.5f;
        float centerZ = (_mazeDepth / 2f) - 0.5f;
    
        floorPlane.transform.position = new Vector3(centerX, -0.01f, centerZ);
    }
    
    // --- PHASE 2: BASE GENERATION ---
    // Build the initial grid of solid cells
    _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];
    for (int x = 0; x < _mazeWidth; x++)
    {
        for (int z = 0; z < _mazeDepth; z++)
        {
            _mazeGrid[x, z] = Instantiate(_mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
        }
    }

    // --- PHASE 3: PATH CARVING ---
    // Run the recursive algorithm to carve the main maze paths
    yield return GenerateMaze(null, _mazeGrid[0, 0]);

    // --- PHASE 4: LEVEL MODIFICATION (SHORTCUTS) ---
    // You MUST break these walls BEFORE baking the NavMesh, otherwise 
    // the AI won't see the new paths and will get stuck on invisible barriers.
    for (int i = 0; i < 5; i++) 
    {
        int rx = Random.Range(1, _mazeWidth - 1);
        int rz = Random.Range(1, _mazeDepth - 1);
        
        // Clear both sides of the shared wall boundary
        if (_mazeGrid[rx, rz] != null && _mazeGrid[rx - 1, rz] != null)
        {
            _mazeGrid[rx, rz].ClearLeftWall();          // Clear current cell's left wall
            _mazeGrid[rx - 1, rz].ClearRightWall();     // Clear neighbor's right wall
        }
        
        // Draw a giant green beam shooting straight up from the shortcut location
        Vector3 shortcutPos = _mazeGrid[rx, rz].transform.position;
        Debug.DrawRay(shortcutPos, Vector3.up * 30f, Color.green, 1000f);
    }

    // --- PHASE 5: NAVMESH BAKING ---
    // Bake the NavMesh now that all the shortcuts are physically cleared.
    GetComponent<Unity.AI.Navigation.NavMeshSurface>().BuildNavMesh();

    // --- PHASE 6: ENTITY PLACEMENT ---
    // 6A. Warp the Enemy AFTER the bake so it lands on a valid NavMesh point
    GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
    if (enemy != null)
    {
        // For a 20x20 maze, this picks a spot between 5 and 15 (Mid-Zone)
        int randX = Random.Range(_mazeWidth / 4, (_mazeWidth / 4) * 3);
        int randZ = Random.Range(_mazeDepth / 4, (_mazeDepth / 4) * 3);
        
        Vector3 spawnPos = new Vector3(randX, 0.5f, randZ);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPos); // Physically snap agent to the baked floor
            agent.enabled = true; // Turn the brain on
        }
        Debug.Log("Enemy safely deployed in Mid-Maze at: " + spawnPos);
    }

    // 6B. Spawn Exit at the far corner
    Instantiate(_exitPrefab, new Vector3(_mazeWidth - 1, 0, _mazeDepth - 1), Quaternion.identity);
}

    private IEnumerator GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        // Only spawn if it's not the starting cell (0,0) and matches the chance
        if (Random.value < _spawnChance && currentCell.transform.position != Vector3.zero) 
        {
            Instantiate(_pickupPrefab, currentCell.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }
        
        yield return new WaitForSeconds(0.005f);

        MazeCell nextCell;

        while ((nextCell = GetNextUnvisitedCell(currentCell)) != null)
        {
            
            yield return GenerateMaze(currentCell, nextCell);
        }
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        // var unvisitedCells = GetAllUnvisitedCells(currentCell);

        return GetAllUnvisitedCells(currentCell)
            .OrderBy(_ => Random.value)
            .FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetAllUnvisitedCells(MazeCell currentCell)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        if (x + 1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x + 1, z];

            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }
        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];
            
            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }
        if (z + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, z + 1];
            
            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        if (z - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, z-  1];

            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }
        
        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }
        
        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
