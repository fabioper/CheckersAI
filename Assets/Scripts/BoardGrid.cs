using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public BoardCell[,] Cells = new BoardCell[8, 8];
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
                boardCell.CellCoordinates = new CellCoordinates(x, y);
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

    public void SetPieceAt(Piece piece, int x, int y)
    {
        piece.MoveTo(Cells[x, y]);
    }
}
