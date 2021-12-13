using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Utils;

namespace Controllers
{
    public class GameController : MonoBehaviour
    {
        public Game CurrentGame { get; set; }
        
        private void Awake() => InitGame();

        private void InitGame()
        {
            CurrentGame = new Game();
        }

        private void MoveIA()
        {
            if (!CurrentGame.IsTurn(TeamColor.Black))
                return;

            var result = Minimax(CurrentGame.Board, 1, true);
            CurrentGame.Board = result.Board;
        }

        private void VerifyVictory()
        {
            if (CurrentGame.BlackPieces.Count == 0)
            {
                Debug.Log("Vitória das peças brancas!");
                CurrentGame.Winner = TeamColor.White;
                return;
            }

            if (CurrentGame.WhitePieces.Count == 0)
            {
                Debug.Log("Vitória das peças pretas!");
                CurrentGame.Winner = TeamColor.Black;
            }
        }

        public (double Evaluation, Board Board) Minimax(Board board, int depth, bool maxPlayer)
        {
            if (depth == 0 || CurrentGame.Winner.HasValue)
                return (board.Evaluate(), board);

            if (maxPlayer)
            {
                var maxEvaluation = double.NegativeInfinity;
                Board bestMove = null;

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
                Board bestMove = null;

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

        private IEnumerable<Board> GetAllMoves(Board board, TeamColor color)
        {
            var moves = new List<Board>();

            var allPieces = color == TeamColor.Black ? board.Game.BlackPieces : board.Game.WhitePieces;
        
            foreach (var piece in allPieces)
            {
                var validMoves = board.GetPossibleMoves(piece);
                foreach (var move in validMoves)
                {
                    var tempBoard = board.DeepCopy();
                    var tempPiece = tempBoard.GetPiece(piece.Cell.Position.Row, piece.Cell.Position.Column);
                    var newBoard = SimulateMove(tempPiece, move.Key, tempBoard, move.Value);
                    moves.Add(newBoard);
                }
            }

            return moves;
        }

        private static Board SimulateMove(Piece piece, CellCoordinates position, Board board, List<Cell> skips)
        {
            board.MoveTo(piece, (position, skips));
            if (skips.Any())
                board.Remove(piece);
            return board;
        }
    }
}