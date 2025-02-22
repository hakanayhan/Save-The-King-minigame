﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 6;
    public GameObject tilePrefab;

    public enum Color
    {
        Red,
        Blue,
        Green
    }

    public List<Color> colors;

    public Vector2 tileSize = new Vector2(1, 1);
    public float spacing;
    private GameObject[,] grid;

    public Tile SelectedTile;

    private bool isSwappingAvailable = true;

    void Start()
    {
        grid = new GameObject[width, height];
        GenerateTile();
    }

    void GenerateTile()
    {
        float startX = -width / 2 * (tileSize.x + spacing) + gameObject.GetComponent<Transform>().position.x * 2;
        float startY = -height / 2 * (tileSize.y + spacing) + gameObject.GetComponent<Transform>().position.y * 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(startX + x * (tileSize.x + spacing), startY + y * (tileSize.y + spacing));
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                tile.transform.localScale = new Vector3(tileSize.x, tileSize.y, 1);
                grid[x, y] = tile;
                Color validColor = GetValidColor(x, y);

                tile.GetComponent<Tile>().Initialize(x, y, this, validColor);
            }
        }
    }

    public void SwapTiles(Tile tile1, Tile tile2, bool revert = false)
    {
        if (isSwappingAvailable && GameManager.Instance.isGameOn)
        {
            grid[tile1.x, tile1.y] = tile2.gameObject;
            grid[tile2.x, tile2.y] = tile1.gameObject;

            int tempX = tile1.x;
            int tempY = tile1.y;

            tile1.UpdatePosition(tile2.x, tile2.y);
            tile2.UpdatePosition(tempX, tempY);

            Vector3 tempPosition = tile1.transform.position;
            tile1.transform.position = tile2.transform.position;
            tile2.transform.position = tempPosition;

            if (!CheckForMatches() && !revert)
            {
                StartCoroutine(RevertTilesAfterDelay(tile1, tile2));
            }
        }
    }

    private IEnumerator RevertTilesAfterDelay(Tile tile1, Tile tile2)
    {
        isSwappingAvailable = false;
        yield return new WaitForSeconds(0.5f);
        isSwappingAvailable = true;
        SwapTiles(tile2, tile1, true);
    }
    public bool CheckForMatches()
    {
        List<Tile> matchedTiles = new List<Tile>();
        // Check horizontal matches
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 2; x++)
            {
                if (x + 2 < width)
                {
                    Tile tile1 = grid[x, y]?.GetComponent<Tile>();
                    Tile tile2 = grid[x + 1, y]?.GetComponent<Tile>();
                    Tile tile3 = grid[x + 2, y]?.GetComponent<Tile>();

                    if (tile1 != null && tile2 != null && tile3 != null &&
                        tile1.color == tile2.color && tile2.color == tile3.color)
                    {
                        matchedTiles.Add(tile1);
                        matchedTiles.Add(tile2);
                        matchedTiles.Add(tile3);
                    }
                }
            }
        }

        // Check vertical matches
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 2; y++)
            {
                if (y + 2 < height)
                {
                    Tile tile1 = grid[x, y]?.GetComponent<Tile>();
                    Tile tile2 = grid[x, y + 1]?.GetComponent<Tile>();
                    Tile tile3 = grid[x, y + 2]?.GetComponent<Tile>();

                    if (tile1 != null && tile2 != null && tile3 != null &&
                        tile1.color == tile2.color && tile2.color == tile3.color)
                    {
                        matchedTiles.Add(tile1);
                        matchedTiles.Add(tile2);
                        matchedTiles.Add(tile3);
                    }
                }
            }
        }

        // Remove matched tiles
        if (matchedTiles.Count > 0)
        {
            StartCoroutine(RemoveTilesAfterDelay(matchedTiles));
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator RemoveTilesAfterDelay(List<Tile> matchedTiles)
    {
        isSwappingAvailable = false;
        yield return new WaitForSeconds(0.5f);
        isSwappingAvailable = true;
        RemoveTiles(matchedTiles);
    }
    private void RemoveTiles(List<Tile> matchedTiles)
    {
        foreach (var tile in matchedTiles)
        {
            // Destroy the matched tile
            Destroy(tile.gameObject);
            grid[tile.x, tile.y] = null;
        }
    }

    private Color GetValidColor(int x, int y)
    {
        List<Color> validColors = new List<Color>(colors);

        if (x > 1 && grid[x - 1, y] != null && grid[x - 2, y] != null)
        {
            Tile left1 = grid[x - 1, y].GetComponent<Tile>();
            Tile left2 = grid[x - 2, y].GetComponent<Tile>();

            if (left1.color == left2.color)
            {
                validColors.Remove(left1.color);
            }
        }

        if (y > 1 && grid[x, y - 1] != null && grid[x, y - 2] != null)
        {
            Tile up1 = grid[x, y - 1].GetComponent<Tile>();
            Tile up2 = grid[x, y - 2].GetComponent<Tile>();

            if (up1.color == up2.color)
            {
                validColors.Remove(up1.color);
            }
        }

        return validColors[Random.Range(0, validColors.Count)];
    }
}