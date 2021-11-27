using System.Collections.Generic;
using System.Linq;
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

        possibleMoves.AddRange(GetPossibleMoves(column, row, -1));
        possibleMoves.AddRange(GetPossibleMoves(column, row, 1));

        return possibleMoves;
    }

    private IEnumerable<BoardCell> GetPossibleMoves(int column, int row, int columnDirection)
    {
        var possibleMoves = new List<BoardCell>();

        var currentCell = BoardGrid.Instance.GetCellAt(row, column);
        var nextColumn = BoardGrid.Instance.GetCellAt(row + Direction, column + columnDirection);

        if (nextColumn is null)
            return possibleMoves;

        if (nextColumn.IsEmpty())
        {
            possibleMoves.Add(nextColumn);

            return possibleMoves;
        }

        if (CanAttack(nextColumn.CurrentPiece, out var attackingDestination))
        {
            possibleMoves.Add(attackingDestination);
        }

        return possibleMoves;
    }

    private bool CanAttack(Piece piece, out BoardCell attackDestination)
    {
        attackDestination = null;
        
        var nextCell = GetNextCellFrom(piece);

        if (!IsEnemy(piece) || nextCell == null || !nextCell.IsEmpty())
            return false;

        attackDestination = nextCell;
        return true;

    }

    private BoardCell GetNextCellFrom(Piece piece)
    {
        var enemyPosition = piece.CurrentCell.CellCoordinates;
        var currentPosition = CurrentCell.CellCoordinates;

        var columnDirection = enemyPosition.Column - currentPosition.Column;
        var nextColumn = enemyPosition.Column + columnDirection;
        var nextRow = enemyPosition.Row + Direction;

        return BoardGrid.Instance.GetCellAt(nextRow, nextColumn);
    }

    private bool IsEnemy(Piece piece)
        => piece.TeamColor != TeamColor;
}
