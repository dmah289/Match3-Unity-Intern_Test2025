using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

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

    public int BoardSizeY
    {
        get => boardSizeY;
        set => boardSizeY = value;
    }

    public BoardCell[,] Cells { get; }

    public Transform Root { get; }

    private int m_matchMin;

    public int RemainingCells { get; set; }
    
    public bool IsEmpty => RemainingCells == 0;

    public int[,] transformMatrix = new int[,]
    {
        {-4, -5, -3, 0},
        {2, -2, 2, -3},
        {1, 5, 3, 6},
        {-1, 1, 4, -6},
        {-2, 2, 1, 0},
        {-1, 0, -2, -1}
    };

    public Board(Transform transform, GameSettings gameSettings)
    {
        Root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        Cells = new BoardCell[boardSizeX, boardSizeY];

        CreateBoard();
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
                go.transform.SetParent(Root);

                BoardCell boardCell = go.GetComponent<BoardCell>();
                boardCell.Setup(x, y);

                Cells[x, y] = boardCell;
            }
        }

        //set neighbours
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (y + 1 < boardSizeY) Cells[x, y].NeighbourUp = Cells[x, y + 1];
                if (x + 1 < boardSizeX) Cells[x, y].NeighbourRight = Cells[x + 1, y];
                if (y > 0) Cells[x, y].NeighbourBottom = Cells[x, y - 1];
                if (x > 0) Cells[x, y].NeighbourLeft = Cells[x - 1, y];
            }
        }

    }

    internal void Fill()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                BoardCell boardCell = Cells[x, y];
                NormalItem item = new NormalItem();

                List<NormalItem.eNormalType> types = new List<NormalItem.eNormalType>();
                if (boardCell.NeighbourBottom != null)
                {
                    NormalItem nitem = boardCell.NeighbourBottom.Item as NormalItem;
                    if (nitem != null)
                    {
                        types.Add(nitem.ItemType);
                    }
                }

                if (boardCell.NeighbourLeft != null)
                {
                    NormalItem nitem = boardCell.NeighbourLeft.Item as NormalItem;
                    if (nitem != null)
                    {
                        types.Add(nitem.ItemType);
                    }
                }

                item.SetType(Utils.GetRandomNormalTypeExcept(types.ToArray()));
                item.SetView();
                item.SetViewRoot(Root);

                boardCell.Assign(item);
                boardCell.ApplyItemPosition(false);
            }
        }
    }

    internal void FillDivisableBy3()
    {
        RemainingCells = boardSizeX * boardSizeY;
        
        int numOfType = (boardSizeX * boardSizeY) / 3;
        NormalItem.eNormalType[] types = GetRandomTypes(numOfType);

        int[] typeIndexOnBoard = AllocateTypeIndices(types);
        
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                BoardCell boardCell = Cells[x, y];
                NormalItem item = new NormalItem();
                
                item.SetType(types[typeIndexOnBoard[x * BoardSizeY + y]]);
                item.SetView();
                item.SetViewRoot(Root);
                
                boardCell.Assign(item);
                boardCell.ApplyItemPosition(false);
            }
        }
    }

    private int[] AllocateTypeIndices(NormalItem.eNormalType[] types)
    {
        int[] typeIndexOnBoard = new int[types.Length * 3];

        for (int i = 0; i < typeIndexOnBoard.Length; i++)
        {
            typeIndexOnBoard[i] = i % types.Length;
        }
        
        typeIndexOnBoard.Shuffle();
        
        return typeIndexOnBoard;
    }

    private NormalItem.eNormalType[] GetRandomTypes(int numOfTypes)
    {
        NormalItem.eNormalType[] types = new NormalItem.eNormalType[numOfTypes];

        for (int i = 0; i < Constants.NUMBER_OF_NORMAL_ITEM; i++)
        {
            types[i] = (NormalItem.eNormalType)i;
        }
        
        for (int i = Constants.NUMBER_OF_NORMAL_ITEM; i < numOfTypes; i++)
        {
            types[i] = Utils.GetRandomNormalType();
        }

        return types;
    }

    internal void Shuffle()
    {
        List<Item> list = new List<Item>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                list.Add(Cells[x, y].Item);
                Cells[x, y].Free();
            }
        }

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int rnd = UnityEngine.Random.Range(0, list.Count);
                Cells[x, y].Assign(list[rnd]);
                Cells[x, y].ApplyItemMoveToPosition();

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
                BoardCell boardCell = Cells[x, y];
                if (!boardCell.IsEmpty) continue;

                NormalItem item = new NormalItem();

                item.SetType(Utils.GetRandomNormalType());
                item.SetView();
                item.SetViewRoot(Root);

                boardCell.Assign(item);
                boardCell.ApplyItemPosition(true);
            }
        }
    }

    internal void ExplodeAllItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                BoardCell boardCell = Cells[x, y];
                boardCell.ExplodeItem();
            }
        }
    }

    public void Swap(BoardCell cell1, BoardCell cell2, Action callback)
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

    public List<BoardCell> GetHorizontalMatches(BoardCell boardCell)
    {
        List<BoardCell> list = new List<BoardCell>();
        list.Add(boardCell);

        //check horizontal match
        BoardCell newcell = boardCell;
        while (true)
        {
            BoardCell neib = newcell.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(boardCell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = boardCell;
        while (true)
        {
            BoardCell neib = newcell.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(boardCell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }


    public List<BoardCell> GetVerticalMatches(BoardCell boardCell)
    {
        List<BoardCell> list = new List<BoardCell>();
        list.Add(boardCell);

        BoardCell newcell = boardCell;
        while (true)
        {
            BoardCell neib = newcell.NeighbourUp;
            if (neib == null) break;

            if (neib.IsSameType(boardCell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = boardCell;
        while (true)
        {
            BoardCell neib = newcell.NeighbourBottom;
            if (neib == null) break;

            if (neib.IsSameType(boardCell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }

    internal void ConvertNormalToBonus(List<BoardCell> matches, BoardCell boardCellToConvert)
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
            if (boardCellToConvert == null)
            {
                int rnd = UnityEngine.Random.Range(0, matches.Count);
                boardCellToConvert = matches[rnd];
            }

            item.SetView();
            item.SetViewRoot(Root);

            boardCellToConvert.Free();
            boardCellToConvert.Assign(item);
            boardCellToConvert.ApplyItemPosition(true);
        }
    }


    internal eMatchDirection GetMatchDirection(List<BoardCell> matches)
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

    internal List<BoardCell> FindFirstMatch()
    {
        List<BoardCell> list = new List<BoardCell>();

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                BoardCell boardCell = Cells[x, y];

                var listhor = GetHorizontalMatches(boardCell);
                if (listhor.Count >= m_matchMin)
                {
                    list = listhor;
                    break;
                }

                var listvert = GetVerticalMatches(boardCell);
                if (listvert.Count >= m_matchMin)
                {
                    list = listvert;
                    break;
                }
            }
        }

        return list;
    }

    public List<BoardCell> CheckBonusIfCompatible(List<BoardCell> matches)
    {
        var dir = GetMatchDirection(matches);

        var bonus = matches.Where(x => x.Item is BonusItem).FirstOrDefault();
        if(bonus == null)
        {
            return matches;
        }

        List<BoardCell> result = new List<BoardCell>();
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

    internal List<BoardCell> GetPotentialMatches()
    {
        List<BoardCell> result = new List<BoardCell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                BoardCell boardCell = Cells[x, y];

                //check right
                /* example *\
                  * * * * *
                  * * * * *
                  * * * ? *
                  * & & * ?
                  * * * ? *
                \* example  */

                if (boardCell.NeighbourRight != null)
                {
                    result = GetPotentialMatch(boardCell, boardCell.NeighbourRight, boardCell.NeighbourRight.NeighbourRight);
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
                if (boardCell.NeighbourUp != null)
                {
                    result = GetPotentialMatch(boardCell, boardCell.NeighbourUp, boardCell.NeighbourUp.NeighbourUp);
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
                if (boardCell.NeighbourBottom != null)
                {
                    result = GetPotentialMatch(boardCell, boardCell.NeighbourBottom, boardCell.NeighbourBottom.NeighbourBottom);
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
                if (boardCell.NeighbourLeft != null)
                {
                    result = GetPotentialMatch(boardCell, boardCell.NeighbourLeft, boardCell.NeighbourLeft.NeighbourLeft);
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
                BoardCell neib = boardCell.NeighbourRight;
                if (neib != null && neib.NeighbourRight != null && neib.NeighbourRight.IsSameType(boardCell))
                {
                    BoardCell second = LookForTheSecondCellVertical(neib, boardCell);
                    if (second != null)
                    {
                        result.Add(boardCell);
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
                neib = boardCell.NeighbourUp;
                if (neib != null && neib.NeighbourUp != null && neib.NeighbourUp.IsSameType(boardCell))
                {
                    BoardCell second = LookForTheSecondCellHorizontal(neib, boardCell);
                    if (second != null)
                    {
                        result.Add(boardCell);
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

    private List<BoardCell> GetPotentialMatch(BoardCell boardCell, BoardCell neighbour, BoardCell target)
    {
        List<BoardCell> result = new List<BoardCell>();

        if (neighbour != null && neighbour.IsSameType(boardCell))
        {
            BoardCell third = LookForTheThirdCell(target, neighbour);
            if (third != null)
            {
                result.Add(boardCell);
                result.Add(neighbour);
                result.Add(third);
            }
        }

        return result;
    }

    private BoardCell LookForTheSecondCellHorizontal(BoardCell target, BoardCell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look right
        BoardCell second = null;
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

    private BoardCell LookForTheSecondCellVertical(BoardCell target, BoardCell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up        
        BoardCell second = target.NeighbourUp;
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

    private BoardCell LookForTheThirdCell(BoardCell target, BoardCell main)
    {
        if (target == null) return null;
        if (target.IsSameType(main)) return null;

        //look up
        BoardCell third = CheckThirdCell(target.NeighbourUp, main);
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

    private BoardCell CheckThirdCell(BoardCell target, BoardCell main)
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
                BoardCell boardCell = Cells[x, y];
                if (boardCell.IsEmpty)
                {
                    shifts++;
                    continue;
                }

                if (shifts == 0) continue;

                BoardCell holder = Cells[x, y - shifts];

                Item item = boardCell.Item;
                boardCell.Free();

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
                BoardCell boardCell = Cells[x, y];
                boardCell.Clear();

                GameObject.Destroy(boardCell.gameObject);
                Cells[x, y] = null;
            }
        }
    }

    public void Task1()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                BoardCell boardCell = Cells[x, y];

                FishItem fishItem = ConvertToFishItem(boardCell);
                fishItem.SetView();
                fishItem.SetViewRoot(Root);
                
                boardCell.Clear();
                boardCell.Assign(fishItem);
                boardCell.ApplyItemPosition(false);
            }
        }
    }

    private FishItem ConvertToFishItem(BoardCell boardCell)
    {
        NormalItem normalItem = boardCell.Item as NormalItem;
        
        FishItem fishItem = new FishItem();

        int normalTypeIndex = (int)normalItem.ItemType;
        
        int fishTypeIndex = (normalTypeIndex + transformMatrix[boardCell.BoardY, boardCell.BoardX]) % Constants.NUMBER_OF_NORMAL_ITEM;
        
        if(fishTypeIndex < 0) 
            fishTypeIndex += Constants.NUMBER_OF_NORMAL_ITEM;

        fishItem.SetType((FishItem.eFishType)fishTypeIndex);
        
        return fishItem;
    }

    public void OnItemSelected()
    {
        RemainingCells--;
    }
}
