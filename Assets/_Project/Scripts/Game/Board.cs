using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Match3
{
    // controls the Game State
    public class Board : MonoBehaviour
    {
        [Header ("Piece")]
        [SerializeField] GameObject piecePrefab;
        [SerializeField] Vector3 pivot;
        [SerializeField] Vector2 spacing = Vector2.one;
        [SerializeField] float scale = 1;
        [Header ("Board")]
        [SerializeField] int pieceCount = 7;
        [SerializeField] Vector2Int size = new Vector2Int (6, 6);
        [Header ("Game")]
        [SerializeField] int pointsPerPiece = 1000;
        [SerializeField] int firstPointsGoal = 15000;
        [Tooltip ("The next goal is determined by the current point count + a percentage of the current point count.")]
        [Range (0.1f, 1)]
        [SerializeField] float pointPercentage = 0.5f;
        [Tooltip ("The duration in seconds of the round")]
        [SerializeField] float roundTimeInSeconds = 120f;
        [SerializeField] float rushRoundTimeInSeconds = 10f;

        public IBoardRenderer renderer;

        readonly Vector2Int noPiece = new Vector2Int (-1, -1);
        Vector2Int clickedPiece = new Vector2Int (-1, -1);
        Vector2Int hoverPiece = new Vector2Int (-1, -1);
        Piece[, ] pieces;
        Coroutine updateRoutine;
        int round;
        bool rush;
        bool updatingBoard = false;

        public int RoundGoal { get; private set; }
        public int Points { get; private set; }
        public float Timer { get; private set; }
        public bool GameRunning { get; private set; }
        public Vector2Int Size => size;
        public Vector2Int HoverPiece => hoverPiece;
        public Action OnPointsChanged;
        public Action OnGoalChanged;

        public const string GameOver = "GameOver";

        void Start ()
        {
            renderer = GetComponent<IBoardRenderer> ();
            GenerateBoard (Size);
            renderer.InitializeBoard (pieces);
        }

        public void StartGame (bool rush)
        {
            this.rush = rush;
            Timer = rush ? rushRoundTimeInSeconds : roundTimeInSeconds;
            RoundGoal = firstPointsGoal;
            Points = 0;
            OnGoalChanged?.Invoke ();
            GameRunning = true;

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    pieces[x, y].type = UnityEngine.Random.Range (0, pieceCount);
                }
            }

            renderer.InitializeBoard (pieces);
            updateRoutine = StartCoroutine (UpdateBoard ());
        }

        private void Update ()
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                if (Points < RoundGoal)
                {
                    GameRunning = false;
                    EventManager.TriggerEvent (GameOver);
                }
                else
                {
                    RoundGoal += Mathf.CeilToInt (Points * pointPercentage);
                    OnGoalChanged?.Invoke ();
                    Timer = rush? rushRoundTimeInSeconds : roundTimeInSeconds;
                }
            }
        }

        public void GenerateBoard (Vector2Int size)
        {
            pieces = new Piece[size.x, size.y];
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    GameObject go = Instantiate (piecePrefab);
                    go.name = $"x: {x} | y: {y}";
                    go.transform.parent = transform;
                    go.transform.localScale = new Vector2 (scale, scale);
                    go.transform.localPosition = pivot + new Vector3 (x * spacing.x, y * spacing.y, 0);
                    var piece = go.GetComponent<Piece> ();
                    piece.PointerClick += OnMouseClickPiece;
                    piece.PointerUp += OnMouseUpPiece;
                    piece.PointerEnter += OnMouseEnterPiece;
                    piece.PointerExit += OnMouseExitPiece;
                    piece.coordinates = new Vector2Int (x, y);
                    pieces[x, y] = piece;
                }
            }
        }

        IEnumerator MovePiece (Vector2Int to)
        {
            updatingBoard = true;
            var fromPiece = pieces[clickedPiece.x, clickedPiece.y];
            var toPiece = pieces[to.x, to.y];
            var toType = toPiece.type;
            //swap the pieces
            toPiece.type = fromPiece.type;
            fromPiece.type = toType;
            ResetMovement ();
            //move the pieces visually
            yield return renderer.MovingPiece (fromPiece.coordinates, to);

            //Debug.Log ($"[MOVING] From {clickedPiece.x}:{clickedPiece.y} t:{pieces[clickedPiece.x,clickedPiece.y].type} to {to.x}:{to.y} t:{pieces[to.x,to.y].type}");
            renderer.PiecesChanged (new List<Piece> { toPiece, fromPiece });

            updateRoutine = StartCoroutine (UpdateBoard ());
        }

        IEnumerator UpdateBoard ()
        {
            // search pattern
            //1 . # . # . #
            //2 # . # . # .
            //3 . # . # . #
            //4 # . # . # .
            //5 . # . # . #
            //6 # . # . # .

            var anyMatch = false;
            do
            {
                if (!GameRunning)
                    StopCoroutine (updateRoutine);

                anyMatch = false;
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = y % 2; x < size.x; x += 2)
                    {
                        var piece = pieces[x, y];

                        //Vertical Matches
                        var vMatches = new List<Piece> (6);
                        vMatches.Add (piece);
                        SearchForMatches (piece, new Vector2Int (0, -1), ref vMatches);
                        SearchForMatches (piece, new Vector2Int (0, 1), ref vMatches);

                        vMatches = vMatches.OrderBy (f => f.coordinates.y).ToList ();
                        //matched 3 or more
                        if (vMatches.Count >= 3)
                        {
                            anyMatch = true;
                            Points += pointsPerPiece * vMatches.Count;
                            OnPointsChanged?.Invoke ();

                            var topMatch = vMatches[vMatches.Count - 1];
                            if (PieceAt (topMatch.coordinates.x, topMatch.coordinates.y + 1))
                            {
                                var pieceBuffer = new List<Piece> (6);
                                GetAbove (topMatch.coordinates, ref pieceBuffer);
                                var piecesChanged = new List<Piece> (pieceBuffer.Count);
                                for (int i = 0; i < pieceBuffer.Count; i++)
                                {
                                    var newPos = pieceBuffer[i].coordinates;
                                    newPos.y -= vMatches.Count;
                                    var pieceToFall = GetPiece (pieceBuffer[i].coordinates.x, pieceBuffer[i].coordinates.y);
                                    var fallToPiece = GetPiece (newPos.x, newPos.y);
                                    fallToPiece.type = pieceToFall.type;
                                    piecesChanged.Add (fallToPiece);
                                }

                                yield return renderer.Match (vMatches);
                                yield return renderer.FallBy (pieceBuffer, vMatches.Count);
                                renderer.PostFall (vMatches);
                                renderer.PiecesChanged (piecesChanged);

                                for (int i = 0; i < pieceBuffer.Count; i++)
                                    GetPiece (pieceBuffer[i].coordinates.x, pieceBuffer[i].coordinates.y - vMatches.Count).type = pieceBuffer[i].type;

                                int emptyPieces = Mathf.Abs (vMatches.Count - pieceBuffer.Count);
                                for (int i = vMatches.Count - 1; i > vMatches.Count - emptyPieces - 1; i--)
                                    pieceBuffer.Add (vMatches[i]);

                                for (int i = 0; i < pieceBuffer.Count; i++)
                                    pieceBuffer[i].type = UnityEngine.Random.Range (0, pieceCount);
                                yield return renderer.GenerateGem (pieceBuffer);
                            }
                            else
                            {
                                for (int i = 0; i < vMatches.Count; i++)
                                    vMatches[i].type = UnityEngine.Random.Range (0, pieceCount);

                                yield return renderer.GenerateGem (vMatches);
                            }

                            continue;
                        }

                        //Horizontal Matches
                        var hMatches = new List<Piece> (6);
                        hMatches.Add (piece);
                        SearchForMatches (piece, new Vector2Int (-1, 0), ref hMatches);
                        SearchForMatches (piece, new Vector2Int (1, 0), ref hMatches);

                        if (hMatches.Count >= 3)
                        {
                            anyMatch = true;
                            Points += pointsPerPiece * hMatches.Count;
                            OnPointsChanged?.Invoke ();
                            //Debug.Break ();
                            //yield return new WaitForSeconds (0.07f);

                            var pieceBuffer = new List<Piece> (6);
                            var piecesChanged = new List<Piece> ();
                            for (int i = 0; i < hMatches.Count; i++)
                            {
                                GetAbove (hMatches[i].coordinates, ref pieceBuffer);
                                foreach (var p in pieceBuffer)
                                    piecesChanged.Add (p);

                                if (i == hMatches.Count - 1)
                                {
                                    yield return renderer.Match (hMatches);
                                    yield return renderer.FallBy (pieceBuffer, 1);
                                    renderer.PiecesChanged (piecesChanged);
                                }
                                else
                                    StartCoroutine (renderer.FallBy (pieceBuffer, 1));

                                pieceBuffer.Clear ();
                            }

                            for (int t = 0; t < piecesChanged.Count; t++)
                                GetPiece (piecesChanged[t].coordinates.x, piecesChanged[t].coordinates.y - 1).type = piecesChanged[t].type;

                            for (int t = 0; t < hMatches.Count; t++)
                                piecesChanged.Add (hMatches[t]);

                            renderer.PiecesChanged (piecesChanged);

                            pieceBuffer.Clear ();
                            for (int t = 0; t < hMatches.Count; t++)
                            {
                                var generatedPiece = GetPiece (hMatches[t].coordinates.x, size.y - 1);
                                generatedPiece.type = UnityEngine.Random.Range (0, pieceCount);
                                pieceBuffer.Add (generatedPiece);
                            }

                            yield return renderer.GenerateGem (pieceBuffer);
                        }
                    }
                }
            } while (anyMatch);

            updatingBoard = false;
        }

        /// <summary>
        /// returns all the pieces above the given one
        /// </summary>
        void GetAbove (Vector2Int cor, ref List<Piece> pieces)
        {
            if (TryGetPiece (cor.x, cor.y + 1, out var piece))
            {
                pieces.Add (piece);
                GetAbove (piece.coordinates, ref pieces);
            }
        }

        void SearchForMatches (Piece piece, Vector2Int direction, ref List<Piece> matches)
        {
            Piece nextPiece;
            if (TryGetPiece (piece.coordinates.x + direction.x, piece.coordinates.y + direction.y, out nextPiece))
            {
                if (piece.type == nextPiece.type)
                {
                    matches.Add (nextPiece);
                    SearchForMatches (nextPiece, direction, ref matches);
                }
            }
        }

        /// <summary>
        /// Returns the piece at the given position, otherwise null if out of bounds
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="piece"></param>
        /// <returns>True if there was a piece at the given position</returns>
        bool TryGetPiece (int x, int y, out Piece piece)
        {
            piece = null;
            if (x < 0 || y < 0 || x >= size.x || y >= size.y)
                return false;
            piece = pieces[x, y];
            return true;
        }

        Piece GetPiece (int x, int y)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y)
                return null;
            return pieces[x, y];
        }

        bool PieceAt (int x, int y)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y)
                return false;
            return true;
        }

        //events
        void OnMouseClickPiece (Vector2Int coor)
        {
            if (updatingBoard || !GameRunning)
                return;

            if (clickedPiece == noPiece)
                clickedPiece = coor;
            else if (coor.AdjacentOf (clickedPiece))
            {
                StartCoroutine (MovePiece (coor));
                renderer.OnPieceSwap (coor);
            }
            else
                clickedPiece = noPiece;

            renderer.OnPieceClicked (coor);
        }

        void ResetMovement ()
        {
            clickedPiece = noPiece;
        }

        void OnMouseUpPiece (Vector2Int coor)
        {
            if (!GameRunning)
                return;
            renderer.OnPieceUp (coor);
        }

        void OnMouseEnterPiece (Vector2Int coor)
        {
            if (!GameRunning)
                return;
            renderer.OnPieceEnter (coor);
            hoverPiece = coor;
        }
        void OnMouseExitPiece (Vector2Int coor)
        {
            if (!GameRunning)
                return;
            renderer.OnPieceExit (coor);
            hoverPiece = noPiece;
        }
    }
}