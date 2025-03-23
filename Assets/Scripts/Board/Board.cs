using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    private int boardSizeX;

    private int boardSizeY;

    private int bottomRowSize;
    public int BottomRowSize => bottomRowSize;

    private Cell[,] m_cells;

    private Cell[] bottomRow_cells;
    public Cell[] BottomRow_cells => bottomRow_cells;

    private Transform m_root;

    private int m_matchMin;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        this.bottomRowSize = gameSettings.BottomRowSize;
        

        m_cells = new Cell[boardSizeX, boardSizeY];

        bottomRow_cells = new Cell[bottomRowSize];

        CreateBoard();
    }
     
    public bool CheckCellInBoard(Cell cell){
         
 for (int x = 0; x < boardSizeX; x++)
        {
           
            for (int y = 0; y < boardSizeY; y++)
            {
              
                if (m_cells[x, y] == cell)
                {
                   
                   return true;
                }
            }
           
        }
        return false;
    }
    public bool IsFullItemBottomRow(){
        foreach(Cell cell in bottomRow_cells){
            if(cell.IsEmpty) return false;
        }
        return true;
    }
    public void SetSortingLayer(Cell cell){
        foreach(Cell child in m_cells){
            if(child.IsEmpty) continue;
            if(cell == child){
              
                child.Item.SetSortingLayerHigher();
            }
            else{
                child.Item.SetSortingLayerLower();
            }
        }
         foreach(Cell child in bottomRow_cells){
            if(child.IsEmpty) continue;
            if(cell == child){
              
                child.Item.SetSortingLayerHigher();
            }
            else{
                child.Item.SetSortingLayerLower();
            }
        }
       
    }

    public bool CheckCellInBottomRow(Cell cell){
         
 for (int x = 0; x < bottomRowSize; x++)
        {
                if (bottomRow_cells[x] == cell)
                {
                   
                   return true;
                }
            
           
        }
        return false;
    }
    
   
  public List<Cell> SortCellsByItemType()
{
    List<Cell> potentialMoves = new List<Cell>();

   
    foreach (Cell cell in m_cells)
    {
        if (cell.IsEmpty) continue;
        potentialMoves.Add(cell);
    }

    
    Dictionary<NormalItem.eNormalType, int> bottomRowItemCount = new Dictionary<NormalItem.eNormalType, int>();

    for (int i = 0; i < bottomRowSize; i++)
    {
        Cell cell = bottomRow_cells[i];
        if (cell.Item is NormalItem normalItem)
        {
            if (!bottomRowItemCount.ContainsKey(normalItem.ItemType))
                bottomRowItemCount[normalItem.ItemType] = 0;
            
            bottomRowItemCount[normalItem.ItemType]++;
        }
    }

   
    return potentialMoves
        .OrderByDescending(cell => bottomRowItemCount.TryGetValue((cell.Item as NormalItem)?.ItemType ?? default, out int count) ? count : 0)
        .ThenBy(cell => (cell.Item as NormalItem)?.ItemType)
        .ToList();
}


