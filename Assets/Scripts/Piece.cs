using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TeamColor TeamColor { get; set; }
    public BoardCell CurrentCell { get; set; }

    public bool IsQueen { get; set; }

    private int ForwardDirection => TeamColor == TeamColor.White ? 1 : -1;

    public void SetPosition(BoardCell cell)
    {
        CurrentCell = cell;
        CurrentCell.CurrentPiece = this;
        transform.position = CurrentCell.transform.position;
    }

    public void MoveTo(PieceMovement move)
    {
        SetPosition(move.Path.LastOrDefault());
        if (ReachedLastRow())
            IsQueen = true;

        foreach (var cell in move.Path.Where(cell => !cell.IsEmpty() && !cell.CurrentPiece.IsTeam(TeamColor)))
        {
            cell.CurrentPiece.Remove();
        }

        EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
    }

    private bool ReachedLastRow()
    {
        return TeamColor == TeamColor.White && CurrentCell.CellCoordinates.Row == 7 ||
               TeamColor == TeamColor.Black && CurrentCell.CellCoordinates.Row == 0;
    }

    private void Remove()
    {
        Destroy(gameObject);
        GameController.Instance.DecreaseCountFor(TeamColor);
        EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
    }

    public bool CanMoveTo(BoardCell cell, out PieceMovement foundMoves)
    {
        var possibleMoves = GetPossibleMoves();
        foundMoves = possibleMoves.FirstOrDefault(x => x.Path.Contains(cell));
        return foundMoves != null;
    }

    private IEnumerable<PieceMovement> GetPossibleMoves()
    {
        var possibleMoves = new List<PieceMovement>();

        var nextCells = GetNextCells().ToList();

        foreach (var nextCell in nextCells)
        {
            var boardCells = nextCell.ToList();
            boardCells.Insert(0, CurrentCell);

            var move = new PieceMovement();
            var cell = boardCells.FirstOrDefault();
            if (cell == null) continue;
            
            var isBackwards = nextCells.IndexOf(nextCell) > 1;
            
            while (HasValidMove(cell, boardCells, out var movePosition, isBackwards))
            {
                move.Path.AddRange(movePosition);
                cell = movePosition.LastOrDefault();

                if (movePosition.Count == 1 && !IsQueen)
                    break;
            }

            possibleMoves.Add(move);
        }

        return possibleMoves;
    }

    private bool HasValidMove(BoardCell cell, List<BoardCell> boardCells, out List<BoardCell> movePosition, bool isBackwards = false)
    {
        movePosition = new List<BoardCell>();
        var index = boardCells.IndexOf(cell);
        var nextCell = boardCells.ElementAtOrDefault(index + 1);

        if (nextCell == null)
            return false;

        if (index == 0 && nextCell.IsEmpty() && !isBackwards || IsQueen && nextCell.IsEmpty())
        {
            movePosition.Add(nextCell);
            return true;
        }

        var attackingCell = boardCells.ElementAtOrDefault(index + 2);
        if (!CanAttack(nextCell, attackingCell))
            return false;

        movePosition.Add(nextCell);
        movePosition.Add(attackingCell);
        return true;
    }

    private bool CanAttack(BoardCell cell, BoardCell nextCell)
    {
        return !cell.IsEmpty() && !cell.CurrentPiece.IsTeam(TeamColor) && nextCell != null && nextCell.IsEmpty();
    }

    public IEnumerable<IEnumerable<BoardCell>> GetNextCells()
    {
        yield return GetForwardLeftDiagonal();
        yield return GetForwardRightDiagonal();
        yield return GetBackwardsLeftDiagonal();
        yield return GetBackwardsRightDiagonal();
    }

    private IEnumerable<BoardCell> GetBackwardsRightDiagonal()
    {
        var currentColumn = CurrentCell.CellCoordinates.Column;
        var currentRow = CurrentCell.CellCoordinates.Row;
        var backwardDirection = ForwardDirection * -1;

        while (currentColumn < 7 && (backwardDirection < 0 && currentRow > 0 || backwardDirection > 0 && currentRow < 7))
        {
            currentRow += backwardDirection;
            yield return BoardGrid.Instance.GetCellAt(currentRow, ++currentColumn);
        }
    }

    private IEnumerable<BoardCell> GetBackwardsLeftDiagonal()
    {
        var currentColumn = CurrentCell.CellCoordinates.Column;
        var currentRow = CurrentCell.CellCoordinates.Row;
        var backwardDirection = ForwardDirection * -1;

        while (currentColumn > 0 &&
               (backwardDirection < 0 && currentRow > 0 || backwardDirection > 0 && currentRow < 7))
        {
            currentRow += backwardDirection;
            yield return BoardGrid.Instance.GetCellAt(currentRow, --currentColumn);
        }
    }

    public IEnumerable<BoardCell> GetForwardRightDiagonal()
    {
        var currentColumn = CurrentCell.CellCoordinates.Column;
        var currentRow = CurrentCell.CellCoordinates.Row;

        while (currentColumn < 7 && (ForwardDirection > 0 && currentRow < 7 || ForwardDirection < 0 && currentRow > 0))
        {
            currentRow += ForwardDirection;
            yield return BoardGrid.Instance.GetCellAt(currentRow, ++currentColumn);
        }
    }

    public IEnumerable<BoardCell> GetForwardLeftDiagonal()
    {
        var currentColumn = CurrentCell.CellCoordinates.Column;
        var currentRow = CurrentCell.CellCoordinates.Row;

        while (currentColumn > 0 &&
               (ForwardDirection > 0 && currentRow < 7 || ForwardDirection < 0 && currentRow > 0))
        {
            currentRow += ForwardDirection;
            yield return BoardGrid.Instance.GetCellAt(currentRow, --currentColumn);
        }
    }

    private void Update()
    {
        if (IsQueen)
        {
            var renderer = GetComponent<Renderer>();
            renderer.material.color = IsTeam(TeamColor.White) ? Color.yellow : Color.blue;
        }
    }

    /*private IEnumerable<PieceMovement> GetPossibleMoves(BoardCell cell, int columnDirection, PieceMovement movement = null)
    {
        movement ??= new PieceMovement();
        var possibleMoves = new List<PieceMovement>();

        var row = cell.CellCoordinates.Row;
        var column = cell.CellCoordinates.Column;
        
        var forwardColumn = BoardGrid.Instance.GetCellAt(row + Direction, column + columnDirection);
        /*var backwardsColumn = BoardGrid.Instance.GetCellAt(row + Direction * -1, column + columnDirection);#1#

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
    }*/

    private bool IsTeam(TeamColor teamColor) => teamColor == TeamColor;

    /*private bool CanAttack(Piece piece, BoardCell startCell, PieceMovement attackDestination)
    {
        var nextCell = GetNextCellFrom(piece, startCell.CellCoordinates.Column);

        if (!IsEnemy(piece) || nextCell == null || !nextCell.IsEmpty())
            return false;

        attackDestination.Destination = nextCell;
        attackDestination.AttackedPieces.Add(piece);
        return true;

    }*/

    /*private BoardCell GetNextCellFrom(Piece piece, int fromColumn = 0)
    {
        var enemyPosition = piece.CurrentCell.CellCoordinates;
        var currentPosition = fromColumn == 0 ? CurrentCell.CellCoordinates.Column : fromColumn;

        var columnDirection = enemyPosition.Column - currentPosition;
        var nextColumn = enemyPosition.Column + columnDirection;
        var nextRow = enemyPosition.Row + Direction;

        return BoardGrid.Instance.GetCellAt(nextRow, nextColumn);
    }*/

    /*private bool IsEnemy(Piece piece)
        => piece.TeamColor != TeamColor;*/
}