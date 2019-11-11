using UnityEngine;

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

    public void LoadSprite (Sprite sprite)
    {
        m_Renderer.sprite = sprite;
    }

    public void SetBall (Ball ball)
    {
        m_Ball = ball;
    }

    public Ball GetBall ()
    {
        return m_Ball;
    }

    public void InitTile (int row, int column)
    {
        if (m_Renderer == null)
            m_Renderer = GetComponent<SpriteRenderer>();
        PRow = row;
        PColumn = column;
        m_Ball = null;
    }

    public void UpdateTile (float delta)
    {
        if (m_Ball != null)
            m_Ball.UpdateBall(delta);
    }
    #endregion

    #region unity methods
    private void OnMouseDown() {
        
    }
    #endregion
}
