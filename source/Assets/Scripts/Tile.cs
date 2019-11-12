using UnityEngine;
using System.Collections.Generic;
public class Tile : MonoBehaviour
{
    #region properties
    public int m_ColumnIndex;
    public int PColumn
    {
        get {
            return m_ColumnIndex;
        }
        set {
            m_ColumnIndex = value;
        }
    }
    public int m_RowIndex;
    public int PRow
    {
        get 
        {
            return m_RowIndex;
        }
        set
        {
            m_RowIndex = value;
        }
    }
    #endregion

    #region variables
    SpriteRenderer m_Renderer;
    Ball m_Ball;
    Board m_Board;
    bool m_CanTakeInput = true;
    bool m_IsDestination;
    List<Tile> m_Weights;
    Vector3 m_Target;
    Vector3 m_Velocity;
    int m_MoveIndex;
    float m_SmoothTime;
    float m_RunningTime;
    #endregion    

    #region public methods
    public Vector2 GetPosition ()
    {
        return transform.position;
    }

    public void SetPosition (Vector3 pos)
    {
        transform.position = pos;
    }

    public int GetWeight ()
    {
        return m_Weights.Count;
    }

    public void AddWeight (Tile tile)
    {
        if (tile.GetWeight() == 0)
            m_Weights.Add(tile);
        else
        {
            var paths = tile.GetShortestPath();
            foreach (var item in paths)
            {
                m_Weights.Add(item);
            }
        }
        m_Weights.Add(this);
    }

    public List<Tile> GetShortestPath ()
    {
        return m_Weights;
    }

    public void ReseTile ()
    {
        m_Weights.Clear();
        m_IsDestination = false;
    }

    public void LoadSprite (Sprite sprite)
    {
        m_Renderer.sprite = sprite;
    }

    public void SetDestinationTile ()
    {
        m_IsDestination = true;

        // m_Weights.RemoveAt(0);

        m_MoveIndex = 1;
        m_Velocity = Vector3.zero;
        m_Target = m_Weights[m_MoveIndex].GetPosition();
        // m_Weights.RemoveAt(0);

        // SetupPaths();
    }

    public void SetBall (Ball ball)
    {
        m_Ball = ball;
    }

    public Ball GetBall ()
    {
        return m_Ball;
    }

    public void InitTile (int row, int column, Board board)
    {
        if (m_Renderer == null)
            m_Renderer = GetComponent<SpriteRenderer>();
        if (m_Weights == null)
            m_Weights = new List<Tile>();

        PRow = row;
        PColumn = column;
        m_CanTakeInput = true;
        m_IsDestination = false;
        m_Board = board;
        m_Ball = null;
        m_Target = Vector3.zero;
        m_Velocity = Vector3.zero;
        m_SmoothTime = 0.08f;
        m_MoveIndex = 0;
    }

    public void OnReceiveEventFromBoard (GameEvent evt)
    {
        switch (evt.PCommand)
        {
            case GameCommand.BOARD_CAN_RECEIVE_INPUT:
            {
                m_CanTakeInput = true;
                break;
            }
            case GameCommand.BOARD_STOP_RECEIVE_INPUT:
            {
                m_CanTakeInput = false;
                break;
            }
        }
    }

    public void UpdateTile (float delta)
    {
        if (m_Ball != null)
        {
            if (m_IsDestination)
            {
                float time = m_SmoothTime - m_RunningTime;
                bool updateTime = true;
                bool publishEventToBoard = false;
                Vector3 des = Vector3.zero;
                if (time < 0)
                {
                    des = m_Target;
                    m_MoveIndex++;
                    if (m_MoveIndex < m_Weights.Count)
                    {
                        var item = m_Weights[m_MoveIndex];
                        // m_Weights.RemoveAt(0);
                        m_Target = item.GetPosition();
                    }
                    else
                    {
                        m_IsDestination = false;
                        publishEventToBoard = true;
                    }
                    updateTime = false;
                    m_RunningTime = 0;
                }
                else
                {
                    des = Vector3.SmoothDamp(m_Ball.GetBallPosition(), m_Target, ref m_Velocity, time);
                }
                m_Ball.SetBallPosition(des);
                if (updateTime)
                    m_RunningTime += delta;
                if (publishEventToBoard)
                    PublishEventToBoard(GameCommand.TILE_FINISH_MOVE_TO_TARGET);
            }
            else
            {
                m_Ball.UpdateBall(delta);
            }
        }
    }
    #endregion

    #region private methods
    void PublishEventToBoard (GameCommand command)
    {
        if (command > GameCommand.TILE_START && command < GameCommand.TILE_END)
        {
            GameEvent evt = new GameEvent();
            evt.PCommand = command;
            m_Board.NotifyFromTile(evt, this);
        }
    }
    #endregion

    #region unity methods
    private void OnMouseDown() {

        if (m_CanTakeInput)
        {
            // GameEvent evt = new GameEvent();
            // evt.PCommand = GameCommand.TILE_SELECT;
            // m_Board.NotifyFromTile(evt, this);
            // Debug.Log("Tile x: " + PRow + " and y: " + PColumn);

            PublishEventToBoard(GameCommand.TILE_SELECT);
        }
    }

    private void OnEnable() {
        Board.EventBoard += OnReceiveEventFromBoard;
    }

    private void OnDisable() {
        Board.EventBoard -= OnReceiveEventFromBoard;
    }
    #endregion
}
