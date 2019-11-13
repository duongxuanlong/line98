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
    public delegate void BoardCastEventToSubcribers(GameEvent evt);
    public static event BoardCastEventToSubcribers EventBoard;
    #endregion

    #region variable 
    Tile[,] m_Tiles;
    List<Sprite> m_TileSprites;
    List<Tile> m_Utility;
    List<Tile> m_ScoreTiles;
    List<Ball> m_ShowingBalls;
    List<Ball> m_RandomBalls;
    BallFactory m_BallFactory;
    Tile m_SelectedTile;
    Tile m_TargetTile;
    bool m_IsBoardInit = false;
    bool m_Temp = false;
    bool m_GameOver;
    int m_Quota;
    int m_ScorePoint;
    public int m_TotalBalls;
    int m_LeastBallsToScore;
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
            // total = Constant.BOARD_ROW * Constant.BOARD_COLUMN - m_TotalBalls;
            // m_TotalBalls = Constant.BOARD_COLUMN * Constant.BOARD_ROW;
            // BoradcastBoardEvent(GameCommand.BOARD_STOP_RECEIVE_INPUT);
            m_GameOver = true;
            BoradcastBoardEvent(GameCommand.BOARD_GAME_OVER);
            return;
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
                m_RandomBalls.Add(ball);
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

    public void NotifyFromTile (GameEvent evt, Tile caller)
    {
        switch (evt.PCommand)   
        {
            case GameCommand.TILE_SELECT:
            {
                HandleSelectTile(caller);
                break;
            }

            case GameCommand.TILE_FINISH_MOVE_TO_TARGET:
            {
                // score game point
                HandleScoreTiles(caller);

                // convert ball from scale mode to normal mode
                // ChangeBallToNormalMode();

                // GenerateRandomBalls();

                // ResetBoard();

                // BoradcastBoardEvent(GameCommand.BOARD_CAN_RECEIVE_INPUT);
                break;
            }
        }
    }

    public void UpdateBoard (float delta)
    {
        // if (m_SelectedTile != null)
        //     m_SelectedTile.UpdateTile(delta);

        // if (m_TargetTile != null)
        // {
        //     m_TargetTile.UpdateTile(delta);
        // }
        for (int i = 0; i < Constant.BOARD_ROW; ++i)
            for (int j = 0; j < Constant.BOARD_COLUMN; ++j)
                m_Tiles[i, j].UpdateTile(delta);

        if (m_ScoreTiles.Count > 0)
        {
            bool finish = true;
            foreach (var item in m_ScoreTiles)
            {
                Ball ball = item.GetBall();
                if (!ball.IsBlinkAnimFinish())
                {
                    finish = false;
                    break;
                }
            }

            if (finish)
            {
                ReleaseBallsFromTiles();
                PrepareBoardToPlay();
            }
        }
    }
    #endregion

    #region private methods
    void ReleaseBallsFromTiles ()
    {
        foreach ( var item in m_ScoreTiles)
        {
            Ball ball = item.GetBall();
            ball.SetBallActive(false);
            m_BallFactory.AddBallToFactory(ball);
            item.ReseTile();
            item.SetBall(null);
        }
        m_ScoreTiles.Clear();
    }
    void ResetBoard ()
    {
        m_SelectedTile = null;
        m_TargetTile = null;

        for (int i = 0; i < Constant.BOARD_ROW; ++i)
            for (int j = 0; j < Constant.BOARD_COLUMN; ++j)
            m_Tiles[i, j].ReseTile();
    }
    bool CheckMoveTile (Tile source, Tile targetTile, int weight)
    {
        bool isValid = false;

        if ((targetTile.GetBall() == null 
            || targetTile.GetBall().GetBallMode() == BallFactory.BallMode.Scale
            || source.GetBall().GetBallType() == BallFactory.BallType.Ghost)
            && targetTile != source)
        {
            if (targetTile.GetWeight() == 0 || (weight + 1 < targetTile.GetWeight()))
            {
                isValid = true;
            }
        }

        return isValid;
    }

    Tile CheckScoreTile (Tile source, int targetRow, int targetColumn)
    {
        Tile target = m_Tiles[targetRow, targetColumn];

        BallFactory.BallType sourceType = source.GetBall().GetBallType();
        Ball targetBall = target.GetBall();
        if (targetBall == null)
        {
            target = null;
        }
        else if (target.GetBall().GetBallMode() == BallFactory.BallMode.Scale)
        {
            target = null;
        }
        else
        {
            BallFactory.BallType targetType = target.GetBall().GetBallType();
            if (!(sourceType == BallFactory.BallType.Ghost 
                || targetType == BallFactory.BallType.Ghost
                || sourceType == targetType))
                target = null;
        }
        

        return target;
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
                if (CheckMoveTile(m_SelectedTile, up, weight))
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
                if (CheckMoveTile(m_SelectedTile, low, weight))
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
                if (CheckMoveTile(m_SelectedTile, left, weight))
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
                if (CheckMoveTile(m_SelectedTile, right, weight))
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
        if (destination.GetWeight() == 0)
        {
            ResetBoard();
            return;
        }

        Ball ball = m_SelectedTile.GetBall();
        m_SelectedTile.SetBall(null);
        destination.SetBall(ball);
        destination.SetDestinationTile();

        m_TargetTile = destination;

        BoradcastBoardEvent(GameCommand.BOARD_STOP_RECEIVE_INPUT);
    }

    void BoradcastBoardEvent (GameCommand command)
    {
        if (command > GameCommand.BOARD_START && command < GameCommand.BOARD_END)
        {
            GameEvent evt = new GameEvent();
            evt.PCommand = command;
            if (command == GameCommand.BOARD_SCORE_POINT)
            {
                SimpleJSON.JSONObject obj = new SimpleJSON.JSONObject();
                obj[Constant.SAVE_SCORE] = m_ScorePoint;
                evt.PParams = obj;
            }
            EventBoard(evt);
        }
        
        // switch (command)
        // {
        //     case GameCommand.BOARD_CAN_RECEIVE_INPUT:
        //     {
        //         evt.PCommand = command;
        //         break;
        //     }

        //     case GameCommand.BOARD_STOP_RECEIVE_INPUT:
        //     {
        //         evt.PCommand = command;
        //         break;
        //     }
        // }

        // if (command != GameCommand.NONE)
        // {
        //     EventBoard(evt);
        // }
    }

    void HandleSelectTile (Tile caller)
    {
        if (caller == null)
            return;

        Ball ball = caller.GetBall(); 
        if (m_SelectedTile == null)
        {
            if (ball != null && ball.GetBallMode() != BallFactory.BallMode.Scale)
            {
                m_SelectedTile = caller;
                m_SelectedTile.PlayBallSelectedAnimation(true);
            }
            // Debug.Log("Select tile");
        }
        else
        {
            // find shortest path
            if (ball == null)
            {
                m_SelectedTile.PlayBallSelectedAnimation(false);
                FindShortestPath(caller);
            }
            else // do nothing
            {
                m_SelectedTile.PlayBallSelectedAnimation(false);
                m_SelectedTile = null;
            }
            // Debug.Log("Find something");
        }
    }

    void HandleScoreTiles (Tile caller)
    {
        //horizontal with left
        m_ScoreTiles.Clear();
        // m_ScoreTiles.Add(caller.GetBall());
        m_ScoreTiles.Add(caller);
        Tile source = caller;
        int row = caller.PRow;
        int colum = caller.PColumn;
        while (--colum >= 0)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }
        //horizontal with right
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (++colum < Constant.BOARD_COLUMN)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }

        if (m_ScoreTiles.Count >= m_LeastBallsToScore)
        {
            ResolveScoreTiles();
            return;
        }

        //vertical with up
        m_ScoreTiles.Clear();
        // m_ScoreTiles.Add(caller.GetBall());
        m_ScoreTiles.Add(caller);
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (--row >= 0)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }
        //vertical  with down
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (++row < Constant.BOARD_ROW)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }

        if (m_ScoreTiles.Count >= m_LeastBallsToScore)
        {
            ResolveScoreTiles();
            return;
        }

        //diagonal left-up to right down
        m_ScoreTiles.Clear();
        // m_ScoreTiles.Add(caller.GetBall());
        m_ScoreTiles.Add(caller);
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (--row >= 0 && --colum >= 0)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (++row < Constant.BOARD_ROW && ++colum < Constant.BOARD_ROW)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }

        if (m_ScoreTiles.Count >= m_LeastBallsToScore)
        {
            ResolveScoreTiles();
            return;
        }

        //diagonal with left-down to right-up
        m_ScoreTiles.Clear();
        // m_ScoreTiles.Add(caller.GetBall());
        m_ScoreTiles.Add(caller);
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (++row < Constant.BOARD_ROW && --colum >= 0)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }
        source = caller;
        row = caller.PRow;
        colum = caller.PColumn;
        while (--row >= 0 && ++colum < Constant.BOARD_COLUMN)
        {
            Tile target = CheckScoreTile(source, row, colum);
            if (target != null)
            {
                // Ball ball = target.GetBall();
                // m_ScoreTiles.Add(ball);
                m_ScoreTiles.Add(target);
                source = target;
            }
            else
            {
                break;
            }
        }

        ResolveScoreTiles();
    }

    void ResolveScoreTiles ()
    {
        bool notifyToUI = false;
        // what can I do here
        if (m_ScoreTiles.Count >= m_LeastBallsToScore)
        {
            // foreach ( var item in m_ScoreTiles)
            // {
            //     Ball ball = item.GetBall();
            //     ball.SetBallActive(false);
            //     m_BallFactory.AddBallToFactory(ball);
            //     item.ReseTile();
            //     item.SetBall(null);
            // }
            notifyToUI = true;
            m_ScorePoint = m_ScoreTiles.Count;
            m_TotalBalls -= m_ScoreTiles.Count;
        }

        if (m_ScoreTiles.Count < m_LeastBallsToScore)
            m_ScoreTiles.Clear();

        if (notifyToUI)
        {
            BoradcastBoardEvent(GameCommand.BOARD_SCORE_POINT);
        }
        else
        {
            PrepareBoardToPlay();
        }
    }

    void PrepareBoardToPlay ()
    {
        ChangeBallToNormalMode();

        GenerateRandomBalls();

        ResetBoard();

        if (m_GameOver)
            BoradcastBoardEvent(GameCommand.BOARD_STOP_RECEIVE_INPUT);
        else
            BoradcastBoardEvent(GameCommand.BOARD_CAN_RECEIVE_INPUT);
    }

    void PlayBlinkAnim ()
    {
        foreach (var item in m_ScoreTiles)
            item.PlayBallBlinkAnim();
    }

    void ChangeBallToNormalMode ()
    {
        foreach (var item in m_RandomBalls)
        {
            item.SetBallMode(BallFactory.BallMode.Normal);
        }

        m_RandomBalls.Clear();
    }

    void NotifyFromCanvas (GameEvent evt)
    {
        switch (evt.PCommand)
        {
            case GameCommand.UI_FINISH_SCORE:
            {
                // PrepareBoardToPlay();
                PlayBlinkAnim();
                break;
            }
        }
    }

    void SetUpParams ()
    {
        m_GameOver = false;
        m_Quota = 3;

        m_LeastBallsToScore = 5;

        m_TotalBalls = 0;

        m_ScorePoint = 0;

        m_SelectedTile = null;

        if (m_ShowingBalls == null)
        {
            m_ShowingBalls = new List<Ball>();
        }

        if (m_Utility == null)
            m_Utility = new List<Tile>();

        if (m_ScoreTiles == null)
            m_ScoreTiles = new List<Tile>();

        if (m_RandomBalls == null)
            m_RandomBalls = new List<Ball>();
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
    private void OnEnable() {
        CanvasManager.EventCanvas += NotifyFromCanvas;
    }

    private void OnDisable() {
        CanvasManager.EventCanvas -= NotifyFromCanvas;
    }
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
