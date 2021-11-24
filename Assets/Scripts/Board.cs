using UnityEngine;

public class Board : MonoBehaviour
{
    private readonly Piece[,] _pieces = new Piece[8, 8];
    public GameObject whitePiecesPrefab;
    public GameObject blackPiecePrefab;
    public GameObject boardGrid;
    public BoardGrid grid;

    private void GenerateBoard()
    {
        for (var row = 0; row < 3; row++)
        {
            var oddRow = IsOdd(row);
            for (var column = 0; column < 8; column += 2)
                GeneratePiece(row, oddRow ? column : column + 1, whitePiecesPrefab);
        }
        
        for (var row = 7; row > 4; row--)
        {
            var oddRow = IsOdd(row);
            for (var column = 0; column < 8; column += 2)
                GeneratePiece(row, oddRow ? column : column + 1, blackPiecePrefab);
        }
    }

    private static bool IsOdd(int y)
    {
        var oddRow = y % 2 == 0;
        return oddRow;
    }

    private void GeneratePiece(int x, int y, GameObject prefab)
    {
        var go = Instantiate(prefab, transform, true);
        var piece = go.GetComponent<Piece>();
        _pieces[x, y] = piece;
        MovePiece(piece, x, y);
    }

    private void MovePiece(Piece piece, int x, int y)
    {
        grid.SetPieceAt(piece, x, y);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        InitializeBoardGrid();
        GenerateBoard();
    }

    private void InitializeBoardGrid()
    {
        grid = boardGrid.GetComponent<BoardGrid>();
        grid.InitGrid();
    }
}
