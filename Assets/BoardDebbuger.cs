using TMPro;
using UnityEngine;

namespace Match3
{
    public class BoardDebbuger : MonoBehaviour
    {
        [SerializeField] Board board;
        [SerializeField] TextMeshProUGUI hover;
        // Start is called before the first frame update
        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        // Update is called once per frame
        void Update ()
        {
            hover.text = board.HoverPiece.x > 0 ? $"{board.HoverPiece.x} : {board.HoverPiece.y}" :
                "None";
        }
    }
}