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
    }
}