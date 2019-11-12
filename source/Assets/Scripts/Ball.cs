using UnityEngine;

public class Ball : MonoBehaviour
{
    #region selected animation
    Vector3 m_OriginalPosition;
    const float SATime = 0.2f;
    const float SATarget = 0.5f;
    float m_SARunningTime;
    float m_SAVelocity;
    float m_SAVelocityScale;
    int m_Phase;
    bool m_IsPlayingSelectedAnimation;
    #endregion

    #region blink animation
    const float BATime = 0.1f;
    const int BACount = 6;
    float m_BARunningTime;
    int m_BACount;
    bool m_IsFaded;
    bool m_IsPlayingBlinkAnimation;
    bool m_IsFinishBlinkAnim;
    #endregion

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

    public Vector3 GetBallPosition ()
    {
        return transform.position;
    }

    public void SetBallMode (BallFactory.BallMode mode)
    {
        m_Mode = mode;
        Vector3 par = Vector3.one;

        if (m_Mode == BallFactory.BallMode.Scale)
        {
            par.x = 0.5f;
            par.y = 0.5f;
            m_Renderer.sortingOrder = 0;
        }
        else if (m_Mode == BallFactory.BallMode.Normal)
        {
            par.x = 1;
            par.y = 1;
            m_Renderer.sortingOrder = 1;
        }

        SetScale(par);
    }

    public BallFactory.BallMode GetBallMode ()
    {
        return m_Mode;
    }

    public void SetBallPosition (Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetOriginalPosition (Vector3 pos)
    {
        m_OriginalPosition = pos;
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

        SetupSAParams ();
        SetupBlinkParams();
    }

    public void SetScale (Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void PlaySelectedAnimation (bool play)
    {
        if (!play)
        {
            transform.position = m_OriginalPosition;
            transform.localScale = Vector3.one;
            m_OriginalPosition = Vector3.zero;   
        }

        m_IsPlayingSelectedAnimation = play;
        m_SARunningTime = 0;
        m_SAVelocityScale = 0;
    }

    public void PlayBlinkAnimation ()
    {
        m_IsPlayingBlinkAnimation = true;
        m_IsFinishBlinkAnim = false;
        m_IsFaded = true;
        m_BARunningTime = 0;
        m_BACount = 0;
    }

    public bool IsBlinkAnimFinish ()
    {
        return m_IsFinishBlinkAnim;
    }

    public void UpdateBall (float deltaTime)
    {
        if (m_IsPlayingSelectedAnimation)
        {
            float targetY = GetTargetY();
            float targetScale = GetTargetScale();

            float y = 0;
            float scale = 1;

            float time = SATime - m_SARunningTime;
            if (time < 0)
            {
                y = targetY;
                scale = targetScale;

                m_Phase++;
                if (m_Phase > 3)
                {
                    m_Phase = 0;
                }
                ResetSAParams();
            }
            else
            {
                y = Mathf.SmoothDamp(transform.position.y, targetY, ref m_SAVelocity, time);
                scale = Mathf.SmoothDamp(transform.localScale.y, targetScale, ref m_SAVelocityScale, time);
            }

            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;

            Vector3 localScale = transform.localScale;
            localScale.y = scale;
            transform.localScale = localScale;

            m_SARunningTime += deltaTime;
            
        }

        if (m_IsPlayingBlinkAnimation)
        {
            if (m_BARunningTime >= BATime)
            {
                Color color = m_Renderer.color;
                if (m_IsFaded)
                {
                    color.a = 0.5f;
                }
                else
                {
                    color.a = 1;
                }
                m_IsFaded = !m_IsFaded;
                m_Renderer.color = color;
                m_BARunningTime = 0;
                m_BACount++;
                if (m_BACount >= BACount)
                {
                    m_IsFinishBlinkAnim = true;
                    m_IsPlayingBlinkAnimation = false;
                }
            }
            m_BARunningTime += deltaTime;
        }
    }
    #endregion

    #region private methods
    float GetTargetScale ()
    {
        float scale = 1;
        switch (m_Phase)
        {
            case 0:
            {
                scale = 0.5f;
                break;
            }
            case 1:
            {
                scale = 0.8f;
                break;
            }
            case 2:
            {
                scale = 0.5f;
                break;
            }
            case 3:
            {
                scale = 1;
                break;
            }
        }
        return scale;
    }
    float GetTargetY()
    {
        float y = 0;
        switch (m_Phase)
        {
            case 0:
            {
                y = m_OriginalPosition.y + SATarget;
                break;
            }
            
            case 1:
            {
                y = m_OriginalPosition.y;
                break;
            }

            case 2:
            {
                y = m_OriginalPosition.y - SATarget;
                break;
            }

            case 3:
            {
                y = m_OriginalPosition.y;
                break;
            }
            
        }
        return y;
    }
    void SetupSAParams()
    {
        m_OriginalPosition = Vector3.zero;
        m_IsPlayingSelectedAnimation = false;
        m_SARunningTime = 0;
        m_SAVelocity = 0;
        m_SAVelocityScale = 0;
        m_Phase = 0;
    }
    
    void SetupBlinkParams ()
    {
        m_BACount = 0;
        m_BARunningTime = 0;
        m_IsFaded = false;
        m_IsPlayingBlinkAnimation = false;
        m_IsFinishBlinkAnim = false;
    }

    void ResetSAParams ()
    {
        m_SARunningTime = 0;
        m_SAVelocity = 0;
        m_SAVelocityScale = 0;
    }
    #endregion

    #region unity methods
    // private void Start() {
    //     if (m_Renderer == null)
    //         m_Renderer = GetComponent<SpriteRenderer>();
    // }
    #endregion
}
