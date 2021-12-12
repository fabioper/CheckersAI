using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class GameController : MonoBehaviour
    {
        private static GameController _instance;

        public static GameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameController>();
                }

                return _instance;
            }
        }
        

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ActiveTeam = TeamColor.White;
            Pieces = new HashSet<PieceController>();
        }

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

            var result = Minimax(BoardGridController.Instance, 1, true);
            BoardGridController.Replace(result.Board);
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

        public (double Evaluation, BoardGridController Board) Minimax(BoardGridController board, int depth, bool maxPlayer)
        {
            if (depth == 0 || Winner.HasValue)
                return (board.Evaluate(), board);

            if (maxPlayer)
            {
                var maxEvaluation = double.NegativeInfinity;
                BoardGridController bestMove = null;

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
                BoardGridController bestMove = null;

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

        private IEnumerable<BoardGridController> GetAllMoves(BoardGridController board, TeamColor color)
        {
            var moves = new List<BoardGridController>();

            var allPieces = color == TeamColor.Black ? BlackPieces : WhitePieces;
        
            foreach (var piece in allPieces)
            {
                var validMoves = board.GetPossibleMoves(piece);
                foreach (var move in validMoves)
                {
                    var tempBoard = board.Clone();
                    var tempPiece = tempBoard.GetPiece(piece.Cell.Position.Row, piece.Cell.Position.Column);
                    var newBoard = SimulateMove(tempPiece, move.Key, tempBoard, move.Value);
                    moves.Add(newBoard);
                }
            }

            return moves;
        }

        private static BoardGridController SimulateMove(PieceController piece, CellCoordinates position, BoardGridController board, List<BoardCellController> skips)
        {
            board.MoveTo(piece, (position, skips));
            if (skips.Any())
                board.Remove(piece);
            return board;
        }
    }
}