public List<Cell> DistributeCellsByItemType()
{
   
    Dictionary<NormalItem.eNormalType, int> bottomRowItemCount = new Dictionary<NormalItem.eNormalType, int>();

    for (int i = 0; i < bottomRowSize; i++)
    {
        
            Cell cell = bottomRow_cells[i];
            if (cell.Item is NormalItem normalItem)
            {
                if (!bottomRowItemCount.ContainsKey(normalItem.ItemType))
                    bottomRowItemCount[normalItem.ItemType] = 0;

                bottomRowItemCount[normalItem.ItemType]++;
            }
        
    }

   
    Dictionary<NormalItem.eNormalType, Queue<Cell>> groupedCells = new Dictionary<NormalItem.eNormalType, Queue<Cell>>();

    foreach (Cell cell in m_cells)
    {
        if (cell.Item is NormalItem normalItem)
        {
            if (!groupedCells.ContainsKey(normalItem.ItemType))
            {
                groupedCells[normalItem.ItemType] = new Queue<Cell>();
            }
            groupedCells[normalItem.ItemType].Enqueue(cell);
        }
    }

   
    List<Cell> sortedCells = new List<Cell>();

    while (groupedCells.Values.Any(q => q.Count > 0)) 
    {
       
        var sortedItemTypes = groupedCells.Keys
            .OrderBy(type => bottomRowItemCount.ContainsKey(type) ? bottomRowItemCount[type] : 0)
            .ToList();

        foreach (var itemType in sortedItemTypes)
        {
            if (groupedCells[itemType].Count > 0)
            {
                sortedCells.Add(groupedCells[itemType].Dequeue());

               
                if (!bottomRowItemCount.ContainsKey(itemType))
                {
                    bottomRowItemCount[itemType] = 1;
                }
                else
                {
                    bottomRowItemCount[itemType]++;
                }
            }
        }
    }

    return sortedCells;
}
  public Cell GetBottomCell(Item item)
{
    Cell firstEmptyCell = null; 
    Cell targetCell = null;     
    int targetX = 0;           

    for (int i = 0; i < bottomRowSize; i++) 
    {
        
            Cell currentCell = bottomRow_cells[i];
            if (currentCell.IsEmpty)
            {  
                if (firstEmptyCell == null)
                {
                
                    firstEmptyCell = currentCell; 
                }
            }
            else if (currentCell.Item != null && currentCell.Item.IsSameType(item))
            {
                if (currentCell.NeighbourRight != null)
                {
                    targetCell = currentCell.NeighbourRight;
                    targetX = i + 1; 
                }
            
        }
    }
    
    if (targetCell != null)
    {
        ShiftItemsRight(targetX);
        return targetCell;
    }
    return firstEmptyCell;
}
private void ShiftItemsRight(int startX)
{
    for (int x = bottomRowSize - 1; x > startX; x--) 
    {
       
            Cell currentCell = bottomRow_cells[x - 1]; 
            Cell rightCell = bottomRow_cells[x];      

            if (!currentCell.IsEmpty && rightCell.IsEmpty)
            {
               Item item = currentCell.Item;
               currentCell.Free();
                rightCell.Assign(item);
                item.View.DOMove(rightCell.transform.position, 0.3f);
            }
        
    }
}
    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }
        //set neighbours
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
               
                if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
            
                if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
            }
        }
        // tạo 5 ô bên dưới
        float offsetY = 1.5f;
        Vector3 originBottom = new Vector3(-bottomRowSize * 0.5f + 0.5f, origin.y - offsetY, 0f);
        GameObject prefabBGBottom = Resources.Load<GameObject>(Constants.PREFAB_CELL_BOTTOM);
        for (int x = 0; x < bottomRowSize; x++)
        {
            
                GameObject go = GameObject.Instantiate(prefabBGBottom);
                go.transform.position = originBottom + new Vector3(x, 0, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, 0);
                bottomRow_cells[x] = cell;
            
        }
          //set neighbours
        for (int x = 0; x < bottomRowSize; x++)
        {
            
               
                if (x + 1 < bottomRowSize) bottomRow_cells[x].NeighbourRight = bottomRow_cells[x + 1];
            
                if (x > 0) bottomRow_cells[x].NeighbourLeft = bottomRow_cells[x - 1];
            
        }
        
    }

   internal void Fill()
{
    NormalItem.eNormalType[] allTypes = (NormalItem.eNormalType[])Enum.GetValues(typeof(NormalItem.eNormalType));
    List<NormalItem.eNormalType> itemPool = new List<NormalItem.eNormalType>();
    int totalCells = boardSizeX * boardSizeY;
    int itemsPerType = 3;

    foreach (var type in allTypes)
    {
        for (int i = 0; i < itemsPerType; i++)
        {
            itemPool.Add(type);
        }
    }
    int balance = totalCells - allTypes.Length * itemsPerType;
    if(balance > 0){
        for(int i =0;i<balance/3;i++){
NormalItem.eNormalType type = allTypes[UnityEngine.Random.Range(0,allTypes.Length)];
for(int j = 0;j<itemsPerType;j++){
    itemPool.Add(type);
}
        }
    }

   
   
    itemPool = itemPool.OrderBy(x => UnityEngine.Random.value).ToList();

    
    int index = 0;
    for (int x = 0; x < boardSizeX; x++)
    {
        for (int y = 0; y < boardSizeY; y++)
        {
            Cell cell = m_cells[x, y];
            NormalItem item = new NormalItem();

            item.SetType(itemPool[index]); 
            item.SetView();
            item.SetViewRoot(m_root);

            cell.Assign(item);
            item.SetOriginalCell(cell);
            cell.ApplyItemPosition(false);

            index++;
        }
    }
}
    internal void Shuffle()
    {
        List<Item> list = new List<Item>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                list.Add(m_cells[x, y].Item);
                m_cells[x, y].Free();
            }
        }
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int rnd = UnityEngine.Random.Range(0, list.Count);
                m_cells[x, y].Assign(list[rnd]);
                m_cells[x, y].ApplyItemMoveToPosition();

                list.RemoveAt(rnd);
            }
        }
    }
    internal void FillGapsWithNewItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (!cell.IsEmpty) continue;

                NormalItem item = new NormalItem();

                item.SetType(Utils.GetRandomNormalType());
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(true);
            }
        }
    }
    internal void ExplodeAllItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.ExplodeItem();
            }
        }
    }
    public void Swap(Cell cell1, Cell cell2, Action callback)
    {
        Item item = cell1.Item;
        cell1.Free();
        Item item2 = cell2.Item;
        cell1.Assign(item2);
        cell2.Free();
        cell2.Assign(item);

        item.View.DOMove(cell2.transform.position, 0.3f);
        item2.View.DOMove(cell1.transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });
    }
    public List<Cell> GetHorizontalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        //check horizontal match
        Cell newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }
    internal void ConvertNormalToBonus(List<Cell> matches, Cell cellToConvert)
    {
        eMatchDirection dir = GetMatchDirection(matches);

        BonusItem item = new BonusItem();
        switch (dir)
        {
            case eMatchDirection.ALL:
                item.SetType(BonusItem.eBonusType.ALL);
                break;
            case eMatchDirection.HORIZONTAL:
                item.SetType(BonusItem.eBonusType.HORIZONTAL);
                break;
            case eMatchDirection.VERTICAL:
                item.SetType(BonusItem.eBonusType.VERTICAL);
                break;
        }

        if (item != null)
        {
            if (cellToConvert == null)
            {
                int rnd = UnityEngine.Random.Range(0, matches.Count);
                cellToConvert = matches[rnd];
            }

            item.SetView();
            item.SetViewRoot(m_root);

            cellToConvert.Free();
            cellToConvert.Assign(item);
            cellToConvert.ApplyItemPosition(true);
        }
    }
    internal eMatchDirection GetMatchDirection(List<Cell> matches)
    {
        if (matches == null || matches.Count < m_matchMin) return eMatchDirection.NONE;

        var listH = matches.Where(x => x.BoardX == matches[0].BoardX).ToList();
        if (listH.Count == matches.Count)
        {
            return eMatchDirection.VERTICAL;
        }

        var listV = matches.Where(x => x.BoardY == matches[0].BoardY).ToList();
        if (listV.Count == matches.Count)
        {
            return eMatchDirection.HORIZONTAL;
        }

        if (matches.Count > 5)
        {
            return eMatchDirection.ALL;
        }

        return eMatchDirection.NONE;
    }

    internal List<Cell> FindFirstMatch()
    {
        List<Cell> list = new List<Cell>();

        for (int x = 0; x < bottomRowSize; x++)
        {
           
                Cell cell = bottomRow_cells[x];

                var listhor = GetHorizontalMatches(cell);
                if (listhor.Count >= m_matchMin)
                {
                    list = listhor;
                    break;
                }
            
        }

        return list;
    }

    public List<Cell> CheckBonusIfCompatible(List<Cell> matches)
    {
        var dir = GetMatchDirection(matches);

        var bonus = matches.Where(x => x.Item is BonusItem).FirstOrDefault();
        if(bonus == null)
        {
            return matches;
        }

        List<Cell> result = new List<Cell>();
        switch (dir)
        {
            case eMatchDirection.HORIZONTAL:
                foreach (var cell in matches)
                {
                    BonusItem item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.HORIZONTAL)
                    {
                        result.Add(cell);
                    }
                }
                break;
            case eMatchDirection.VERTICAL:
                foreach (var cell in matches)
                {
                    BonusItem item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.VERTICAL)
                    {
                        result.Add(cell);
                    }
                }
                break;
            case eMatchDirection.ALL:
                foreach (var cell in matches)
                {
                    BonusItem item = cell.Item as BonusItem;
                    if (item == null || item.ItemType == BonusItem.eBonusType.ALL)
                    {
                        result.Add(cell);
                    }
                }
                break;
        }

        return result;
    }

    internal List<Cell> GetPotentialMatches()
    {
        List<Cell> result = new List<Cell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];

                //check right
                /* example *\
                  * * * * *
                  * * * * *
                  * * * ? *
                  * & & * ?
                  * * * ? *
                \* example  */

                if (cell.NeighbourRight != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourRight, cell.NeighbourRight.NeighbourRight);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                //check up
                /* example *\
                  * ? * * *
                  ? * ? * *
                  * & * * *
                  * & * * *
                  * * * * *
                \* example  */
                if (cell.NeighbourUp != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourUp, cell.NeighbourUp.NeighbourUp);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                //check bottom
                /* example *\
                  * * * * *
                  * & * * *
                  * & * * *
                  ? * ? * *
                  * ? * * *
                \* example  */
                if (cell.NeighbourBottom != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourBottom, cell.NeighbourBottom.NeighbourBottom);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                //check left
                /* example *\
                  * * * * *
                  * * * * *
                  * ? * * *
                  ? * & & *
                  * ? * * *
                \* example  */
                if (cell.NeighbourLeft != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourLeft, cell.NeighbourLeft.NeighbourLeft);
                    if (result.Count > 0)
                    {
                        break;
                    }
                }

                /* example *\
                  * * * * *
                  * * * * *
                  * * ? * *
                  * & * & *
                  * * ? * *
                \* example  */
                Cell neib = cell.NeighbourRight;
                if (neib != null && neib.NeighbourRight != null && neib.NeighbourRight.IsSameType(cell))
                {
                    Cell second = LookForTheSecondCellVertical(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourRight);
                        result.Add(second);
                        break;
                    }
                }

                /* example *\
                  * * * * *
                  * & * * *
                  ? * ? * *
                  * & * * *
                  * * * * *
                \* example  */
                neib = null;
                neib = cell.NeighbourUp;
                if (neib != null && neib.NeighbourUp != null && neib.NeighbourUp.IsSameType(cell))
                {
                    Cell second = LookForTheSecondCellHorizontal(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourUp);
                        result.Add(second);
                        break;
                    }
                }
            }

            if (result.Count > 0) break;
        }

        return result;
    }

    private List<Cell> GetPotentialMatch(Cell cell, Cell neighbour, Cell target)
    {
        List<Cell> result = new List<Cell>();

        if (neighbour != null && neighbour.IsSameType(cell))
        {
            Cell third = LookForTheThirdCell(target, neighbour);
            if (third != null)
            {
                result.Add(cell);
                result.Add(neighbour);
                result.Add(third);
            }
        }

        return result;
    }

    private Cell LookForTheSecondCellHorizontal(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look right
        Cell second = null;
        second = target.NeighbourRight;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        //look left
        second = null;
        second = target.NeighbourLeft;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        return null;
    }

    private Cell LookForTheSecondCellVertical(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up        
        Cell second = target.NeighbourUp;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        //look bottom
        second = null;
        second = target.NeighbourBottom;
        if (second != null && second.IsSameType(main))
        {
            return second;
        }

        return null;
    }

    private Cell LookForTheThirdCell(Cell target, Cell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up
        Cell third = CheckThirdCell(target.NeighbourUp, main);
        if (third != null)
        {
            return third;
        }

        //look right
        third = null;
        third = CheckThirdCell(target.NeighbourRight, main);
        if (third != null)
        {
            return third;
        }

        //look bottom
        third = null;
        third = CheckThirdCell(target.NeighbourBottom, main);
        if (third != null)
        {
            return third;
        }

        //look left
        third = null;
        third = CheckThirdCell(target.NeighbourLeft, main); ;
        if (third != null)
        {
            return third;
        }

        return null;
    }

    private Cell CheckThirdCell(Cell target, Cell main)
    {
        if (target != null && target != main && target.IsSameType(main))
        {
            return target;
        }

        return null;
    }

    internal void ShiftDownItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            int shifts = 0;
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                if (cell.IsEmpty)
                {
                    shifts++;
                    continue;
                }

                if (shifts == 0) continue;

                Cell holder = m_cells[x, y - shifts];

                Item item = cell.Item;
                cell.Free();

                holder.Assign(item);
                item.View.DOMove(holder.transform.position, 0.3f);
            }
        }
    }

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }
    }
    

