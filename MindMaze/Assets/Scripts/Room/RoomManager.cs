using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Room Generation")]
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private float roomSize = 10f;
    [SerializeField] private int mazeSize = 5;

    [Header("Visual Settings")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private float emotionIntensity = 1f;
    [SerializeField] private float colorTransitionSpeed = 2f;

    [Header("Gameplay Settings")]
    [SerializeField] private float minDistanceBetweenPuzzles = 15f;
    [SerializeField] private int puzzlesPerLevel = 3;

    // Room tracking
    private Room[,] rooms;
    private List<Vector2Int> puzzleLocations = new List<Vector2Int>();
    private Vector2Int currentPlayerRoom;
    private EmotionConfiguration emotionConfig;

    // Events
    public static event Action<EmotionType> OnRoomEmotionChanged;
    public static event Action<Vector2Int> OnPlayerRoomChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeRoomManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeRoomManager()
    {
        rooms = new Room[mazeSize, mazeSize];
        emotionConfig = Resources.Load<EmotionConfiguration>("EmotionConfig");
        
        if (emotionConfig == null)
        {
            emotionConfig = ScriptableObject.CreateInstance<EmotionConfiguration>();
            emotionConfig.InitializeDefaultEmotions();
            Debug.LogWarning("EmotionConfiguration not found. Using defaults.");
        }
    }

    public void GenerateLevel(int level)
    {
        ClearCurrentLevel();
        GenerateMaze();
        AssignEmotions();
        PlacePuzzles();
        SpawnPlayer();
    }

    private void GenerateMaze()
    {
        // Initialize rooms
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                Vector3 position = new Vector3(x * roomSize, 0, z * roomSize);
                GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity, transform);
                rooms[x, z] = new Room
                {
                    position = new Vector2Int(x, z),
                    gameObject = roomObj,
                    emotion = EmotionType.Fear // Default emotion
                };
            }
        }

        // Generate maze using recursive backtracking
        GenerateMazeRecursive(0, 0, new bool[mazeSize, mazeSize]);
    }

    private void GenerateMazeRecursive(int x, int z, bool[,] visited)
    {
        visited[x, z] = true;

        // Get unvisited neighbors
        List<Vector2Int> neighbors = GetUnvisitedNeighbors(x, z, visited);
        
        // Randomize neighbor order
        for (int i = 0; i < neighbors.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, neighbors.Count);
            Vector2Int temp = neighbors[i];
            neighbors[i] = neighbors[randomIndex];
            neighbors[randomIndex] = temp;
        }

        // Visit each neighbor
        foreach (Vector2Int neighbor in neighbors)
        {
            if (!visited[neighbor.x, neighbor.y])
            {
                // Create passage (remove wall) between current room and neighbor
                CreatePassage(x, z, neighbor.x, neighbor.y);
                GenerateMazeRecursive(neighbor.x, neighbor.y, visited);
            }
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbors(int x, int z, bool[,] visited)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        // Check all four directions
        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            int newX = x + dir.x;
            int newZ = z + dir.y;

            if (IsValidPosition(newX, newZ) && !visited[newX, newZ])
            {
                neighbors.Add(new Vector2Int(newX, newZ));
            }
        }

        return neighbors;
    }

    private void CreatePassage(int x1, int z1, int x2, int z2)
    {
        // Calculate door position
        Vector3 pos1 = rooms[x1, z1].gameObject.transform.position;
        Vector3 pos2 = rooms[x2, z2].gameObject.transform.position;
        Vector3 doorPos = (pos1 + pos2) / 2f;

        // Create door
        GameObject door = Instantiate(doorPrefab, doorPos, Quaternion.identity, transform);

        // Rotate door based on passage direction
        if (Mathf.Abs(x1 - x2) > 0)
        {
            door.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        // Link rooms
        rooms[x1, z1].connectedRooms.Add(rooms[x2, z2]);
        rooms[x2, z2].connectedRooms.Add(rooms[x1, z1]);
    }

    private void AssignEmotions()
    {
        // Get all emotion types
        Array emotions = Enum.GetValues(typeof(EmotionType));
        List<EmotionType> availableEmotions = new List<EmotionType>();

        foreach (EmotionType emotion in emotions)
        {
            availableEmotions.Add(emotion);
        }

        // Shuffle emotions
        for (int i = 0; i < availableEmotions.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, availableEmotions.Count);
            EmotionType temp = availableEmotions[i];
            availableEmotions[i] = availableEmotions[randomIndex];
            availableEmotions[randomIndex] = temp;
        }

        // Assign emotions to rooms
        int emotionIndex = 0;
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                rooms[x, z].emotion = availableEmotions[emotionIndex % availableEmotions.Count];
                UpdateRoomVisuals(x, z);
                emotionIndex++;
            }
        }
    }

    private void PlacePuzzles()
    {
        puzzleLocations.Clear();
        List<Vector2Int> availableRooms = new List<Vector2Int>();

        // Get all room positions
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                availableRooms.Add(new Vector2Int(x, z));
            }
        }

        // Place puzzles
        for (int i = 0; i < puzzlesPerLevel; i++)
        {
            if (availableRooms.Count == 0) break;

            // Get random room
            int randomIndex = UnityEngine.Random.Range(0, availableRooms.Count);
            Vector2Int puzzleLocation = availableRooms[randomIndex];

            // Add puzzle to room
            puzzleLocations.Add(puzzleLocation);
            
            // Remove nearby rooms from available rooms
            availableRooms.RemoveAll(room => 
                Vector2Int.Distance(room, puzzleLocation) < minDistanceBetweenPuzzles);
        }

        // Create puzzle triggers in selected rooms
        foreach (Vector2Int location in puzzleLocations)
        {
            CreatePuzzleTrigger(location);
        }
    }

    private void CreatePuzzleTrigger(Vector2Int location)
    {
        GameObject room = rooms[location.x, location.y].gameObject;
        
        // Create trigger object
        GameObject trigger = new GameObject("PuzzleTrigger");
        trigger.transform.parent = room.transform;
        trigger.transform.localPosition = Vector3.zero;

        // Add collider
        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(3f, 3f, 3f);

        // Add trigger script
        PuzzleTrigger puzzleTrigger = trigger.AddComponent<PuzzleTrigger>();
        puzzleTrigger.Initialize(rooms[location.x, location.y].emotion);
    }

    private void UpdateRoomVisuals(int x, int z)
    {
        Room room = rooms[x, z];
        EmotionData emotionData = emotionConfig.GetEmotionData(room.emotion);

        // Update room material
        Renderer[] renderers = room.gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = new Material(defaultMaterial);
            material.color = emotionData.color * emotionIntensity;
            renderer.material = material;
        }
    }

    public void UpdatePlayerRoom(Vector3 playerPosition)
    {
        int x = Mathf.RoundToInt(playerPosition.x / roomSize);
        int z = Mathf.RoundToInt(playerPosition.z / roomSize);

        if (IsValidPosition(x, z))
        {
            Vector2Int newRoom = new Vector2Int(x, z);
            if (newRoom != currentPlayerRoom)
            {
                currentPlayerRoom = newRoom;
                OnPlayerRoomChanged?.Invoke(currentPlayerRoom);
                OnRoomEmotionChanged?.Invoke(rooms[x, z].emotion);
            }
        }
    }

    private bool IsValidPosition(int x, int z)
    {
        return x >= 0 && x < mazeSize && z >= 0 && z < mazeSize;
    }

    private void SpawnPlayer()
    {
        // Find player spawn point (entrance room)
        Vector3 spawnPosition = rooms[0, 0].gameObject.transform.position + Vector3.up;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            player.transform.position = spawnPosition;
            currentPlayerRoom = new Vector2Int(0, 0);
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }
    }

    private void ClearCurrentLevel()
    {
        if (rooms != null)
        {
            for (int x = 0; x < mazeSize; x++)
            {
                for (int z = 0; z < mazeSize; z++)
                {
                    if (rooms[x, z]?.gameObject != null)
                    {
                        Destroy(rooms[x, z].gameObject);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}

[System.Serializable]
public class Room
{
    public Vector2Int position;
    public GameObject gameObject;
    public EmotionType emotion;
    public List<Room> connectedRooms = new List<Room>();
}

public class PuzzleTrigger : MonoBehaviour
{
    private EmotionType emotion;

    public void Initialize(EmotionType roomEmotion)
    {
        emotion = roomEmotion;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PuzzleManager.Instance.StartPuzzle(emotion);
        }
    }
}
