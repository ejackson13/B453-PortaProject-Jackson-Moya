using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMap : MonoBehaviour
{

    [SerializeField] TileBase[] tiles;
    [SerializeField] bool isPhaseable = true;


    private int north = 1;
    private int west = 2;
    private int east = 4;
    private int south = 8;

    private Vector2Int xBounds = new Vector2Int(-13, 12); // inclusive
    private Vector2Int yBounds = new Vector2Int(-8, 7); // inclusive
    private Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();

        //tilemap.SetTile(Vector3Int.zero, tiles[0]);

        for (int x=xBounds.x; x <= xBounds.y; x++)
        {
            for (int y=yBounds.x; y <= yBounds.y; y++)
            {
                if (tilemap.GetTile(new Vector3Int(x, y)) != null)
                {
				    int north_tile = tilemap.GetTile(new Vector3Int(x, y+1)) != null ? 1 : 0;

                    int west_tile = tilemap.GetTile(new Vector3Int(x-1, y)) != null ? 1 : 0;

                    int east_tile = tilemap.GetTile(new Vector3Int(x+1, y)) != null ? 1 : 0;

                    int south_tile = tilemap.GetTile(new Vector3Int(x, y-1)) != null ? 1 : 0;

                    int tile_index = 0 + (north * north_tile) + (west * west_tile) + (east * east_tile) + (south * south_tile);

                    if (tile_index == 15 && isPhaseable)
                    {
					    if (Random.value < 0.1f)
                        {
                            tile_index += Random.Range(0, 12);
                        }
                    }

                    tilemap.SetTile(new Vector3Int(x, y), tiles[tile_index]);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
