using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    #region delegate - event
    public delegate void NotifyCallback(GameEvent evt);
    public static event NotifyCallback EventCanvas;
    #endregion

    #region reference
    public Text Ref_ScoreTxt;
    public Text Ref_BestScoreTxt;
    public Button Ref_Replay;
    #endregion
    // Start is called before the first frame update
    
    #region unity methods
    private void Awake() {
        int score = PlayerPrefs.GetInt(Constant.SAVE_SCORE, 0);
        int bestscore = PlayerPrefs.GetInt(Constant.SAVE_BEST_SCORE, 0);

        if (Ref_ScoreTxt != null)
            Ref_ScoreTxt.text = "" + score;
        
        if (Ref_BestScoreTxt != null)
            Ref_BestScoreTxt.text = "" + bestscore;

        if (Ref_Replay != null)
            Ref_Replay.gameObject.SetActive(false);
    }

    private void OnEnable() {
        Board.EventBoard += OnReceiveEventFromBoard;
    }

    private void OnDisable() {
        Board.EventBoard -= OnReceiveEventFromBoard;
    }
    #endregion

    #region private methods
    void HandleScorePoint (SimpleJSON.JSONNode param)
    {
        if (!param[Constant.SAVE_SCORE].IsNull)
        {
            int point = param[Constant.SAVE_SCORE];
            int currentPoint = PlayerPrefs.GetInt(Constant.SAVE_SCORE);
            int bestPoint = PlayerPrefs.GetInt(Constant.SAVE_BEST_SCORE);

            currentPoint += point;
            Ref_ScoreTxt.text = "" + currentPoint;
            PlayerPrefs.SetInt(Constant.SAVE_SCORE, currentPoint);

            if (currentPoint > bestPoint)
            {
                bestPoint = currentPoint;
                Ref_BestScoreTxt.text = "" + bestPoint;
                PlayerPrefs.SetInt(Constant.SAVE_BEST_SCORE, bestPoint);
            }
        }

        BroadcastCanvasEvent(GameCommand.UI_FINISH_SCORE);
    }

    void TriggerGameOver ()
    {
        if (Ref_Replay != null)
            Ref_Replay.gameObject.SetActive(true);
    }

    void BroadcastCanvasEvent (GameCommand command)
    {
        if (command > GameCommand.UI_START && command < GameCommand.UI_END)
        {
            GameEvent evt = new GameEvent();
            evt.PCommand = command;
            EventCanvas(evt);
        }
    }
    void OnReceiveEventFromBoard (GameEvent evt)
    {
        switch (evt.PCommand)
        {
            case GameCommand.BOARD_SCORE_POINT:
            {
                HandleScorePoint(evt.PParams);
                break;
            }

            case GameCommand.BOARD_GAME_OVER:
            {
                TriggerGameOver();
                break;
            }
        }
    }
    #endregion

    #region public methods
    public void RestartGame ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
