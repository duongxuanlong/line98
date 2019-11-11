using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    #region prefabs
    public GameObject Pre_Tile;
    #endregion   

    #region variable 
    List<Tile> m_Tiles;
    List<Sprite> m_TileSprites;
    #endregion

    #region public methods
    public void InitBoard ()
    {
        LoadTileSprite ();
        
        StartCoroutine(GenerateTiles());
    }
    #endregion

    #region private methods
    IEnumerator GenerateTiles ()
    {
        if (m_Tiles == null)
            m_Tiles = new List<Tile>();

        for (int i = 0; i < Constant.BOARD_ROW; i++)
        {
            Vector3 pos = Vector3.zero;
            pos.y = Constant.BOARD_START_Y - i * Constant.TILE_UNIT;

            for (int j = 0; j < Constant.BOARD_COLUMN; ++j)
            {
                pos.x = j * Constant.TILE_UNIT + Constant.BOARD_START_X;
                GameObject obj = Instantiate(Pre_Tile, transform);
                Tile tile = obj.GetComponent<Tile>();
                tile.InitTile(i, j);
                tile.SetPosition(pos);
                tile.LoadSprite(m_TileSprites[(i + j) % 2]);
                m_Tiles.Add(tile);
            }

        }
        yield return null;

    }

    void LoadTileSprite ()
    {
        if (m_TileSprites == null)
            m_TileSprites = new List<Sprite>();

        Sprite tile1 = Resources.Load<Sprite> ("tiles/zTile1");
        m_TileSprites.Add(tile1);

        Sprite tile2 = Resources.Load<Sprite> ("tiles/zTile2");
        m_TileSprites.Add(tile2);
    }
    #endregion

    #region unity methods
    private void Start() {
        InitBoard();
    }
    #endregion
}
