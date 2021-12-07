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

    public void MoveTo(PieceMovement move)
    {
        Cell.Piece = null;
        SetPosition(move.Path.LastOrDefault());
        
        if (ReachedLastRow())
            PromoteKing();

        foreach (var cell in move.Path.Where(cell => !cell.IsEmpty() && !cell.Piece.IsTeam(Color)))
        {
            cell.Piece.Remove();
        }

        EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
    }

    private bool ReachedLastRow() => Cell.Position.Row == LastRow;

    public bool CanMoveTo(BoardCell cell, out PieceMovement move)
    {
        move = null;
        return false;
    }

    public List<PieceMovement> GetPossibleMoves()
    {
        // TODO
        return null;
    }
    
    private void Remove()
    {
        Destroy(gameObject);
        GameController.Instance.DecreaseCountFor(Color);
        EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
    }
}