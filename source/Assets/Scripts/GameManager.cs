using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    #region prefabs
    public GameObject Pre_Board;
    #endregion

    #region variables
    Board m_Board;
    bool m_FirstTime = true;
    #endregion

    #region unity methods
    void Start()
    {
        if (Pre_Board != null)
        {
            if (m_Board == null)
            {
                GameObject obj = Instantiate(Pre_Board, transform);
                m_Board = obj.GetComponent<Board>();
                m_Board.InitBoard();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.deltaTime;
        if (m_Board != null)
        {
            if (m_Board.IsBoardInit() && m_FirstTime)
            {
                m_FirstTime = false;
                m_Board.GenerateRandomBalls(true);
                m_Board.GenerateRandomBalls(false);
            }

            m_Board.UpdateBoard(time);
        }
    }

    private void OnApplicationQuit() {
        PlayerPrefs.SetInt(Constant.SAVE_SCORE, 0);
    }
    #endregion

    #region enum
    
    #endregion
}
