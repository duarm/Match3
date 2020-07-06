using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Match3
{
    public class PointsUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI pointsText;
        [HideInInspector]
        [SerializeField] Board board;

        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        private void Awake ()
        {
            board.OnPointsChanged += UpdatePoints;
        }

        private void UpdatePoints ()
        {
            pointsText.text = board.Points.ToString ();
        }
    }
}