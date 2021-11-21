using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Board : MonoBehaviour
{
    private readonly Piece[,] _pieces = new Piece[8, 8];
    public GameObject whitePiecesPrefab;
    public GameObject blackPiecePrefab;
    public Vector3 boardOffset = new Vector3(-4.0f, 0.1f, -4.0f);
    [SerializeField] private float piecesHorizontalDistance = 0.235f;
    [SerializeField] private float piecesVerticalDistance = 0.24f;
    [SerializeField] private Vector3 piecesOffset = new Vector3(0.48f, 0, 0.48f);

    private void GenerateBoard()
    {
        for (var y = 0; y < 3; y++)
        {
            var oddRow = IsOddRow(y);
            for (var x = 0; x < 8; x += 2)
                GeneratePiece(oddRow ? x : x + 1, y);
        }
        
        for (var y = 7; y > 4; y--)
        {
            var oddRow = IsOddRow(y);
            for (var x = 0; x < 8; x += 2)
                GeneratePiece(oddRow ? x : x + 1, y, false);
        }
    }

    private static bool IsOddRow(int y)
    {
        var oddRow = y % 2 == 0;
        return oddRow;
    }

    private void GeneratePiece(int x, int y, bool isWhite = true)
    {
        var prefab = isWhite ? whitePiecesPrefab : blackPiecePrefab;
        var go = Instantiate(prefab, transform, true);
        var piece = go.GetComponent<Piece>();
        _pieces[x, y] = piece;
        MovePiece(piece, x, y);
    }

    private void MovePiece(Piece p, int x, int y)
    {
        var horizontal = new Vector3(piecesHorizontalDistance, 0, 0);
        var vertical = new Vector3(0, 0, piecesVerticalDistance);
        p.transform.position = (horizontal * x) + (vertical * y) + boardOffset + piecesOffset;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
