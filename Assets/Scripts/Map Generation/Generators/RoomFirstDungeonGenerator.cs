using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [Header("Spawn Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] decorationPrefabs;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject torchPrefab;
    [SerializeField] private GameObject chestPrefab;

    [Header("Density Settings")]
    [Range(0f, 1f)][SerializeField] private float decorationDensity = 0.05f;
    [Range(0f, 1f)][SerializeField] private float enemyDensity = 0.10f;
    [SerializeField] private int minTorchesPerRoom = 3;
    [SerializeField] private int maxTorchesPerRoom = 8;

    [Header("Dungeon Settings")]
    [SerializeField] private int minRoomCount = 8;
    [SerializeField] private int minRoomWidth = 5, minRoomHeight = 5;
    [SerializeField] private int dungeonWidth = 50, dungeonHeight = 50;
    [SerializeField]
    [Range(0,10)] private int offset = 1;
    [SerializeField]
    [Range (0.1f,1)] private float randomWalkRoomPercent = 0.1f;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        ClearSpawnedObjects();
        tilemapVisualizer.Clear();
        RunProceduralGeneration();
    }

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        List<BoundsInt> roomsList = new List<BoundsInt>();
        while (roomsList.Count < minRoomCount)
            roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, 
                new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        roomsList = SortRooms(roomsList);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        int splitIndex = Mathf.RoundToInt(roomsList.Count * randomWalkRoomPercent);

        BoundsInt startRoom = roomsList[0];
        var randomWalkRoomList = roomsList.GetRange(1, splitIndex); 
        var treasureRooms = roomsList.GetRange(splitIndex, roomsList.Count - splitIndex);
        var combatRooms = roomsList
        .Where(r => r != startRoom && !treasureRooms.Contains(r))
        .ToList();

        HashSet<Vector2Int> randomWalkRoomFloor = CreateRandomWalkRooms(roomsList);
        HashSet<Vector2Int> simpleRoomFloor = CreateSimpleRooms(roomsList);

        floor.UnionWith(randomWalkRoomFloor);
        floor.UnionWith(simpleRoomFloor);

        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

        PlaceRoomContents(
            roomsList,
            startRoom,
            treasureRooms,
            combatRooms,
            floor,       // all floor positions
            corridors    // the HashSet<Vector2Int> of your corridors
        );
    }

    private List<BoundsInt> SortRooms(List<BoundsInt> roomsList)
    {
        List<BoundsInt> sorted = new List<BoundsInt>(roomsList);

        for (int i = 1; i < sorted.Count; i++)
        {
            BoundsInt key = sorted[i];
            float keySize = key.size.sqrMagnitude;

            int j = i - 1;
            while (j >= 0 && sorted[j].size.sqrMagnitude > keySize)
            {
                sorted[j + 1] = sorted[j];
                j--;
            }
       
            sorted[j + 1] = key;
        }
        return sorted;
    }

    private HashSet<Vector2Int> CreateRandomWalkRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) && position.y >=
                    (roomBounds.yMin + offset) && position.y <= (roomBounds.yMax - offset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != destination.y)
        {
            if (destination.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (destination.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != destination.x)
        {
            if (destination.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (destination.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        corridor = IncreaseCorridorSize3by3(corridor);
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var rooms in roomsList)
        {
            for (int coloumn = offset; coloumn < rooms.size.x - offset; coloumn++)
            {
                for (int row = offset; row < rooms.size.y - offset; row++)
                {
                    Vector2Int position = (Vector2Int)rooms.min + new Vector2Int (coloumn, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private void PlaceRoomContents(
        List<BoundsInt> allRooms,
        BoundsInt startRoom,
        List<BoundsInt> treasureRooms,
        List<BoundsInt> combatRooms,
        HashSet<Vector2Int> floor,
        HashSet<Vector2Int> corridors
    )
    {
        // helper: get the floor?inside?this?room (excluding corridors)
        List<Vector2Int> GetRoomTiles(BoundsInt room)
        {
            var list = new List<Vector2Int>();
            for (int x = room.xMin + offset; x < room.xMax - offset; x++)
            {
                for (int y = room.yMin + offset; y < room.yMax - offset; y++)
                {
                    var p = new Vector2Int(x, y);
                    if (floor.Contains(p) && !corridors.Contains(p))
                        list.Add(p);
                }
            }
            return list;
        }

        Vector3 startPos = tilemapVisualizer.CellToWorld((Vector3Int)Vector2Int.RoundToInt(startRoom.center))
                                 + new Vector3(0.5f, 0.5f, 0);
        Instantiate(playerPrefab, startPos, Quaternion.identity);

        // 1) Place chest(s) in each treasure room (at room.center)
        foreach (var room in treasureRooms)
        {
            Vector3 worldPos = tilemapVisualizer.CellToWorld((Vector3Int)Vector2Int.RoundToInt(room.center));
            var go = Instantiate(chestPrefab, worldPos, Quaternion.identity);
            Vector3 mage1 = worldPos + new Vector3(1, 0, 0);
            Vector3 mage2 = worldPos + new Vector3(-1, 0, 0);
            var go2 = Instantiate(magePrefab, mage1, Quaternion.identity);
            var go3 = Instantiate(magePrefab, mage2, Quaternion.identity);
            spawnedObjects.Add(go);
            spawnedObjects.Add(go2);
            spawnedObjects.Add(go3);
        }

        // 2) Place enemies in your combat rooms
        foreach (var room in combatRooms)
        {
            var tiles = GetRoomTiles(room);
            int count = Mathf.RoundToInt(tiles.Count * enemyDensity);
            Shuffle(tiles);
            for (int i = 0; i < count; i++)
            {
                var p = tiles[i];
                Vector3 worldPos = tilemapVisualizer.CellToWorld((Vector3Int)p) + Vector3.one * 0.5f;
                var go = Instantiate(enemyPrefab, worldPos, Quaternion.identity);
                spawnedObjects.Add(go);
            }
        }

        // 3) Place decorative props in every room
        foreach (var room in allRooms)
        {
            var tiles = GetRoomTiles(room);
            int count = Mathf.RoundToInt(tiles.Count * decorationDensity);
            Shuffle(tiles);
            for (int i = 0; i < count; i++)
            {
                var p = tiles[i];
                Vector3 worldPos = tilemapVisualizer.CellToWorld((Vector3Int)p) + Vector3.one * 0.5f;
                var deco = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
                var go = Instantiate(deco, worldPos, Quaternion.identity);
                spawnedObjects.Add(go);
            }
        }

        // 4) Place torches only against walls in each room
        foreach (var room in allRooms)
        {
            // gather (wallCell, floorCell, dir) tuples
            var candidates = new List<(Vector2Int wallCell, Vector2Int floorCell, Vector2Int dir)>();
            foreach (var p in GetRoomTiles(room))
            {
                foreach (var d in Direction2D.cardinalDirectionsList)
                {
                    var wallCell = p + d;
                    if (!floor.Contains(wallCell))
                        candidates.Add((wallCell, p, d));
                }
            }

            if (candidates.Count == 0)
                continue;

            Shuffle(candidates);
            int torchCount = Random.Range(minTorchesPerRoom, maxTorchesPerRoom + 1);

            for (int i = 0; i < torchCount && i < candidates.Count; i++)
            {
                var (wallCell, floorCell, dir) = candidates[i];

                // decide where to spawn
                Vector2Int spawnCell = dir == Vector2Int.up ? wallCell : floorCell;
                Vector3 worldPos = tilemapVisualizer.CellToWorld((Vector3Int)spawnCell);
                var go = Instantiate(torchPrefab, worldPos, Quaternion.identity);
                spawnedObjects.Add(go);
            }
        }
    }

    // Fisher–Yates shuffle helper
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    private void ClearSpawnedObjects()
    {
        foreach (var go in spawnedObjects)
            if (go != null) Destroy(go);
        spawnedObjects.Clear();
    }
}
