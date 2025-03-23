using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    public bool IsAuto {get; private set;}

    private Board m_board;

    private GameManager m_gameManager;


    private Camera m_cam;

   

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }
    private void Fill()
    {
        m_board.Fill();
        FindMatchesAndCollapse();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
              
                break;
           
        }
    }
   
public void AutoPlayLose()
{
   
    if (m_gameManager.CurrentModeLv == GameManager.eLevelMode.TIMER) return;
   
     m_gameManager.SetAuto(true);
    List<Cell> potentialMoves = m_board.DistributeCellsByItemType();
    RunAutoPlayStep(potentialMoves, 0);
}
private void OnDestroy() {
      m_gameManager.StateChangedAction -= OnGameStateChange;
      StopAllCoroutines();
}
public void AutoPlayWin()
{
    m_gameManager.SetAuto(true);
    List<Cell> potentialMoves = m_board.SortCellsByItemType();
    RunAutoPlayStep(potentialMoves, 0);
}
private void RunAutoPlayStep(List<Cell> potentialMoves, int index)
{
    if (index >= potentialMoves.Count )
    {
       m_gameManager.SetAuto(false);
        return;
    }
    Cell cell = potentialMoves[index];
   MoveItemDown(cell, () =>
    {
       
        DOVirtual.DelayedCall(0.5f, () => RunAutoPlayStep(potentialMoves, index + 1));
    });
}

private void MoveItemDown(Cell cell, Action onComplete)
{
    m_board.SetSortingLayer(cell);
    Cell targetCell = m_board.GetBottomCell(cell.Item);

    if (targetCell != null)
    {
       
        Sequence moveSequence = DOTween.Sequence();

        moveSequence.Append(
            cell.Item.View.DOMove(targetCell.transform.position, 0.5f)
        );

        moveSequence.OnComplete(() =>
        {
            targetCell.Assign(cell.Item);
            cell.Free();
            FindMatchesAndCollapse(targetCell, onComplete); 
          
        });
    }
    else
    {
        onComplete?.Invoke();
    }
}

    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;
        if(m_gameManager.IsAuto) return;

        if (!m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
               
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                    Cell clickedCell = hit.collider.GetComponent<Cell>();
                    
                   if (clickedCell == null) return;
                    if(clickedCell.Item == null) return;
             if(m_board.CheckCellInBoard(clickedCell) && !m_board.IsFullItemBottomRow()){
               IsBusy = true;
              
            MoveItemDown(clickedCell,null);
             }
             if(m_board.CheckCellInBottomRow(clickedCell) && m_gameManager.CurrentModeLv == GameManager.eLevelMode.TIMER){
                
                  IsBusy = true;
                 MoveItemUp(clickedCell);
             }
                }
            }
              
       
    }
  
private void MoveItemUp(Cell cell)
{
     m_board.SetSortingLayer(cell);
     Cell targetCell = cell.Item.OriginalCell;
      if (targetCell != null)
    {
       
        cell.Item.View.DOMove(targetCell.transform.position, 0.5f).OnComplete(() =>
        {
            Debug.Log(cell.Item);
            targetCell.Assign(cell.Item);
            cell.Free(); 
            IsBusy = false;
            
        });

    }
}
  private void FindMatchesAndCollapse(Cell cell, Action onComplete)
{
    List<Cell> matches = GetMatches(cell).Distinct().ToList();
    
    if (matches.Count < m_gameSettings.MatchesMin)
    {
        IsBusy = false;
        if(m_gameManager.CurrentModeLv == GameManager.eLevelMode.MOVES && m_board.IsFullItemBottomRow()){
                m_gameOver = true;
                m_gameManager.SetAuto(false);
                m_gameManager.GameOver();
                return;
            }
        onComplete?.Invoke(); 
    }
    else
    {
        OnMoveEvent();
        CollapseMatches(matches, () =>
        {
            ShiftItemsLeft(3, onComplete); 
        });
       
    }
}

   private void ShiftItemsLeft(int startX, Action onComplete)
{
    Sequence shiftSequence = DOTween.Sequence();

    for (int x = startX; x < m_board.BottomRowSize; x++)
    {
       
            Cell currentCell = m_board.BottomRow_cells[x];
            Cell leftCell = m_board.BottomRow_cells[x - 3];

            if (!currentCell.IsEmpty && leftCell.IsEmpty)
            {
                Item item = currentCell.Item;
                currentCell.Free();
                leftCell.Assign(item);

                shiftSequence.Join(
                    item.View.DOMove(leftCell.transform.position, 0.3f)
                );
            }
        
    }

    shiftSequence.OnComplete(() =>
    {
        IsBusy = false;
        onComplete?.Invoke(); 
    });
}
    private void FindMatchesAndCollapse()
    {
       
        List<Cell> matches = m_board.FindFirstMatch();

        if (matches.Count > 0)
        {
            CollapseMatches(matches,null);
        }
        else
        {
            m_potentialMatch = m_board.GetPotentialMatches();
            if (m_potentialMatch.Count > 0)
            {
                IsBusy = false;

                m_timeAfterFill = 0f;
            }
          
        }
    }
    private List<Cell> GetMatches(Cell cell)
    {
        List<Cell> listHor = m_board.GetHorizontalMatches(cell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        return listHor.Distinct().ToList();
    }

   private void CollapseMatches(List<Cell> matches, Action onComplete)
{
    Sequence explodeSequence = DOTween.Sequence();

    foreach (Cell cell in matches)
    {
        explodeSequence.Join(
            cell.ExplodeItemTween()
        );
    }

    explodeSequence.OnComplete(() =>
    {
        onComplete?.Invoke(); 
    });
}
    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    internal void Clear()
    {
        m_board.Clear();
    }

   
}
