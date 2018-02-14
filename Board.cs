using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OverlapData
{
    public bool IsOverlaping;
    public int AtPosition;

    public int OverlapperId;
    public int OverlappeeId;
    public bool SelfOverlap;

    public OverlapData()
    {
        IsOverlaping    = false;
        AtPosition      = -1;
        OverlapperId    = -1;
        OverlapperId    = -1;
    }

    public OverlapData(bool overlap)
    {
        IsOverlaping    = overlap;
        AtPosition      = -1;
        OverlapperId    = -1;
        OverlapperId    = -1;
    }

    public OverlapData(bool overlap, int position, int overlapper, int overlappee, bool self)
    {
        IsOverlaping    = overlap;
        AtPosition      = position;
        OverlapperId    = overlapper;
        OverlappeeId    = overlappee;
        SelfOverlap     = self;
    }
}

public class Board : MonoBehaviour
{
    [System.Serializable]
    class PieceData
    {
        public Piece piece;
        public int position;

        public PieceData(Piece p, int pos)
        {
            piece = p;
            position = pos;
        }
    }

    [SerializeField]
    CameraPerspective camPerspective;

    Camera cam;
    [SerializeField]
    LayerMask playerMask;

    private int player1Score = 0;
    private int player2Score = 0;
    
    // UI
    [SerializeField]
    Text player1ScoreText;
    [SerializeField]
    Text player2ScoreText;

    [SerializeField]
    Button rollButton;
    [SerializeField]
    Text rollText;

    
    [SerializeField]
    private PlayerType currentTurn = PlayerType.Player1;
    private bool finishedTurn = true;
    private bool boardSettled = true;
    private int roll = 0;

    [SerializeField]
    List<TileObject> player1Pathway = new List<TileObject>();
    [SerializeField]
    List<TileObject> player2Pathway = new List<TileObject>();

    [SerializeField]
    List<Piece> player1Pieces = new List<Piece>();
    [SerializeField]
    List<Piece> player2Pieces = new List<Piece>();

    // New tracking system
    // Used for tracking multiple game pieces per player
    [SerializeField]
    List<PieceData> player1Positions = new List<PieceData>();
    [SerializeField]
    List<PieceData> player2Positions = new List<PieceData>();
    
    [SerializeField]
    Transform player1OffBoard;
    [SerializeField]
    Transform player2OffBoard;

