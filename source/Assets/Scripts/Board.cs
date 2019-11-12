using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    #region prefabs
    public GameObject Pre_Tile;
    public GameObject Pre_BallFactory;
    #endregion   

    #region delegate - event
    public delegate void BoardCastEventToTile(GameEvent evt);
    public static event BoardCastEventToTile EventBoard;
    #endregion

    #region variable 
    Tile[,] m_Tiles;
    List<Sprite> m_TileSprites;
    List<Ball> m_ShowingBalls;
    List<Tile> m_PathTiles;
    List<Tile> m_RandomTiles;
    List<Tile> m_Utility;
    BallFactory m_BallFactory;
    Tile m_SelectedTile;
    Tile m_TargetTile;
    bool m_IsBoardInit = false;
    bool m_Temp = false;
    int m_Quota;
    int m_TotalBalls;
    int m_NumberOfBalls;
    #endregion

    #region public methods
    public bool IsBoardInit ()
    {
        if (m_BallFactory == null)
            return false;

        return m_IsBoardInit && m_BallFactory.IsBallFacotryInit();
    }
    public void InitBoard ()
    {
        SetUpParams();

        LoadTileSprite ();

        GenerateBallFactory();
        
        StartCoroutine(GenerateTiles());
    }

    public void GenerateRandomBalls (bool firstTime = false)
    {
        if (firstTime)
        {
            if (m_ShowingBalls.Count == 0)
            {
                int temp = 0;
                Vector3 pos = Vector3.zero;
                pos.x = 0 - Constant.TILE_UNIT * 2;
                pos.y = Constant.BOARD_START_Y + Constant.TILE_UNIT + 1;
                while (temp < 3)
                {
                    Ball showingBall = m_BallFactory.GenerateRandomBall(BallFactory.BallMode.Normal);
                    pos.x += Constant.TILE_UNIT;
                    showingBall.SetBallPosition(pos);
                    showingBall.SetBallActive(false);
                    m_ShowingBalls.Add(showingBall);
                    temp++;
                }
            }
        }

        int count = 0;
        int total = 0;
        if (m_TotalBalls + m_Quota <= Constant.BOARD_COLUMN * Constant.BOARD_ROW)
        {
            total = m_Quota;
            m_TotalBalls += m_Quota;
        }
        else
        {
            total = Constant.BOARD_ROW * Constant.BOARD_COLUMN - m_TotalBalls;
            m_TotalBalls = Constant.BOARD_COLUMN * Constant.BOARD_ROW;
        }

        while (count < total)
        {
            int i = Random.Range(0, Constant.BOARD_ROW);
            int j = Random.Range(0, Constant.BOARD_COLUMN);

            BallFactory.BallMode mode = BallFactory.BallMode.Scale;
            if (firstTime)
                mode = BallFactory.BallMode.Normal;

            Ball ball = m_Tiles[i, j].GetBall();
            if (ball == null)
            {
                ball = m_BallFactory.GenerateRandomBall(mode);
                ball.SetBallPosition(m_Tiles[i, j].GetPosition());
                m_Tiles[i, j].SetBall (ball);
                m_RandomTiles.Add(m_Tiles[i, j]);
                // count++;

                if (!firstTime)
                {
                    m_ShowingBalls[count].SetBallType(ball.GetBallType());
                    m_ShowingBalls[count].LoadSprite(m_BallFactory.GetBallSprite(ball.GetBallType()));
                    if (!m_ShowingBalls[count].IsObjectActive())
                        m_ShowingBalls[count].SetBallActive(true);
                }

                count++;
            }
        }
    }

    public void NotifyFromTile (Tile caller)
    {
        Ball ball = caller.GetBall(); 
        if (m_SelectedTile == null)
        {
            if (ball != null)
                m_SelectedTile = caller;
            // Debug.Log("Select tile");
        }
        else
        {
            // find shortest path
            if (ball == null)
            {
                FindShortestPath(caller);
            }
            else // do nothing
            {
                m_SelectedTile = null;
            }
            // Debug.Log("Find something");
        }
    }

    public void UpdateBoard (float delta)
    {
        if (m_SelectedTile != null)
            m_SelectedTile.UpdateTile(delta);

        if (m_TargetTile != null)
        {
            m_TargetTile.UpdateTile(delta);
        }
    }
    #endregion

    #region private methods
    bool CheckTile (Tile targetTile, int weight)
    {
        bool isValid = false;

        if ((targetTile.GetBall() == null || targetTile.GetBall().GetBallMode() == BallFactory.BallMode.Scale)
            && targetTile != m_SelectedTile)
        {
            if (targetTile.GetWeight() == 0 || (weight + 1 < targetTile.GetWeight()))
            {
                isValid = true;
            }
        }

        return isValid;
    }
    void FindShortestPath (Tile destination)
    {
        m_Utility.Clear();

        m_Utility.Add(m_SelectedTile);

        while (m_Utility.Count > 0)
        {
            Tile temp = m_Utility[0];
            m_Utility.RemoveAt(0);

            int row = temp.PRow;
            int column = temp.PColumn;
            int weight = temp.GetWeight();

            //upper 
            if (row - 1 >= 0)
            {
                Tile up = m_Tiles[row - 1, column];
                if (CheckTile(up, weight))
                {
                    up.AddWeight(temp);
                    m_Utility.Add(up);
                }
                // if ((up.GetBall() == null || up.GetBall().GetBallMode() == BallFactory.BallMode.Scale)
                //     && up != m_SelectedTile)
                // {
                //     if (up.GetWeight() == 0 || (weight + 1 < up.GetWeight()))
                //     {
                //         up.AddWeight(temp);
                //         m_Utility.Add(up);
                //     }
                // }
            }

            //lower
            if (row + 1 < Constant.BOARD_ROW)
            {
                Tile low = m_Tiles[row + 1, column];
                if (CheckTile(low, weight))
                {
                    low.AddWeight(temp);
                    m_Utility.Add(low);
                }
                // if (low.GetBall() == null && low != m_SelectedTile)
                // {
                //     if (low.GetWeight() == 0 || (weight + 1 < low.GetWeight()))
                //     {
                //         low.AddWeight(temp);
                //         m_Utility.Add(low);
                //     }
                // }
            }

            //left
            if (column - 1 >= 0)
            {
                Tile left = m_Tiles[row, column - 1];
                if (CheckTile(left, weight))
                {
                    left.AddWeight(temp);
                    m_Utility.Add(left);
                }
                // if (left.GetBall () == null && left != m_SelectedTile)
                // {
                //     if (left.GetWeight() == 0 || (weight + 1 < left.GetWeight()))
                //     {
                //         left.AddWeight(temp);
                //         m_Utility.Add(left);
                //     }
                // }
            }

            //right 
            if (column + 1 < Constant.BOARD_COLUMN)
            {
                Tile right = m_Tiles[row, column + 1];
                if (CheckTile(right, weight))
                {
                    right.AddWeight(temp);
                    m_Utility.Add(right);
                }
                // if (right.GetBall() == null && right != m_SelectedTile)
                // {
                //     if (right.GetWeight() == 0 || (weight + 1 < right.GetWeight()))
                //     {
                //         right.AddWeight(temp);
                //         m_Utility.Add(right);
                //     }
                // }
            }
        }

        // Debug.Log("Shortest path");
        // for (int i = 0; i < Constant.BOARD_ROW; ++i)
        //     for (int j = 0; j < Constant.BOARD_COLUMN; ++j)
        //         Debug.Log("i: " + i + " and j: " + j + " with weight: " + m_Tiles[i, j].GetWeight());
        // var paths = destination.GetShortestPath();
        // foreach (var item in paths)
        // {
        //     Debug.Log("Tile x: " + item.PRow + " and y: " + item.PColumn);
        // }

        // Debug.Log("Tile x: " + destination.PRow + " and y: " + destination.PColumn);
        ResolveShortestPaths(destination);
    }

    void ResolveShortestPaths (Tile destination)
    {
        Ball ball = m_SelectedTile.GetBall();
        m_SelectedTile.SetBall(null);
        destination.SetBall(ball);
        destination.SetDestinationTile();

        m_TargetTile = destination;

        BoradcastBoardEvent(GameCommand.TILE_STOP_RECEIVE_INPUT);
    }

    void BoradcastBoardEvent (GameCommand command)
    {
        GameEvent evt = new GameEvent();
        evt.PCommand = GameCommand.NONE;
        switch (command)
        {
            case GameCommand.TILE_CAN_RECEIVE_INPUT:
            {
                evt.PCommand = command;
                break;
            }

            case GameCommand.TILE_STOP_RECEIVE_INPUT:
            {
                evt.PCommand = command;
                break;
            }
        }

        if (command != GameCommand.NONE)
        {
            EventBoard(evt);
        }
    }

    void SetUpParams ()
    {
        m_Quota = 3;

        m_NumberOfBalls = 0;

        m_TotalBalls = 0;

        m_SelectedTile = null;

        if (m_ShowingBalls == null)
        {
            m_ShowingBalls = new List<Ball>();
        }

        if (m_Utility == null)
            m_Utility = new List<Tile>();

        if (m_PathTiles == null)
            m_PathTiles = new List<Tile>();

        if (m_RandomTiles == null)
            m_RandomTiles = new List<Tile>();
    }
    void GenerateBallFactory()
    {
        GameObject obj = Instantiate(Pre_BallFactory, transform);
        m_BallFactory = obj.GetComponent<BallFactory>();
        m_BallFactory.InitBallFactory();
    }
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
                tile.InitTile(i, j, this);
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
    // private void Start() {
    //     InitBoard();
    // }

    // private void Update() {

    //     if (IsBoardInit() && !m_Temp)
    //     {
    //         GenerateRandomBalls();
    //         m_Temp = true;
    //     }
    // }
    #endregion
}
