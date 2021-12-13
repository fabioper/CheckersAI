using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Utils;

namespace Models
{
    public class Board
    {
        private const int ROWS = 8;
        private const int COLS = 8;

        public Cell[,] Cells;
        public Game Game { get; set; }

        public Board(Game game)
        {
            Game = game;
            Cells = new Cell[8,8];
            
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    Cells[i, j] = new Cell();
                }
            }
        }

        public Cell GetCellAt(int row, int column)
        {
            Cell foundCell = null;

            try
            {
                foundCell = Cells[row, column];
                return foundCell;
            }
            catch (Exception)
            {
            }

            return foundCell;
        }
        
        public Piece GetPiece(int row, int col)
        {
            var cell = GetCellAt(row, col);
            return cell.IsEmpty() ? null : cell.Piece;
        }
        
        public Dictionary<CellCoordinates, List<Cell>> GetPossibleMoves(Piece piece)
        {
            var moves = new Dictionary<CellCoordinates, List<Cell>>();
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

        private Dictionary<CellCoordinates, List<Cell>> TraverseLeft(int start, int stop, int step,
            TeamColor color, int left, List<Cell> skipped = null)
        {
            var moves = new Dictionary<CellCoordinates, List<Cell>>();
            var last = new List<Cell>();
            skipped ??= new List<Cell>();

            foreach (var r in EnumerableUtils.Range(start, stop, step))
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
                    last = new List<Cell> { current };
                }

                left -= 1;
            }

            return moves;
        }

        private Dictionary<CellCoordinates, List<Cell>> TraverseRight(int start, int stop, int step,
            TeamColor color, int right, List<Cell> skipped = null)
        {
            var moves = new Dictionary<CellCoordinates, List<Cell>>();
            var last = new List<Cell>();
            skipped ??= new List<Cell>();

            foreach (var r in EnumerableUtils.Range(start, stop, step))
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
                    last = new List<Cell> { current };
                }

                right += 1;
            }

            return moves;
        }
        
        public bool CanMoveTo(Piece piece, Cell cell,
            out (CellCoordinates moveKey, List<Cell> pieceSkips)? move)
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

            return canMove;
        }

        private static Func<CellCoordinates, bool> SamePosition(Cell cell)
            => x => x.Column == cell.Position.Column && x.Row == cell.Position.Row;
        
        public void MoveTo(Piece piece, (CellCoordinates moveKey, List<Cell> pieceSkips) move)
        {
            // if (piece == null) return;
            piece.Cell.Piece = null;

            var (moveKey, pieceSkips) = move;
            SetPosition(piece, GetCellAt(moveKey.Row, moveKey.Column));

            if (piece.ReachedLastRow())
                piece.PromoteKing();

            foreach (var skippedPiece in pieceSkips.Where(x => !x.IsEmpty()).Select(x => x.Piece))
            {
                skippedPiece.Remove();
            }

            EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
        }
        
        public void Remove(Piece piece) => piece.Remove();

        public double Evaluate()
        {
            var blackPiecesCount = Game.BlackPieces.Count;
            var whitePiecesCount = Game.WhitePieces.Count;
            var blackKingsCount = Game.BlackKings.Count;
            var whiteKingsCount = Game.WhiteKings.Count;

            return blackPiecesCount - whitePiecesCount + (blackKingsCount * 0.5 - whiteKingsCount * 0.5);
        }
        
        public void SetPosition(Piece piece, Cell cell)
        {
            piece.Cell = cell;
            cell.Piece = piece;
        }
    }
}