    // Use this for initialization
    void Start ()
    {
        cam = Camera.main;

        // Set all player pieces' positions to -1 (off the board)
        for (int i = 0; i < 7; i++)
        {
            player1Pieces[i].SetPieceID(i);
            player2Pieces[i].SetPieceID(i);

            player1Positions.Add(new PieceData(player1Pieces[i], -1));
            player2Positions.Add(new PieceData(player2Pieces[i], -1));
        }

        // Update both players' positions
        for (int i = 0; i < player1Positions.Count; i++)
        {
            StartCoroutine(UpdatePosition(0, i));
            StartCoroutine(UpdatePosition(1, i));
        }

        //print(currentTurn + "'s turn!");
    }

	
	// Update is called once per frame
	void Update ()
    {
        if(!finishedTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Piece selectedPiece = SelectPiece((int)currentTurn);

                if (selectedPiece)
                    AdvancePlayer((int)currentTurn, selectedPiece.GetPieceID(), roll);
            }
        }
    }

    void AdvancePlayer(int playerId, int pieceId, int moves)
    {
        // Player 1
        if (playerId == 0)
        {
            // If the player gets to score a point
            if ((player1Pathway.Count - player1Positions[pieceId].position) == moves)
                ScorePoint(playerId, pieceId);
            // Otherwise, the player is still somewhere on the board
            else if ((player1Pathway.Count - player1Positions[pieceId].position) > moves)
            {
                // Check for any possible overlap
                OverlapData overlapData = new OverlapData();
                CheckOverlap(playerId, pieceId, moves, value => overlapData = value);

                if (overlapData != null)
                {
                    //Debug.Log("self overlap = " + overlapData.SelfOverlap);
                    if (overlapData.SelfOverlap)
                    {
                        print("Can't move that piece! Move a different piece.");
                        return;
                    }
                }

                player1Positions[pieceId].position += moves;

                if(player1Positions[pieceId].position >= player1Pathway.Count)
                    ScorePoint(playerId, pieceId);

                bool onRosette = (player1Positions[pieceId].position == 3 ||
                                  player1Positions[pieceId].position == 7 ||
                                  player1Positions[pieceId].position == 13) &&
                                  moves > 0;
                
                NextTurn(onRosette);
            }
            
            StartCoroutine(UpdatePosition(playerId, pieceId));
        }
        // Player 2
        else
        {
            // If the player gets to score a point
            if ((player2Pathway.Count - player2Positions[pieceId].position) == moves)
                ScorePoint(playerId, pieceId);
            // Otherwise, the player is still somewhere on the board
            else if ((player2Pathway.Count - player2Positions[pieceId].position) > moves)
            {
                // Check for any possible overlap
                OverlapData overlapData = null;
                CheckOverlap(playerId, pieceId, moves, value => overlapData = value);

                if (overlapData != null)
                {
                    if (overlapData.SelfOverlap)
                    {
                        print("Can't move that piece! Move a different piece.");
                        return;
                    }
                }

                player2Positions[pieceId].position += moves;

                if (player2Positions[pieceId].position >= player2Pathway.Count)
                    ScorePoint(playerId, pieceId);

                bool onRosette = (player2Positions[pieceId].position == 3 ||
                                  player2Positions[pieceId].position == 7 ||
                                  player2Positions[pieceId].position == 13) &&
                                  moves > 0;

                NextTurn(onRosette);
            }
            
            StartCoroutine(UpdatePosition(playerId, pieceId));
        }
    }


    #region Basic Functions

    Piece SelectPiece(int playerId)
    {
        Piece selectedPlayer = null;

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 20, playerMask))
        {
            Piece temp = hit.transform.parent.GetComponent<Piece>();

            if ((int)temp.GetOwner() == playerId)
                selectedPlayer = hit.transform.parent.GetComponent<Piece>();
        }

        return selectedPlayer;
    }

    private void NextTurn(bool onRosette)
    {
        finishedTurn = true;

        if(!onRosette)
        {
            currentTurn = (currentTurn == PlayerType.Player1) ? PlayerType.Player2 : PlayerType.Player1;
            camPerspective.SwitchPerspective();
        }
    }

    /// <summary>
    /// Equivalent to rolling 4 tetrahedrons with two corners
    /// marked white (indicating an increase in total moves)
    /// and two non-white corners (no increase in total moves)
    /// </summary>
    /// <returns>
    /// totalMoves : int
    /// </returns>
    private int RollDice()
    {
        int totalMoves = 0;

        for (int i = 0; i < 4; i++)
            if (Random.Range(1, 5) % 2 == 0)
                totalMoves++;

        return totalMoves;
    }

    OverlapData IsOverlapping(int playerId, int pieceId, int moves)
    {
        /// If the piece's move count is 0,
        /// then don't worry about self overlap.
        /// Just return non-overlap data
        if (moves == 0)
            return new OverlapData(false);

        if(playerId == 0)
        {
            for (int i = 0; i < player1Positions.Count; i++)
            {
                if (player1Positions[pieceId].position + moves >= 4 && player1Positions[pieceId].position + moves <= 11)
                {
                    // Opponent overlap
                    if ((player1Positions[pieceId].position + moves) == player2Positions[i].position)
                    {
                        print("Opponent overlap, IsOverlapping(" + playerId + ", " + pieceId + ") = true");
                        return new OverlapData(true, (player1Positions[pieceId].position + moves), pieceId, i, false);
                    }
                }

                // Self overlap
                if(pieceId != i)
                {
                    if((player1Positions[pieceId].position + moves) == player1Positions[i].position)
                    {
                        print("Player 1 Self overlap with piece id " + i);
                        return new OverlapData(true, player1Positions[pieceId].position + moves, pieceId, i, true);
                    }
                }
            }
        }
        else
        {
            for (int j = 0; j < player2Positions.Count; j++)
            {
                if(player2Positions[pieceId].position + moves >= 4 && player2Positions[pieceId].position + moves <= 11)
                {
                    // Opponent overlap
                    if ((player2Positions[pieceId].position + moves) == player1Positions[j].position)
                    {
                        print("Opponent overlap, IsOverlapping(" + playerId + ", " + pieceId + ") = true");
                        return new OverlapData(true, (player2Positions[pieceId].position + moves), pieceId, j, false);
                    }
                }
                

                // Self overlap
                if (pieceId != j)
                {
                    if ((player2Positions[pieceId].position + moves) == player2Positions[j].position)
                    {
                        print("Player 2 Self overlap with piece id " + j);
                        return new OverlapData(true, (player2Positions[pieceId].position + moves), pieceId, j, true);
                    }
                }
            }
        }
        
        return new OverlapData(false);
    }

    void CheckOverlap(int playerId, int pieceId, int moves, System.Action<OverlapData> result)
    {
        // Check if any pieces have the same position and
        // if the the other player is NOT on a rosette tile within the neutral zone
        // If so, restart the other player
        // If the other player IS on a rossette tile within the neutral zone,
        // move the advancing player one tile back from the player on the rosette tile

        OverlapData overlapData = IsOverlapping(playerId, pieceId, moves);

        result(overlapData);

        if (overlapData.IsOverlaping)
        {
            int positionOfOverlap = overlapData.AtPosition;
            print("positionOfOverlap = " + positionOfOverlap);

            if (playerId == 0)
            {
                if (!overlapData.SelfOverlap)
                {
                    // Conflict at the rosette tile
                    if (positionOfOverlap == 7)
                    {
                        // If opponent piece on rosette
                        if (FindIdByPosition(1, 7) != -1)
                        {
                            // If opponent on tile behind rosette as well,
                            // remove that piece and put this piece there
                            if (FindIdByPosition(1, 6) != -1)
                                player2Positions[FindIdByPosition(1, 6)].position = -1;

                            player1Positions[pieceId].position = 6;
                        }
                        else
                        {
                            player2Positions[FindIdByPosition(1, positionOfOverlap)].position = -1;
                            player1Positions[pieceId].position = positionOfOverlap;
                        }
                    }
                    else
                    {
                        int opponentId = FindIdByPosition(1, positionOfOverlap);

                        // If there is an opponent piece on positionOfOverlap
                        if (opponentId != -1)
                        {
                            player2Positions[opponentId].position = -1;
                            player1Positions[pieceId].position = positionOfOverlap;
                        }
                    }
                }
            }
            else
            {
                if (!overlapData.SelfOverlap)
                {
                    // Conflict at the rosette tile
                    if (positionOfOverlap == 7)
                    {
                        // If opponent piece at rosette
                        if (FindIdByPosition(0, 7) != -1)
                        {
                            // If opponent on tile behind rosette as well,
                            // remove that piece and put this piece there
                            if (FindIdByPosition(0, 6) != -1)
                                player1Positions[FindIdByPosition(0, 6)].position = -1;

                            player2Positions[pieceId].position = 6;
                        }
                        else
                        {
                            player1Positions[FindIdByPosition(0, positionOfOverlap)].position = -1;
                            player2Positions[pieceId].position = positionOfOverlap;
                        }
                    }
                    else
                    {
                        int opponentId = FindIdByPosition(0, positionOfOverlap);

                        // If there is an opponent piece on positionOfOverlap
                        if (opponentId != -1)
                        {
                            player1Positions[opponentId].position = -1;
                            player2Positions[pieceId].position = positionOfOverlap;
                        }
                    }
                }
            }

            // Update both players' positions
            for (int i = 0; i < player1Positions.Count; i++)
            {
                StartCoroutine(UpdatePosition(0, i));
                StartCoroutine(UpdatePosition(1, i));
            }
        }
    }

    IEnumerator UpdatePosition(int playerId, int pieceId)
    {
        Vector3 dest = Vector3.zero;

        if(playerId == 0)
        {
            if (player1Positions[pieceId].position == -1)
                dest = player1OffBoard.position + new Vector3(0, 0, pieceId);
            else if (player1Positions[pieceId].position == 14)
                dest = Vector3.up * 5;
            else
                dest = player1Pathway[player1Positions[pieceId].position].transform.position;

            player1Positions[pieceId].piece.MoveTo(dest);
        }
        else
        {
            if (player2Positions[pieceId].position == -1)
                dest = player2OffBoard.position + new Vector3(0, 0, pieceId);
            else if (player2Positions[pieceId].position == 14)
                dest = Vector3.up * 5;
            else
                dest = player2Pathway[player2Positions[pieceId].position].transform.position;

            player2Positions[pieceId].piece.MoveTo(dest);
        }
        yield return null;
        
        rollText.text = "ROLL";
        rollButton.interactable = boardSettled = true;
    }

    Piece FindPlayerByID(int playerId, int id)
    {
        //Player thePlayer = null;
        //int index = 0;

        //while (thePlayer == null)
        //{
        //    if (playerId == 0)
        //    {
        //        if (player1Positions[index].piece.GetID() == id)
        //            thePlayer = player1Positions[index].piece;
        //    }
        //    else
        //    {
        //        if (player2Positions[index].piece.GetID() == id)
        //            thePlayer = player2Positions[index].piece;
        //    }
        //}

        return (playerId == 0)? player1Positions[id].piece: player2Positions[id].piece;
    }

    int FindIdByPosition(int playerId, int pos)
    {
        if(playerId == 0)
        {
            for (int i = 0; i < player1Positions.Count; i++)
                if(player1Positions[i].position == pos)
                    return i;
        }
        else
        {
            for (int i = 0; i < player2Positions.Count; i++)
                if (player2Positions[i].position == pos)
                    return i;
        }

        Debug.Log("Could not find id at playerId " + playerId + " position " + pos + "\nReturning -1");
        return -1;
    }

    private void ScorePoint(int playerId, int pieceId)
    {
        if(playerId == 0)
        {
            player1Score++;
            player1ScoreText.text = "Player 1: " + player1Score;
            player1Positions[pieceId].position = -1;

            print("Player 1 has " + player1Score + " points");
        }
        else
        {
            player2Score++;
            player2ScoreText.text = "Player 2: " + player2Score;
            player2Positions[pieceId].position = -1;

            print("Player 2 has " + player2Score + " points");
        }
    }

    public void RollButton()
    {
        if (finishedTurn && boardSettled)
        {
            finishedTurn = boardSettled = rollButton.interactable = false;

            // Roll the dice
            roll = RollDice();
            rollText.text = roll.ToString() + ((roll != 1) ? " moves" : " move");
        }
    }

    #endregion

}
