using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Match3
{
    public class GoalUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI goalText;
        [HideInInspector]
        [SerializeField] Board board;

        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        private void Awake ()
        {
            board.OnGoalChanged += UpdateGoal;
        }

        private void UpdateGoal()
        {
            goalText.text = board.RoundGoal.ToString();
        }
    }
}