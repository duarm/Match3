using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Match3
{
    public class Piece : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler
    {
        public int type;
        public Vector2Int coordinates;
        public event Action<Vector2Int> PointerClick;
        public event Action<Vector2Int> PointerEnter;
        public event Action<Vector2Int> PointerExit;
        public event Action<Vector2Int> PointerUp;

        public void OnPointerEnter (PointerEventData eventData)
        {
            PointerEnter.Invoke (coordinates);
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            PointerExit.Invoke (coordinates);
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            PointerClick.Invoke (coordinates);
        }

        public void OnPointerUp (PointerEventData eventData)
        {
            PointerUp.Invoke (coordinates);
        }
    }
}