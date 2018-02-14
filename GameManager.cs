using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerType
{
    Player1,
    Player2,
    Neutral
}

/// <summary>
/// Tracks the current state of a turn.
/// 
/// ROLLING refers to the player needing to "roll" the dice and getting a result in terms of moves (0, 1, 2, etc. moves).
/// 
/// SELECTING refers to the player needing to select one of thier Pieces to move.
///
/// MOVING refers to the selected Piece moving and waiting for it to finish the move.
/// 
/// After these three steps are completed,
/// the game will finish the turn and repeat.
/// </summary>
public enum TurnStates
{
    ROLLING,
    SELECTING,
    MOVING
}

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    AudioSource audioSource;
    CameraPerspective camPerspective;
    [SerializeField]
    LayerMask pieceLayerMask;

    const int NUM_TILES = 20;
    const int CENTER_TILES = 8;
    const float MOVE_WAIT_TIME = 1.5f;

    private int player1Points = 0;
    private int player2Points = 0;

    public GameObject piecePrefab;
    public Transform player1StoneStorage;
    public Transform player2StoneStorage;

    [Header("Tracks")]
    [SerializeField]
    Track player1Track = null;
    [SerializeField]
    Track player2Track = null;

    Track opponentTrack;

    [Space]
    public Tile[] centerTiles = new Tile[CENTER_TILES]
    {
        new Tile(PlayerType.Neutral, 4),
        new Tile(PlayerType.Neutral, 5),
        new Tile(PlayerType.Neutral, 6),
        new Tile(PlayerType.Neutral, 7),
        new Tile(PlayerType.Neutral, 8),
        new Tile(PlayerType.Neutral, 9),
        new Tile(PlayerType.Neutral, 10),
        new Tile(PlayerType.Neutral, 11)
    };
    
    private PlayerType currentTurn = PlayerType.Player1;
    private TurnStates currentTurnState = TurnStates.ROLLING;

    // The number of moves in a turn
    int roll = 0;
    // Flag for if a player landed on a rosette in a turn
    bool landedOnRosette = false;

    // UI
    [Header("UI")]
    [SerializeField]
    GameObject RollUI;
    [SerializeField]
    Text rollText;
    [SerializeField]
    Button rollButton;
    [SerializeField]
    Button passButton;
    [SerializeField]
    Text playerTurnText;

    // Audio
    [Header("Audio")]
    [SerializeField]
    AudioClip rollSound;
    [SerializeField]
    AudioClip attackSound;

    private void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        camPerspective = FindObjectOfType<CameraPerspective>();

        player1Track = new Track(this, PlayerType.Player1);
        player2Track = new Track(this, PlayerType.Player2);

        UpdateTurnState(TurnStates.ROLLING);
    }

    // Update is called once per frame
    void Update()
    {
        PieceSelection();
    }

    private void UpdateTurnState(TurnStates newTurnState)
    {
        currentTurnState = newTurnState;

        switch (currentTurnState)
        {
            case TurnStates.ROLLING:
                rollButton.interactable = true;
                rollText.text = string.Empty;
                RollUI.SetActive(true);

                break;

            case TurnStates.SELECTING:
                // Now the game should allow the player to select one of their Pieces to move
                // by calling PieceSelection from Update()
                // Maybe show some UI here

                break;

            case TurnStates.MOVING:
                RollUI.SetActive(false);
                StartCoroutine(WaitForMove());
                break;

            default:
                break;
        }
    }
    
    private void AdvancePiece(Piece piece)
    {
        bool reselect = false;

        switch (currentTurn)
        {
            case PlayerType.Player1:
                player1Track.AdvancePiece(piece, roll, out landedOnRosette, out reselect);

                break;
            case PlayerType.Player2:
                player2Track.AdvancePiece(piece, roll, out landedOnRosette, out reselect);

                break;
            default:
                break;
        }

        // Rosette UI here
        //if (landedOnRosette)
        //    Debug.Log("You landed on a rosette!");

        if (reselect)
            UpdateTurnState(TurnStates.SELECTING);
        else
            UpdateTurnState(TurnStates.MOVING);
    }

    #region Helper Functions

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

    public void GetRoll()
    {
        rollButton.interactable = false;
        StartCoroutine(RollAnimation());
    }

    private IEnumerator RollAnimation()
    {
        float factor = 0;
        float delay = DiceAlgorithm(factor);
        int previousNumber = 0;

        roll = RollDice();

        while (factor < 0.8f)
        {
            while (roll == previousNumber)
                roll = RollDice();

            previousNumber = roll;
            rollText.text = roll.ToString();
            audioSource.PlayOneShot(rollSound);

            factor += Time.deltaTime * 5f;
            delay = DiceAlgorithm(factor);
            yield return new WaitForSeconds(delay);
        }

        // TODO: Make UI?

        // FOR TESTING
        //if (currentTurn == PlayerType.Player1)
        //    roll = 5;
        //else
        //    roll = 5;
        // END TESTING

        if (roll != 0)
        {
            // Update currentTurnState to SELECTING
            UpdateTurnState(TurnStates.SELECTING);
        }
        else
        {
            // Finish this player's turn and start other player's turn
            landedOnRosette = false;
            NextTurn();
        }
    }
    
    private void PieceSelection()
    {
        if (currentTurnState == TurnStates.SELECTING)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Hovering over a Piece
            if (Physics.Raycast(ray, out hit, 25, pieceLayerMask))
            {
                Piece hitPiece = hit.transform.parent.parent.GetComponent<Piece>();

                // If we hit a Piece AND
                // if this Piece has the correct owner
                if (hitPiece && hitPiece.GetOwner() == currentTurn)
                {
                    // Highlight/reveal this Piece's destination Tile
                    PreviewDestinationTile(hitPiece);


                    
                    // Clicking on a Piece
                    if (Input.GetMouseButtonDown(0))
                    {
                        // If we hit a Piece AND
                        // if this Piece has the correct owner
                        if (hitPiece && hitPiece.GetOwner() == currentTurn)
                        {
                            AdvancePiece(hitPiece);
                        }
                    }
                }
            }
        }
    }

    void PreviewDestinationTile(Piece piece)
    {
        // Retreive the anticipated Tile and give it some effect (highlight/glow)
        // to signify that this would be the destination of this Piece
        switch (currentTurn)
        {
            case PlayerType.Player1:
                TilePreviewData player1tilePreviewData = player1Track.GetPreviewTile(piece, roll);

                switch (player1tilePreviewData.status)
                {
                    case TilePreviewData.PreviewTileStatus.TILE_EXISTS:
                        player1tilePreviewData.tile.Reveal();
                        break;
                    case TilePreviewData.PreviewTileStatus.SCORING_TILE:
                        print("Reveal " + currentTurn + " \"scoring\" tile");
                        break;
                    case TilePreviewData.PreviewTileStatus.NO_TILE:
                        print("No tile to reveal.");
                        break;
                    default:
                        break;
                }

                break;
            case PlayerType.Player2:
                TilePreviewData player2tilePreviewData = player2Track.GetPreviewTile(piece, roll);

                switch (player2tilePreviewData.status)
                {
                    case TilePreviewData.PreviewTileStatus.TILE_EXISTS:
                        player2tilePreviewData.tile.Reveal();
                        break;
                    case TilePreviewData.PreviewTileStatus.SCORING_TILE:
                        print("Reveal " + currentTurn + " \"scoring\" tile");
                        break;
                    case TilePreviewData.PreviewTileStatus.NO_TILE:
                        print("No tile to reveal.");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    IEnumerator WaitForMove()
    {
        yield return new WaitForSeconds(MOVE_WAIT_TIME);
        NextTurn();
    }

    public void SetOpponentTrack(Track opponentTrack)
    {
        this.opponentTrack = opponentTrack;
    }

    public void MoveQueue(Piece piece, Queue<Tile> tileQueue, int occupantId, bool scored)
    {
        StartCoroutine(MoveQueueCoroutine(piece, tileQueue.ToArray(), occupantId, scored));
    }

    private IEnumerator MoveQueueCoroutine(Piece piece, Tile[] tiles, int occupantId, bool scored)
    {
        float travelTime = 1f;

        foreach (Tile t in tiles)
        {
            piece.MoveTo(t);

            Tile targetTile = tiles[tiles.Length-1];

            // If we are going to be overtaking a Tile
            if (opponentTrack != null)
            {
                // If we are at the Tile before the occupied Tile
                if ((tiles.Length == 1) || t == tiles[tiles.Length-1])
                {
                    piece.FaceTile(targetTile);
                    piece.Attack();

                    // Player attack sound
                    audioSource.PlayOneShot(attackSound);

                    opponentTrack.BumpPiece(occupantId);
                    opponentTrack = null;
                    yield return new WaitForSeconds(3.5f);

                    piece.MoveTo(t);
                }
            }

            travelTime = Vector3.Distance(piece.transform.position, t.GetObjectPosition()) * 0.3f;
            yield return new WaitForSeconds(travelTime);
        }

        //if(scored)
        //{
        //    // Set landedOnRosette to false because the last Tile on the board
        //    // is a rosette and gets recorded. In turn, this allows the scoring player
        //    // to roll again, which isn't allowed.
        //    landedOnRosette = false;
        //    NextTurn();
        //}
    }

    private void NextTurn()
    {
        if(!landedOnRosette)
        {
            currentTurn = (currentTurn == PlayerType.Player1) ? PlayerType.Player2 : PlayerType.Player1;
            UpdatePlayerTurnText();
            StartCoroutine(DisablePassButton());
            camPerspective.SwitchPerspective();
        }

        UpdateTurnState(TurnStates.ROLLING);
    }

    public void Pass()
    {
        currentTurn = (currentTurn == PlayerType.Player1) ? PlayerType.Player2 : PlayerType.Player1;
        UpdatePlayerTurnText();
        camPerspective.SwitchPerspective();
        StartCoroutine(DisablePassButton());

        UpdateTurnState(TurnStates.ROLLING);
    }

    IEnumerator DisablePassButton()
    {
        passButton.interactable = false;
        yield return new WaitForSeconds(1.5f);
        passButton.interactable = true;
    }

    private void UpdatePlayerTurnText()
    {
        playerTurnText.text = (currentTurn == PlayerType.Player1) ? "Player One" : "Player Two";
    }

    float DiceAlgorithm(float factor)
    {
        return (-(1 / (factor - 1f)) * 0.05f + 0.025f) / 1.25f;
    }
    
    public GameObject CreatePiece(Vector3 position, Quaternion identity)
    {
        return Instantiate(piecePrefab, position, identity);
    }

    public void ScorePoint(PlayerType owner)
    {
        switch (owner)
        {
            case PlayerType.Player1:
                player1Points++;

                if (player1Points >= 7)
                {
                    print("Player 1 Wins!");
                }

                break;
            case PlayerType.Player2:
                player2Points++;

                if (player2Points >= 7)
                {
                    print("Player 2 Wins!");
                }

                break;
            default:
                break;
        }
        
        // Scoring UI

    }

    public Track GetOpponentTrack(PlayerType owner)
    {
        return (owner == PlayerType.Player1) ? player2Track : player1Track;
    }


    #endregion


}



