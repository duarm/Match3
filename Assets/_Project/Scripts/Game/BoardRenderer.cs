using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Match3
{
    // UI must display the game state
    public class BoardRenderer : MonoBehaviour, IBoardRenderer
    {
        [Tooltip ("The speed in which the pieces swap when they move.")]
        [SerializeField] public float pieceSwappingSpeed = 0.3f;
        [System.Serializable]
        public class PieceSprite
        {
            [ShowAssetPreview]
            public Sprite sprite;
        }

        [ShowAssetPreview]
        public Sprite noPiece;

        [SerializeField] AudioClip swapSound;
        [SerializeField] AudioClip selectSound;

        [SerializeField] Color mouseHoverPieceColor;
        [SerializeField] Color mouseClickPieceColor;

        [SerializeField] Board board;
        [SerializeField] PieceSprite[] sprites;
        SpriteRenderer[, ] renderers;
        float distanceBetweenTiles;

        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        void IBoardRenderer.InitializeBoard (Piece[, ] pieces)
        {
            var size = board.Size;
            if (renderers == null)
            {
                renderers = new SpriteRenderer[size.x, size.y];
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        var renderer = board.transform.GetChild ((y * size.x) + x)
                            .GetComponent<SpriteRenderer> ();
                        renderers[x, y] = renderer;
                        renderer.sprite = sprites[pieces[x, y].type].sprite;
                    }
                }
                distanceBetweenTiles = Mathf.Abs ((renderers[0, 0].transform.localPosition - renderers[0, 1].transform.position).y);
            }
            else
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        GetRenderer (x, y).sprite = sprites[pieces[x, y].type].sprite;
                    }
                }
            }

        }

        void IBoardRenderer.PiecesChanged (List<Piece> pieces)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                var x = piece.coordinates.x;
                var y = piece.coordinates.y;
                if (piece.type != -1)
                    GetRenderer (x, y).sprite = sprites[piece.type].sprite;
                else
                    GetRenderer (x, y).sprite = null;
            }
        }

        public void OnPieceClicked (Vector2Int coor)
        {
            AudioPlayer.PlaySFX (selectSound);
            GetRenderer (coor.x, coor.y).color = mouseClickPieceColor;
        }

        public void OnPieceSwap (Vector2Int coor)
        {
            AudioPlayer.PlaySFX (swapSound);
        }

        public void OnPieceUp (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = Color.white;
        }

        public void OnPieceEnter (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = mouseHoverPieceColor;
        }

        public void OnPieceExit (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = Color.white;
        }

        public IEnumerator MovingPiece (Vector2Int from, Vector2Int to)
        {
            var fromPiece = GetRenderer (from.x, from.y);
            var toPiece = GetRenderer (to.x, to.y);

            fromPiece.transform.DOMove (toPiece.transform.localPosition, pieceSwappingSpeed)
                .SetEase (Ease.InOutQuad);
            yield return toPiece.transform.DOMove (fromPiece.transform.localPosition, pieceSwappingSpeed)
                .SetEase (Ease.InOutQuad).WaitForCompletion ();
            //after the movement, we swap the physical pieces back
            var oldToPos = fromPiece.transform.localPosition;
            fromPiece.transform.localPosition = toPiece.transform.localPosition;
            toPiece.transform.localPosition = oldToPos;
        }

        public IEnumerator FallBy (List<Piece> pieces, int by)
        {
            var finished = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                //make the piece falls to match position
                var piece = GetRenderer (pieces[i].coordinates.x, pieces[i].coordinates.y);
                var toRenderer = GetRenderer (pieces[i].coordinates.x, pieces[i].coordinates.y - by);
                var toPos = toRenderer.transform.localPosition;
                var initialPos = piece.transform.localPosition;
                if (i == pieces.Count - 1) // we wait for the last one, every piece is moving in parallel
                    yield return piece.transform.DOMove (toPos, 0.2f)
                        .SetEase (Ease.InOutQuad)
                        .OnComplete (() =>
                        {
                            toRenderer.sprite = null;
                            piece.transform.localPosition = initialPos;
                            finished++;
                        })
                        .WaitForCompletion ();
                else
                    piece.transform.DOMove (toPos, 0.2f)
                    .SetEase (Ease.InOutQuad)
                    .OnComplete (() =>
                    {
                        toRenderer.sprite = null;
                        piece.transform.localPosition = initialPos;
                        finished++;
                    });
            }
        }

        public IEnumerator GenerateGem (List<Piece> pieces)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                var renderer = GetRenderer (pieces[i].coordinates.x, pieces[i].coordinates.y);
                var transf = renderer.transform;
                var initialPos = transf.localPosition;
                renderer.sprite = sprites[pieces[i].type].sprite;
                transf.localPosition = new Vector3 (transf.localPosition.x, transf.localPosition.y + (pieces.Count * distanceBetweenTiles));
                if (i == pieces.Count - 1) // we wait for the last one, every piece is moving in parallel
                    yield return transf.DOMove (initialPos, 0.2f)
                        .SetEase (Ease.InOutQuad)
                        .WaitForCompletion ();
                else
                    transf.DOMove (initialPos, 0.2f)
                    .SetEase (Ease.InOutQuad);
            }
        }

        SpriteRenderer GetRenderer (int x, int y)
        {
            return renderers[x, y];
        }

        public IEnumerator PostFall (List<Piece> matches)
        {
            yield return null;
        }

        public IEnumerator Match (List<Piece> pieces)
        {
            foreach (var match in pieces)
            {
                GetRenderer (match.coordinates.x, match.coordinates.y).sprite = noPiece;
            }
            yield return null;
        }
    }
}