using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TeamColor TeamColor { get; set; }
    public BoardCell CurrentCell { get; set; }
    
    private bool IsTeam(TeamColor teamColor) => TeamColor == teamColor;

    private int Direction => TeamColor == TeamColor.White ? 1 : -1;

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

        var row = CurrentCell.CellCoordinates.Row;
        var column = CurrentCell.CellCoordinates.Column;
        
        if (IsLastRow(row))
            return possibleMoves;

        possibleMoves.AddRange(GetLeftPossibleMoves(column, row));
        possibleMoves.AddRange(GetRightPossibleMoves(column, row));

        return possibleMoves;
    }

    private IEnumerable<BoardCell> GetRightPossibleMoves(int column, int row)
    {
        var possibleMoves = new List<BoardCell>();

        if (column >= 7) return possibleMoves;
        
        var rightColumn = BoardGrid.Instance.Cells[row + Direction, column + 1];
        if (!rightColumn.IsEmpty()) return possibleMoves;
        
        possibleMoves.Add(rightColumn);

        return possibleMoves;
    }

    private IEnumerable<BoardCell> GetLeftPossibleMoves(int column, int row)
    {
        var possibleMoves = new List<BoardCell>();

        if (column <= 0) return possibleMoves;
        
        var leftColumn = BoardGrid.Instance.Cells[row + Direction, column - 1];
        if (leftColumn.IsEmpty())
            possibleMoves.Add(leftColumn);

        return possibleMoves;
    }
    
    private bool IsLastRow(int nextRow)
    {
        var whiteLastRow = IsTeam(TeamColor.White) && nextRow > 7;
        var blackLastRow = IsTeam(TeamColor.Black) && nextRow < 0;
        return whiteLastRow || blackLastRow;
    }
}
