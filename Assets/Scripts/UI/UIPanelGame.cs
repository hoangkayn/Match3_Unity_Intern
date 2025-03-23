using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGame : MonoBehaviour,IMenu
{
    public Text LevelConditionView;

    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnAutoWin;
     [SerializeField] private Button btnAutoLose;

    private UIMainManager m_mngr;
   

    private void Awake()
    {
        btnPause.onClick.AddListener(OnClickPause);
        btnAutoWin.onClick.AddListener(OnAutoWin);
         btnAutoLose.onClick.AddListener(OnAutoLose);
    }

    private void OnClickPause()
    {
        m_mngr.ShowPauseMenu();
    }
    private void OnAutoWin(){
        if(m_mngr.GameManager.IsAuto) return;
        
       m_mngr.GameManager.AutoPlayGame(true);
    }
    private void OnAutoLose(){
         if(m_mngr.GameManager.IsAuto) return;
       
       m_mngr.GameManager.AutoPlayGame(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
