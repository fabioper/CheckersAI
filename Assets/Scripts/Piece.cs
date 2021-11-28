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
        return possibleMoves.Select(x => x.Destination).Contains(cell);
    }

    public List<PieceMovement> GetPossibleMoves()
    {
        var possibleMoves = new List<PieceMovement>();

        var row = CurrentCell.CellCoordinates.Row;
        var column = CurrentCell.CellCoordinates.Column;

        possibleMoves.AddRange(GetPossibleMoves(column, row, -1));
        possibleMoves.AddRange(GetPossibleMoves(column, row, 1));

        return possibleMoves;
    }

    private IEnumerable<PieceMovement> GetPossibleMoves(int column, int row, int columnDirection)
    {
        var possibleMoves = new List<PieceMovement>();

        var currentCell = BoardGrid.Instance.GetCellAt(row, column);
        var nextColumn = BoardGrid.Instance.GetCellAt(row + Direction, column + columnDirection);

        if (nextColumn is null)
            return possibleMoves;

        if (!nextColumn.IsEmpty())
        {
            if (CanAttack(nextColumn.CurrentPiece, currentCell, out var attackingDestination))
            {
                var subsequentAttacks = GetSubsequentAttacks(attackingDestination);
                possibleMoves.AddRange(subsequentAttacks);
                return possibleMoves;
            }

            possibleMoves.Add(new PieceMovement
            {
                Start = CurrentCell,
                Destination = currentCell
            });
            return possibleMoves;
        }

        possibleMoves.Add(!currentCell.IsEmpty() ? new PieceMovement
        {
            Start = CurrentCell,
            Destination = nextColumn
        } : new PieceMovement
        {
            Start = CurrentCell,
            Destination = currentCell
        });
        return possibleMoves;
    }

    private IEnumerable<PieceMovement> GetSubsequentAttacks(PieceMovement attackingDestination)
    {
        var subsequentAttacks = new List<PieceMovement>();

        var rightAttacks = GetPossibleMoves(attackingDestination.Destination.CellCoordinates.Column,
            attackingDestination.Destination.CellCoordinates.Row, 1).ToList();


        if (rightAttacks.Any())
        {
            var destination = rightAttacks.Last().Destination;
            var attackedPieces = rightAttacks.SelectMany(x => x.AttackedPieces).ToList();
            subsequentAttacks.Add(new PieceMovement
            {
                Start = attackingDestination.Start,
                Destination = destination,
                AttackedPieces = attackedPieces
            });
        }

        var leftAttacks = GetPossibleMoves(attackingDestination.Destination.CellCoordinates.Column,
            attackingDestination.Destination.CellCoordinates.Row, -1).ToList();
        
        if (leftAttacks.Any())
        {
            subsequentAttacks.Add(new PieceMovement
            {
                Start = attackingDestination.Start,
                Destination = leftAttacks.Last().Destination,
                AttackedPieces = leftAttacks.SelectMany(x => x.AttackedPieces).ToList()
            });
        }
        
        return subsequentAttacks;
    }

    private bool CanAttack(Piece piece, BoardCell startCell, out PieceMovement attackDestination)
    {
        attackDestination = null;
        
        var nextCell = GetNextCellFrom(piece, startCell.CellCoordinates.Column);

        if (!IsEnemy(piece) || nextCell == null || !nextCell.IsEmpty())
            return false;

        attackDestination = new PieceMovement
        {
            Start = startCell,
            Destination = nextCell,
            AttackedPieces = new List<Piece> { piece } 
        };
        return true;

    }

    private BoardCell GetNextCellFrom(Piece piece, int fromColumn = 0)
    {
        var enemyPosition = piece.CurrentCell.CellCoordinates;
        var currentPosition = fromColumn == 0 ? CurrentCell.CellCoordinates.Column : fromColumn;

        var columnDirection = enemyPosition.Column - currentPosition;
        var nextColumn = enemyPosition.Column + columnDirection;
        var nextRow = enemyPosition.Row + Direction;

        return BoardGrid.Instance.GetCellAt(nextRow, nextColumn);
    }

    private bool IsEnemy(Piece piece)
        => piece.TeamColor != TeamColor;
}
