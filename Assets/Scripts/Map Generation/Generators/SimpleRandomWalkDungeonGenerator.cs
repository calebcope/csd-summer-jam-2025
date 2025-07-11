using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{
    [SerializeField]
    protected SimpleRandomWalkData randomWalkParameters;

    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPostions = RunRandomWalk(randomWalkParameters, startPosition);
        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floorPostions);
        WallGenerator.CreateWalls(floorPostions, tilemapVisualizer);
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkData parameters, Vector2Int position)
    {
        var currentPostion = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < randomWalkParameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPostion, randomWalkParameters.walkLength);
            floorPositions.UnionWith(path);
            if (randomWalkParameters.startRandomlyEachIteration)
                currentPostion = floorPositions.ElementAt(UnityEngine.Random.Range(0,floorPositions.Count));
        }
        return floorPositions;
    }

    protected List<Vector2Int> IncreaseCorridorSize3by3(List<Vector2Int> corridor)
    {
        List<Vector2Int> expandedCorridor = new List<Vector2Int>();
        for (int i = 1; i < corridor.Count; i++)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    expandedCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
                }
            }
        }
        return expandedCorridor;
    }

    protected HashSet<Vector2Int> IncreaseCorridorSize3by3(HashSet<Vector2Int> corridor)
    {
        HashSet<Vector2Int> expandedCorridor = new HashSet<Vector2Int>();
        foreach (var position in corridor)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    expandedCorridor.Add(position + (new Vector2Int(x, y)));
                }
            }
        }
        return expandedCorridor;
    }
}
