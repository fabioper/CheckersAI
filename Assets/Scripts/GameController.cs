using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public int WhiteCount { get; set; }
    public int BlackCount { get; set; }

    private void Awake()
    {
        Instance = this;
        ActiveTeam = TeamColor.White;
        WhiteCount = 12;
        BlackCount = 12;
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
            
            var move = pieceMovements.ElementAtOrDefault(Random.Range(0, pieceMovements.Count - 1));
            if (move == null || !move.Path.Any())
                continue;

            randomCell.Piece.MoveTo(move);
            moved = true;
        }
    }

    private void VerifyVictory()
    {
        if (BlackCount == 0)
        {
            Debug.Log("Vitória das peças brancas!");
            return;
        }
        
        if (WhiteCount == 0)
        {
            Debug.Log("Vitória das peças pretas!");
        }
    }

    public void DecreaseCountFor(TeamColor teamColor)
    {
        switch (teamColor)
        {
            case TeamColor.White:
                WhiteCount--;
                break;
            case TeamColor.Black:
                BlackCount--;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(teamColor), teamColor, null);
        }
    }
}