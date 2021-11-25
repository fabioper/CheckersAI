using UnityEngine;

public class Board : MonoBehaviour
{
    public GameObject whitePiecesPrefab;
    public GameObject blackPiecePrefab;

    private void GenerateBoard()
    {
        for (var row = 0; row < 3; row++)
        {
            var oddRow = IsOdd(row);
            for (var column = 0; column < 8; column += 2)
                GeneratePiece(row, oddRow ? column : column + 1, TeamColor.White);
        }
        
        for (var row = 7; row > 4; row--)
        {
            var oddRow = IsOdd(row);
            for (var column = 0; column < 8; column += 2)
                GeneratePiece(row, oddRow ? column : column + 1, TeamColor.Black);
        }
    }

    private GameObject GetPrefabFor(TeamColor teamColor)
        => teamColor == TeamColor.White ? whitePiecesPrefab : blackPiecePrefab;

    private static bool IsOdd(int y)
    {
        var oddRow = y % 2 == 0;
        return oddRow;
    }

    private void GeneratePiece(int x, int y, TeamColor teamColor)
    {
        var go = Instantiate(GetPrefabFor(teamColor), transform, true);
        var piece = go.GetComponent<Piece>();
        piece.TeamColor = teamColor;
        MovePiece(piece, x, y);
    }

    private void MovePiece(Piece piece, int x, int y)
    {
        BoardGrid.Instance.SetPieceAt(piece, x, y);
    }
    
    void Start()
    {
        InitializeBoardGrid();
        GenerateBoard();
    }

    private void InitializeBoardGrid()
    {
        BoardGrid.Instance.InitGrid();
    }
}
