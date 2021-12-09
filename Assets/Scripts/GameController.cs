using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public HashSet<Piece> Pieces { get; set; }
    public TeamColor? Winner { get; set; }

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

        var result = Minimax(BoardGrid.Instance, 1, true);
        BoardGrid.Replace(result.Board);
    }

    private void VerifyVictory()
    {
        if (BlackPieces.Count == 0)
        {
            Debug.Log("Vitória das peças brancas!");
            Winner = TeamColor.White;
            return;
        }

        if (WhitePieces.Count == 0)
        {
            Debug.Log("Vitória das peças pretas!");
            Winner = TeamColor.Black;
        }
    }

    public (double Evaluation, BoardGrid Board) Minimax(BoardGrid board, int depth, bool maxPlayer)
    {
        if (depth == 0 || Winner.HasValue)
            return (board.Evaluate(), board);

        if (maxPlayer)
        {
            var maxEvaluation = double.NegativeInfinity;
            BoardGrid bestMove = null;

            foreach (var move in GetAllMoves(board, TeamColor.Black))
            {
                var (evaluation, _) = Minimax(move, depth - 1, false);
                maxEvaluation = Math.Max(maxEvaluation, evaluation);

                if (Math.Abs(maxEvaluation - evaluation) == 0)
                    bestMove = move;
            }

            return (maxEvaluation, bestMove);
        }
        else
        {
            var minEvaluation = double.PositiveInfinity;
            BoardGrid bestMove = null;

            foreach (var move in GetAllMoves(board, TeamColor.White))
            {
                var (evaluation, _) = Minimax(move, depth - 1, true);
                minEvaluation = Math.Min(minEvaluation, evaluation);

                if (Math.Abs(minEvaluation - evaluation) == 0)
                    bestMove = move;
            }

            return (minEvaluation, bestMove);
        }
    }

    private IEnumerable<BoardGrid> GetAllMoves(BoardGrid board, TeamColor color)
    {
        var moves = new List<BoardGrid>();

        var allPieces = color == TeamColor.Black ? BlackPieces : WhitePieces;
        
        foreach (var piece in allPieces)
        {
            var validMoves = board.GetPossibleMoves(piece);
            foreach (var move in validMoves)
            {
                var tempBoard = board.Clone();
                var tempPiece = piece.Clone();
                var newBoard = SimulateMove(tempPiece, move.Key, tempBoard, move.Value);
                moves.Add(newBoard);
            }
        }

        return moves;
    }

    private static BoardGrid SimulateMove(Piece piece, CellCoordinates position, BoardGrid board, List<BoardCell> skips)
    {
        board.MoveTo(piece, (position, skips));
        if (skips.Any())
            board.Remove(piece);
        return board;
    }
}