using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [SerializeField] private TileBase[] floorTiles;

    [SerializeField]
    private TileBase 
        wallTop, wallTopCornerRight, wallTopCornerLeft, wallTopCornerFull,
        wallRight, wallLeft, wallBottom, wallFull, 
        wallInnerCornerDownLeft, wallInnerCornerDownRight, 
        wallOuterCornerDownRight, wallOuterCornerDownLeft, wallOuterCornerUpRight, wallOuterCornerUpLeft;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions) 
        {
            TileBase tile = floorTiles[UnityEngine.Random.Range(0, floorTiles.Length)];
            PaintSingleTile(floorTilemap, position, tile);
        }
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, position, tile);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, Vector2Int position, TileBase tile)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void PaintSingleBasicWall(Vector2Int position, String binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypesHelper.wallTopCornerRight.Contains(typeAsInt))
        {
            tile = wallTopCornerRight;
        }
        else if (WallTypesHelper.wallTopCornerLeft.Contains(typeAsInt))
        {
            tile = wallTopCornerLeft;
        }
        else if (WallTypesHelper.wallTopCornerFull.Contains(typeAsInt))
        {
            tile = wallTopCornerFull;
        }
        else if (WallTypesHelper.wallRight.Contains(typeAsInt))
        {
            tile = wallRight;
        }
        else if (WallTypesHelper.wallBottom.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallLeft.Contains(typeAsInt))
        {
            tile = wallLeft;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, position, tile);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeAsInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallOuterCornerDownLeft.Contains(typeAsInt))
        {
            tile = wallOuterCornerDownLeft;
        }
        else if (WallTypesHelper.wallOuterCornerDownRight.Contains(typeAsInt))
        {
            tile = wallOuterCornerDownRight;
        }
        else if (WallTypesHelper.wallOuterCornerUpLeft.Contains(typeAsInt))
        {
            tile = wallOuterCornerUpLeft;
        }
        else if (WallTypesHelper.wallOuterCornerUpRight.Contains(typeAsInt))
        {
            tile = wallOuterCornerUpRight;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeAsInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottomEightDirections.Contains(typeAsInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, position, tile);
    }

    public Vector3 CellToWorld(Vector3Int cellPosition)
    {
        return floorTilemap.GetCellCenterWorld(cellPosition);
    }
}
