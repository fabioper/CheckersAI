using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    private readonly BoardCell[,] _cells = new BoardCell[8, 8];
    
    // Start is called before the first frame update
    void Start()
    {
        var cellPosition = 0;

        for (var x = 0; x < 8; x++)
        {
            for (var y = 0; y < 8; y++)
            {
                var boardCell = GetBoardCellFromChildAt(cellPosition);
                _cells[x, y] = boardCell;
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
        var cell = _cells[x, y];
        piece.transform.position = cell.transform.position;
    }
}
