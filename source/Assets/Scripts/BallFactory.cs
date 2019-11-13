using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFactory : MonoBehaviour
{
    #region enum
    public enum BallType 
    {
        None = -1,
        Blue,
        Brown,
        Green,
        Light_Blue,
        Pink,
        Red,
        Yellow,
        Ghost
    }

    public enum BallMode
    {
        Normal,
        Scale,
    }
    #endregion
    
    #region variables
    string[] BallPaths = {
        "balls/ball_blue",
        "balls/ball_brown",
        "balls/ball_green",
        "balls/ball_light_blue",
        "balls/ball_pink",
        "balls/ball_red",
        "balls/ball_yellow",
        "balls/ball_ghost"
    };

    public List<Sprite> m_BallSprites;
    public List<Ball> m_Balls;
    bool m_IsBallFactoryInit = false;
    #endregion

    #region prefabs
    public GameObject Pre_Ball;
    #endregion

    #region public methods
    public void InitBallFactory ()
    {
        StartCoroutine(LoadBallSprites());

        StartCoroutine(GenerateBalls());
    }

    public bool IsBallFacotryInit ()
    {
        return m_IsBallFactoryInit;
    }

    public Ball GenerateRandomBall (BallMode mode)
    {
        int start = (int)BallType.Blue;
        int end = (int)BallType.Ghost + 1;

        int result = Random.Range(start, end);
        // result = 0;
        Ball ball = m_Balls[0];
        m_Balls.RemoveAt(0);

        ball.SetBallType((BallType)result);
        ball.SetBallMode(mode);
        ball.SetBallActive(true);
        ball.LoadSprite(m_BallSprites[result]);

        return ball;
    }

    public void AddBallToFactory (Ball ball)
    {
        m_Balls.Add(ball);
    }

    public Sprite GetBallSprite (BallType type)
    {
        int index = (int)type;
        return m_BallSprites[index];
    }
    #endregion

    #region private methods
    IEnumerator LoadBallSprites ()
    {
        int start = (int)BallType.Blue;
        int count = (int)BallType.Ghost;

        if (m_BallSprites == null)
            m_BallSprites = new List<Sprite>();

        for (int i = start; i <= count; ++i)
        {
            Sprite sprite = Resources.Load<Sprite>(BallPaths[i]);
            m_BallSprites.Add(sprite);
            yield return null;
        }
    }

    IEnumerator GenerateBalls ()
    {
        if (m_Balls == null)
            m_Balls = new List<Ball>();
        
        for (int i = 0; i < Constant.NUMBER_BALLS; ++i)
        {
            GameObject obj = Instantiate(Pre_Ball, transform);
            Ball script = obj.GetComponent<Ball>();
            script.InitBall();
            script.SetBallActive(false);
            m_Balls.Add(script);

            if (i % 9 == 0)
                yield return null;
        }

        m_IsBallFactoryInit = true;
    }
    #endregion

    #region unity methods
    // private void Start() {
    //     StartCoroutine(LoadBallSprites());

    //     StartCoroutine(GenerateBalls());

    // }
    #endregion


}
