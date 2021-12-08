using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TeamColor Color { get; set; }
    public BoardCell Cell { get; set; }

    public bool IsKing { get; set; }
    
    public bool IsTeam(TeamColor teamColor) => teamColor == Color;
    
    private int LastRow => Color switch
    {
        TeamColor.White => 7,
        TeamColor.Black => 0,
        _ => throw new Exception("")
    };

    public void SetPosition(BoardCell cell)
    {
        Cell = cell;
        Cell.Piece = this;
        transform.position = Cell.transform.position;
    }

    public void PromoteKing()
    {
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = IsTeam(TeamColor.White) ? UnityEngine.Color.yellow : UnityEngine.Color.blue;
        IsKing = true;
    }

    public void MoveTo((CellCoordinates moveKey, List<BoardCell> pieceSkips) move)
    {
        Cell.Piece = null;
        var (moveKey, pieceSkips) = move;
        SetPosition(BoardGrid.Instance.GetCellAt(moveKey.Row, moveKey.Column));
        
        if (ReachedLastRow())
            PromoteKing();

        foreach (var piece in pieceSkips.Where(x => !x.IsEmpty()).Select(x => x.Piece))
        {
            piece.Remove();
        }

        EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
    }

    private bool ReachedLastRow() => Cell.Position.Row == LastRow;

    public bool CanMoveTo(BoardCell cell, out (CellCoordinates moveKey, List<BoardCell> pieceSkips)? move)
    {
        move = null;
        
        var possibleMoves = GetPossibleMoves();
        var moveKey = possibleMoves.Keys.FirstOrDefault(SamePosition(cell));

        var canMove = moveKey != null;

        if (canMove)
        {
            possibleMoves.TryGetValue(moveKey, out var pieceSkips);
            move = (moveKey, pieceSkips);
        }

        return canMove;
    }

    private Func<CellCoordinates, bool> SamePosition(BoardCell cell)
    {
        return x => x.Column == cell.Position.Column && x.Row == cell.Position.Row;
    }

    public Dictionary<CellCoordinates, List<BoardCell>> GetPossibleMoves()
    {
        var moves = new Dictionary<CellCoordinates, List<BoardCell>>();
        var left = Cell.Position.Column + 1;
        var right = Cell.Position.Column - 1;
        var row = Cell.Position.Row;

        if (IsTeam(TeamColor.Black) || IsKing)
        {
            UpdateDict(moves, TraverseLeft(row - 1, Math.Max(row - 3, -1), -1, Color, left));
            UpdateDict(moves, TraverseRight(row - 1, Math.Max(row - 3, -1), -1, Color, right));
        }
        
        if (IsTeam(TeamColor.White) || IsKing)
        {
            UpdateDict(moves, TraverseLeft(row + 1, Math.Min(row + 3, 8), 1, Color, left));
            UpdateDict(moves, TraverseRight(row + 1, Math.Min(row + 3, 8), 1, Color, right));
        }

        return moves;
    }

    private Dictionary<CellCoordinates, List<BoardCell>> TraverseLeft(int start, int stop, int step, TeamColor color, int left, List<BoardCell> skipped = null)
    {
        var moves = new Dictionary<CellCoordinates, List<BoardCell>>();
        var last = new List<BoardCell>();
        skipped ??= new List<BoardCell>();


        foreach (var r in RangeIterator(start, stop, step))
        {
            if (left < 0)
                break;

            var current = BoardGrid.Instance.GetCellAt(r, left);
            if (current == null) break;
            if (current.IsEmpty())
            {
                if (skipped.Any() && !last.Any())
                    break;
                
                if (skipped.Any())
                    moves[new CellCoordinates(r, left)] = last.Concat(skipped).ToList();
                else
                    moves[new CellCoordinates(r, left)] = last;

                if (last.Any())
                {
                    var row = step == -1 ? Math.Max(r - 3, 0) : Math.Min(r + 3, 8);

                    UpdateDict(moves, TraverseLeft(r + step, row, step, color, left - 1, last));
                    UpdateDict(moves, TraverseRight(r + step, row, step, color, left + 1, last));
                }
                else
                {
                    break;
                }
            }
            else if (current.Piece.IsTeam(Color))
            {
                break;
            }
            else
            {
                last = new List<BoardCell> { current };
            }

            left -= 1;
        }

        return moves;
    }
    
    private Dictionary<CellCoordinates, List<BoardCell>> TraverseRight(int start, int stop, int step, TeamColor color, int right, List<BoardCell> skipped = null)
    {
        var moves = new Dictionary<CellCoordinates, List<BoardCell>>();
        var last = new List<BoardCell>();
        skipped ??= new List<BoardCell>();


        foreach (var r in RangeIterator(start, stop, step))
        {
            if (right >= 8)
                break;

            var current = BoardGrid.Instance.GetCellAt(r, right);
            if (current == null) break;
            if (current.IsEmpty())
            {
                if (skipped.Any() && !last.Any())
                    break;
                
                if (skipped.Any())
                    moves[new CellCoordinates(r, right)] = last.Concat(skipped).ToList();
                else
                    moves[new CellCoordinates(r, right)] = last;

                if (last.Any())
                {
                    var row = step == -1 ? Math.Max(r - 3, 0) : Math.Min(r + 3, 8);

                    UpdateDict(moves, TraverseLeft(r + step, row, step, color, right - 1, last));
                    UpdateDict(moves, TraverseRight(r + step, row, step, color, right + 1, last));
                }
                else
                {
                    break;
                }
            }
            else if (current.Piece.IsTeam(Color))
            {
                break;
            }
            else
            {
                last = new List<BoardCell> { current };
            }

            right += 1;
        }

        return moves;
    }

    private void Remove()
    {
        Destroy(gameObject);
        GameController.Instance.DecreaseCountFor(Color);
        EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
    }
    
    private static IEnumerable<int> RangeIterator(int start, int stop, int step)
    {
        var x = start;

        do
        {
            yield return x;
            x += step;
            if (step < 0 && x <= stop || 0 < step && stop <= x)
                break;
        }
        while (true);
    }

    public static void UpdateDict(Dictionary<CellCoordinates, List<BoardCell>> dict, Dictionary<CellCoordinates, List<BoardCell>> updatedDict)
    {
        foreach (var key in updatedDict.Keys)
        {
            dict[key] = updatedDict[key];
        }
    }
}