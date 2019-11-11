using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    #region prefabs
    public GameObject Pre_Tile;
    #endregion   

    #region variable 
    Tile[,] m_Tiles;
    List<Sprite> m_TileSprites;
    bool m_IsBoardInit = false;
    int m_Quota;
    int m_NumberOfBalls;
    #endregion

    #region public methods
    public bool IsBoardInit ()
    {
        return m_IsBoardInit;
    }
    public void InitBoard ()
    {
        m_Quota = 3;

        m_NumberOfBalls = 0;

        LoadTileSprite ();
        
        StartCoroutine(GenerateTiles());
    }

    public void GenerateRandomBalls ()
    {
        int count = 0;

        while (count < m_Quota)
        {
            int i = Random.Range(0, Constant.BOARD_ROW);
            int j = Random.Range(0, Constant.BOARD_COLUMN);

            // if ()
        }
    }
    #endregion

    #region private methods
    IEnumerator GenerateTiles ()
    {
        if (m_Tiles == null)
            m_Tiles = new Tile[Constant.BOARD_ROW, Constant.BOARD_COLUMN];

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
                m_Tiles[i, j] = tile;
            }

        }
        yield return null;

        m_IsBoardInit = true;
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
