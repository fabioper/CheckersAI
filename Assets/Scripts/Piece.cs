using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using static Utils.EnumerableUtils;

public class Piece : MonoBehaviour
{

    private const int ROWS = 8;
    private const int COLS = 8;

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

    public Task<bool> CanMoveTo(BoardCell cell, out (CellCoordinates moveKey, List<BoardCell> pieceSkips)? move)
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

        return Task.Run(() => canMove );
    }

    private static Func<CellCoordinates, bool> SamePosition(BoardCell cell)
        => x => x.Column == cell.Position.Column && x.Row == cell.Position.Row;

    public Dictionary<CellCoordinates, List<BoardCell>> GetPossibleMoves()
    {
        var moves = new Dictionary<CellCoordinates, List<BoardCell>>();
        var left = Cell.Position.Column - 1;
        var right = Cell.Position.Column + 1;
        var row = Cell.Position.Row;

        if (IsTeam(TeamColor.Black) || IsKing)
        {
            moves.Update(TraverseLeft(row - 1, Math.Max(row - 3, -1), -1, Color, left));
            moves.Update(TraverseRight(row - 1, Math.Max(row - 3, -1), -1, Color, right));
        }
        
        if (IsTeam(TeamColor.White) || IsKing)
        {
            moves.Update(TraverseLeft(row + 1, Math.Min(row + 3, ROWS), 1, Color, left));
            moves.Update(TraverseRight(row + 1, Math.Min(row + 3, ROWS), 1, Color, right));
        }

        return moves;
    }

    private Dictionary<CellCoordinates, List<BoardCell>> TraverseLeft(int start, int stop, int step, TeamColor color, int left, List<BoardCell> skipped = null)
    {
        var moves = new Dictionary<CellCoordinates, List<BoardCell>>();
        var last = new List<BoardCell>();
        skipped ??= new List<BoardCell>();

        foreach (var r in Range(start, stop, step))
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
                    var row = step == -1 ? Math.Max(r - 3, 0) : Math.Min(r + 3, ROWS);

                    moves.Update(TraverseLeft(r + step, row, step, color, left - 1, last));
                    moves.Update(TraverseRight(r + step, row, step, color, left + 1, last));
                }
                else
                {
                    break;
                }
            }
            else if (current.Piece.IsTeam(color))
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

        foreach (var r in Range(start, stop, step))
        {
            if (right >= COLS)
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
                    var row = step == -1 ? Math.Max(r - 3, 0) : Math.Min(r + 3, ROWS);

                    moves.Update(TraverseLeft(r + step, row, step, color, right - 1, last));
                    moves.Update(TraverseRight(r + step, row, step, color, right + 1, last));
                }
                else
                {
                    break;
                }
            }
            else if (current.Piece.IsTeam(color))
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

    private void Awake()
    {
        GameController.Instance.Pieces.Add(this);
        Debug.Log(GameController.Instance.Pieces.Count);
    }

    private void Remove()
    {
        Destroy(gameObject);
        GameController.Instance.Pieces.Remove(this);
        EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
        Debug.Log(GameController.Instance.Pieces.Count);
    }
}