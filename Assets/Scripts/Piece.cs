using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using static Utils.EnumerableUtils;

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

    private void Awake() => GameController.Instance.Pieces.Add(this);

    private void Remove()
    {
        Destroy(gameObject);
        GameController.Instance.Pieces.Remove(this);
        EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
    }
}