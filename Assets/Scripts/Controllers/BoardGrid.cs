using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using static Utils.EnumerableUtils;

namespace Controllers
{
    public class BoardGrid : MonoBehaviour
    {
        private const int ROWS = 8;
        private const int COLS = 8;
    
        public BoardCell[,] Cells = new BoardCell[8, 8];
        public BoardCell SelectedCell { get; set; }
    
        public static BoardGrid Instance { get; private set; }
    
        private void Awake()
        {
            Instance = this;
        }
    
        public void InitGrid()
        {
            var cellPosition = 0;

            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    var boardCell = GetBoardCellFromChildAt(cellPosition);
                    boardCell.Position = new CellCoordinates(x, y);
                    Cells[x, y] = boardCell;
                    cellPosition++;
                }
            }
        }

        private BoardCell GetBoardCellFromChildAt(int cellPosition)
        {
            var child = transform.GetChild(cellPosition);
            var cell = child.GetComponent<BoardCell>();
            return cell;
        }

        public void SetPieceAt(Piece piece, int x, int y) => piece.SetPosition(Cells[x, y]);

        public BoardCell GetCellAt(int row, int column)
        {
            BoardCell foundCell = null;
        
            try
            {
                foundCell = Cells[row, column];
                return foundCell;
            }
            catch (Exception)
            {
                // ignored
            }

            return foundCell;
        }
    
        public bool HasSelection() => SelectedCell != null;

        public bool IsSelected(BoardCell boardCell) => SelectedCell == boardCell;
    
        public Dictionary<CellCoordinates, List<BoardCell>> GetPossibleMoves(Piece piece)
        {
            var moves = new Dictionary<CellCoordinates, List<BoardCell>>();
            var left = piece.Cell.Position.Column - 1;
            var right = piece.Cell.Position.Column + 1;
            var row = piece.Cell.Position.Row;

            if (piece.IsTeam(TeamColor.Black) || piece.IsKing)
            {
                moves.Update(TraverseLeft(row - 1, Math.Max(row - 3, -1), -1, piece.Color, left));
                moves.Update(TraverseRight(row - 1, Math.Max(row - 3, -1), -1, piece.Color, right));
            }
        
            if (piece.IsTeam(TeamColor.White) || piece.IsKing)
            {
                moves.Update(TraverseLeft(row + 1, Math.Min(row + 3, ROWS), 1, piece.Color, left));
                moves.Update(TraverseRight(row + 1, Math.Min(row + 3, ROWS), 1, piece.Color, right));
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

                var current = GetCellAt(r, left);
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

                var current = GetCellAt(r, right);
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
    
        public Task<bool> CanMoveTo(Piece piece, BoardCell cell, out (CellCoordinates moveKey, List<BoardCell> pieceSkips)? move)
        {
            move = null;
        
            var possibleMoves = GetPossibleMoves(piece);
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
    
        public void MoveTo(Piece piece, (CellCoordinates moveKey, List<BoardCell> pieceSkips) move)
        {
            piece.Cell.Piece = null;
        
            var (moveKey, pieceSkips) = move;
            piece.SetPosition(GetCellAt(moveKey.Row, moveKey.Column));
        
            if (piece.ReachedLastRow())
                piece.PromoteKing();

            foreach (var skippedPiece in pieceSkips.Where(x => !x.IsEmpty()).Select(x => x.Piece))
            {
                skippedPiece.Remove();
            }

            EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
        }

        public double Evaluate()
        {
            var blackPiecesCount = Game.Instance.BlackPieces.Count;
            var whitePiecesCount = Game.Instance.WhitePieces.Count;
            var blackKingsCount = Game.Instance.BlackKings.Count;
            var whiteKingsCount = Game.Instance.WhiteKings.Count;

            return blackPiecesCount - whitePiecesCount + (blackKingsCount * 0.5 - whiteKingsCount * 0.5);
        }
    
        public void Remove(Piece piece) => piece.Remove();

        public static void Replace(BoardGrid newBoard)
        {
            Instance = newBoard;
            EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
        }

        public BoardGrid Clone() => (BoardGrid)MemberwiseClone();
    }
}
