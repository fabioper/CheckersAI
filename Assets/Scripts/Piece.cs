using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TeamColor TeamColor { get; set; }
    public BoardCell CurrentCell { get; set; }
    
    private int Direction => TeamColor == TeamColor.White ? 1 : -1;

    public void SetPosition(BoardCell cell)
    {
        CurrentCell = cell;
        CurrentCell.CurrentPiece = this;
        transform.position = CurrentCell.transform.position;
    }
    
    public void MoveTo(PieceMovement move)
    {
        SetPosition(move.Destination);

        if (move.AttackedPieces.Any())
            move.AttackedPieces.ForEach(piece => piece.Remove());

        EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
    }

    private void Remove()
    {
        Destroy(gameObject);
        GameController.Instance.DecreaseCountFor(TeamColor);
        EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
    }

    public bool CanMoveTo(BoardCell cell, out IEnumerable<PieceMovement> foundMoves)
    {
        var possibleMoves = GetPossibleMoves();
        foundMoves = possibleMoves.Where(x => x.Destination == cell);
        return foundMoves.Any();
    }

    public List<PieceMovement> GetPossibleMoves()
    {
        var possibleMoves = new List<PieceMovement>();

        possibleMoves.AddRange(GetPossibleMoves(CurrentCell, -1));
        possibleMoves.AddRange(GetPossibleMoves(CurrentCell, 1));

        return possibleMoves;
    }

    private IEnumerable<PieceMovement> GetPossibleMoves(BoardCell cell, int columnDirection, PieceMovement movement = null)
    {
        movement ??= new PieceMovement();
        var possibleMoves = new List<PieceMovement>();

        var row = cell.CellCoordinates.Row;
        var column = cell.CellCoordinates.Column;
        
        var forwardColumn = BoardGrid.Instance.GetCellAt(row + Direction, column + columnDirection);
        /*var backwardsColumn = BoardGrid.Instance.GetCellAt(row + Direction * -1, column + columnDirection);*/

        if (forwardColumn is null || !forwardColumn.IsEmpty() && forwardColumn.CurrentPiece.IsTeam(TeamColor))
        {
            return possibleMoves;
        }

        if (!forwardColumn.IsEmpty())
        {
            if (CanAttack(forwardColumn.CurrentPiece, cell, movement))
            {
                var sequence = GetPossibleMoves(movement.Destination, columnDirection, movement)
                    .Union(GetPossibleMoves(movement.Destination, columnDirection * -1, movement));

                possibleMoves.AddRange(sequence);
            }

            possibleMoves.Add(movement);
            return possibleMoves;
        }

        movement.Start = CurrentCell;
        movement.Destination = !cell.IsEmpty() ? forwardColumn : cell;

        possibleMoves.Add(movement);
        return possibleMoves;
    }

    private bool IsTeam(TeamColor teamColor) => teamColor == TeamColor;

    private bool CanAttack(Piece piece, BoardCell startCell, PieceMovement attackDestination)
    {
        var nextCell = GetNextCellFrom(piece, startCell.CellCoordinates.Column);

        if (!IsEnemy(piece) || nextCell == null || !nextCell.IsEmpty())
            return false;

        attackDestination.Destination = nextCell;
        attackDestination.AttackedPieces.Add(piece);
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
