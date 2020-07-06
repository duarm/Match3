using TMPro;
using UnityEngine;

namespace Match3
{
    public class CounterUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI counterText;
        [HideInInspector]
        [SerializeField] Board board;

        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        void Update ()
        {
            if (!board.GameRunning)
                return;

            var minutes = Mathf.Floor (board.Timer / 60).ToString ("00");
            var seconds = (board.Timer % 60).ToString ("00");
            counterText.text = $"{minutes}:{seconds}";
        }
    }
}