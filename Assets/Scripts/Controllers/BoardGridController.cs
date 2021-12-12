using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using static Utils.EnumerableUtils;

namespace Controllers
{
    public class BoardGridController : MonoBehaviour
    {
        
        public BoardCellController SelectedCell { get; set; }

        private static BoardGridController _instance;

        public static BoardGridController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BoardGridController>();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
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

        private BoardCellController GetBoardCellFromChildAt(int cellPosition)
        {
            var child = transform.GetChild(cellPosition);
            var cell = child.GetComponent<BoardCellController>();
            return cell;
        }

        public void SetPieceAt(PieceController piece, int x, int y) => piece.SetPosition(Cells[x, y]);

        

        public bool HasSelection() => SelectedCell != null;

        public bool IsSelected(BoardCellController boardCell) => SelectedCell == boardCell;


        public static void Replace(BoardGridController newBoard)
        {
            _instance = newBoard;
            EventsStore.Instance.NotifyEvent(GameEventType.MoveMade);
        }

        public BoardGridController Clone()
        {
            var board = Instantiate(this);
            var cells = new BoardCellController[8,8];
            
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    cells[i, j] = Cells[i, j].Clone();
                }
            }

            board.Cells = cells;
            
            return board;
        }

        public PieceController GetPiece(int row, int col)
        {
            var cell = GetCellAt(row, col);
            return cell.IsEmpty() ? null : cell.Piece;
        }
    }
}