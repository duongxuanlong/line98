using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    #region variables
    BallFactory.BallType m_Type;
    BallFactory.BallMode m_Mode;
    SpriteRenderer m_Renderer;
    #endregion

    #region public methods
    public void SetBallType (BallFactory.BallType type)
    {
        m_Type = type;
    }

    public BallFactory.BallType GetBallType ()
    {
        return m_Type;
    }

    public void SetBallMode (BallFactory.BallMode mode)
    {
        m_Mode = mode;
    }

    public BallFactory.BallMode GetBallMode ()
    {
        return m_Mode;
    }

    public void SetBallPosition (Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetBallActive (bool active)
    {
        gameObject.SetActive(active);
    }

    public bool IsObjectActive ()
    {
        return gameObject.activeSelf;
    }

    public void LoadSprite (Sprite sprite)
    {
        if (m_Renderer != null)
            m_Renderer.sprite = sprite;
    }

    public void InitBall ()
    {
        if (m_Renderer == null)
            m_Renderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateBall (float deltaTime)
    {

    }
    #endregion

    #region private methods

    #endregion

    #region unity methods
    // private void Start() {
    //     if (m_Renderer == null)
    //         m_Renderer = GetComponent<SpriteRenderer>();
    // }
    #endregion
}
