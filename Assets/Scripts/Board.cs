using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Board : MonoBehaviour
{
    private readonly Piece[,] _pieces = new Piece[8, 8];
    public GameObject whitePiecesPrefab;
    public GameObject blackPiecePrefab;
    public GameObject boardGrid;
    private BoardGrid _grid;
    [SerializeField] public string gridCellTag = "BoardCell";
    private Transform _selection;

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
        _grid.SetPieceAt(piece, x, y);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _grid = boardGrid.GetComponent<BoardGrid>();
        GenerateBoard();
    }

    void Update()
    {
        if (_selection != null)
        {
            var selectionRenderer = _selection.GetComponent<MeshRenderer>();
            selectionRenderer.enabled = false;
            _selection = null;
        }
        
        if (Camera.main is null) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            var selection = hit.transform;

            if (!selection.CompareTag(gridCellTag)) return;
            
            var selectionRenderer = selection.GetComponent<MeshRenderer>();
                
            if (selectionRenderer != null)
                selectionRenderer.enabled = true;

            _selection = selection;
        }
    }
}
