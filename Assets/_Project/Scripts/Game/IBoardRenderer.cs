using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public interface IBoardRenderer
    {
        void InitializeBoard ();
        void RenderBoard (Piece[, ] pieces);
        void PiecesChanged (params Piece[] pieces);
        void OnPieceClicked (Vector2Int coor);
        void OnPieceUp (Vector2Int coor);
        void OnPieceEnter (Vector2Int coor);
        void OnPieceExit (Vector2Int coor);
        IEnumerator GenerateGemVertical (List<Piece> cords);
        IEnumerator GenerateGemHorizontal (List<Piece> cords);
        IEnumerator Fall (List<Piece> cords, List<Piece> to);
        IEnumerator MovingPiece (Vector2Int from, Vector2Int to);
    }
}