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
        if (ReachedLastRow(move.Path.LastOrDefault()))
            IsQueen = true;

        foreach (var cell in move.Path.Where(cell => !cell.IsEmpty() && !cell.CurrentPiece.IsTeam(TeamColor)))
        {
            cell.CurrentPiece.Remove();
        }

        EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
    }

    private bool ReachedLastRow(BoardCell cell)
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

    public bool CanMoveTo(BoardCell cell, out IEnumerable<PieceMovement> foundMoves)
    {
        var possibleMoves = GetPossibleMoves();
        foundMoves = possibleMoves.Where(x => x.Path.Contains(cell));
        return foundMoves.Any();
    }

    public IEnumerable<PieceMovement> GetPossibleMoves()
    {
        var possibleMoves = new List<PieceMovement>();

        var nextCells = GetNextCells();
        
        foreach (var nextCell in nextCells)
        {
            var boardCells = nextCell.ToList();
            boardCells.Insert(0, CurrentCell);
            
            var move = new PieceMovement();
            var cell = boardCells.FirstOrDefault();
            if (cell == null) continue;
            
            if (IsQueen)
            {
                var lastEmpty = boardCells.FindLastIndex(x => x.IsEmpty() || x.CurrentPiece.IsTeam(TeamColor));
                move.Path.AddRange(boardCells.TakeWhile((x, i) => i <= lastEmpty));
            }
            else
            {
                while (HasValidMove(cell, boardCells, out var movePosition))
                {
                    move.Path.AddRange(movePosition);
                    cell = movePosition.LastOrDefault();
    
                    if (movePosition.Count == 1)
                        break;
                }
            }

            possibleMoves.Add(move);
        }
        return possibleMoves;
    }

    private bool HasValidMove(BoardCell cell, List<BoardCell> boardCells, out List<BoardCell> movePosition)
    {
        movePosition = new List<BoardCell>();
        var index = boardCells.IndexOf(cell);
        var nextCell = boardCells.ElementAtOrDefault(index + 1);

        if (nextCell == null)
            return false;
        
        if (index == 0 && nextCell.IsEmpty())
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
        yield return GetLeftDiagonal();
        yield return GetRightDiagonal();
    }

    public IEnumerable<BoardCell> GetRightDiagonal()
    {
        var currentColumn = CurrentCell.CellCoordinates.Column;
        var currentRow = CurrentCell.CellCoordinates.Row;

        while (currentColumn < 7 && (ForwardDirection > 0 && currentRow < 7 || ForwardDirection < 0 && currentRow > 0))
        {
            currentRow += ForwardDirection;
            yield return BoardGrid.Instance.GetCellAt(currentRow, ++currentColumn);
        }
    }

    public IEnumerable<BoardCell> GetLeftDiagonal()
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
