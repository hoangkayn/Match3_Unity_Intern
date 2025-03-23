using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTime : LevelCondition
{
    private float m_time;
    private float remainingCells;
    private GameManager m_mngr;
    public override void Setup(float value, Text txt, GameManager mngr)
    {
        base.Setup(value, txt, mngr);

        m_mngr = mngr;

        m_time = value;

        remainingCells = m_mngr.GameSettings.BoardSizeX * m_mngr.GameSettings.BoardSizeY;
         m_mngr.BoardController.OnMoveEvent += OnMove;

        UpdateText();
    }

    private void Update()
    {
        if (m_conditionCompleted) return;

        if (m_mngr.State == GameManager.eStateGame.PAUSE) return;

        m_time -= Time.deltaTime;

        UpdateText();

        if (m_time <= -1f)
        {
            OnConditionComplete();
        }
    }
    private void OnMove(){
if (m_conditionCompleted) return;

        remainingCells -= 3;
        if(remainingCells <= 0)
        {
            this.iswinGame = true;
            OnConditionComplete();
        }
    }
  

    protected override void UpdateText()
    {
        if (m_time < 0f) return;

        m_txt.text = string.Format("TIME:\n{0:00}", m_time);
    }
}
