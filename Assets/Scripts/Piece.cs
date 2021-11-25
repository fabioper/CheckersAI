using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] public GameObject boardGrid;
    public TeamColor TeamColor { get; set; }
    public BoardCell CurrentCell { get; set; }
    
    public void MoveTo(BoardCell cell)
    {
        CurrentCell = cell;
        cell.CurrentPiece = this;
        transform.position = cell.transform.position;
    }

    public bool CanMoveTo(BoardCell cell)
    {
        var possibleMoves = GetPossibleMoves();
        return possibleMoves.Contains(cell);
    }

    public List<BoardCell> GetPossibleMoves()
    {
        var possibleMoves = new List<BoardCell>();
        var direction = TeamColor == TeamColor.White ? 1 : -1;
        
        var nextRow = CurrentCell.CellCoordinates.Row + direction;
        var column = CurrentCell.CellCoordinates.Column;
        
        if (nextRow > 7)
            return possibleMoves;

        if (column > 0)
        {
            var leftColumn = BoardGrid.Instance.Cells[nextRow, column - direction];
            if (leftColumn.IsEmpty())
                possibleMoves.Add(leftColumn);
        }

        if (column < 7)
        {
            var rightColumn = BoardGrid.Instance.Cells[nextRow, column + direction];
            if (rightColumn.IsEmpty())
                possibleMoves.Add(rightColumn);
        }

        return possibleMoves;
    }
}
