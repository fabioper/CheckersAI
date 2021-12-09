using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public HashSet<Piece> Pieces { get; set; }

    public HashSet<Piece> WhitePieces
        => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.White)));
    public HashSet<Piece> BlackPieces
        => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.Black)));
    public HashSet<Piece> WhiteKings
        => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.Black) && x.IsKing));
    public HashSet<Piece> BlackKings
        => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.Black) && x.IsKing));

    private void Awake()
    {
        Instance = this;
        ActiveTeam = TeamColor.White;
        Pieces = new HashSet<Piece>();
    }

    public TeamColor ActiveTeam { get; set; }

    public bool IsTurn(TeamColor teamColor) => ActiveTeam == teamColor;

    public void ChangeTurn() => ActiveTeam = ActiveTeam == TeamColor.White ? TeamColor.Black : TeamColor.White;

    private void Start()
    {
        EventsStore.Instance.OnEvent(GameEventType.MoveMade, ChangeTurn);
        EventsStore.Instance.OnEvent(GameEventType.MoveMade, MoveIA);
        EventsStore.Instance.OnEvent(GameEventType.PieceAttacked, VerifyVictory);
    }

    private void MoveIA()
    {
        if (!IsTurn(TeamColor.Black))
            return;

        var moved = false;
        while (!moved)
        {
            var randomRow = Random.Range(0, 7);
            var randomColumn = Random.Range(0, 7);
            
            var randomCell = BoardGrid.Instance.GetCellAt(randomRow, randomColumn);
            
            if (randomCell == null || randomCell.IsEmpty() || randomCell.Piece.Color != TeamColor.Black)
                continue;

            var pieceMovements = randomCell.Piece.GetPossibleMoves();
            
            if (!pieceMovements.Any())
                continue;

            var randomKey = pieceMovements.Keys.ElementAtOrDefault(Random.Range(0, pieceMovements.Count - 1));
            if (randomKey != null)
            {
                var move = pieceMovements.FirstOrDefault(x =>
                    randomKey.Row == x.Key.Row && randomKey.Column == x.Key.Column);
                
                randomCell.Piece.MoveTo((move.Key, move.Value));
                moved = true;
            }
        }
    }

    private void VerifyVictory()
    {
        Debug.Log(WhitePieces.Count);
        if (BlackPieces.Count == 0)
        {
            Debug.Log("Vitória das peças brancas!");
            return;
        }
        
        if (WhitePieces.Count == 0)
        {
            Debug.Log("Vitória das peças pretas!");
        }
    }
}