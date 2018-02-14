using System.Collections.Generic;
using UnityEngine;

public class TilePreviewData
{
    public enum PreviewTileStatus
    {
        TILE_EXISTS,
        SCORING_TILE,
        NO_TILE
    }

    // Contructor
    public TilePreviewData(Tile tile, PreviewTileStatus status)
    {
        this.tile = tile;
        this.status = status;
    }

    // Default constructor
    public TilePreviewData()
    {
        tile = null;
        status = PreviewTileStatus.NO_TILE;
    }

    // Attributes
    public Tile tile;
    public PreviewTileStatus status;
}

/// When crating a new Track, 7 Pieces will be generated and stored within a PieceManager
/// composed within this Track object. These Pieces will be managed in regards to their owner,
/// locations, and operations such as moving on the game board.
[System.Serializable]
public class Track
{
    [HideInInspector]
    public GameManager gameManager;

    const int PIECE_COUNT = 7;
    const int TRACK_SIZE = 14;

    private PlayerType owner;
    private Transform stoneStorage;

    [HideInInspector]
    PieceManager pieceManager = new PieceManager();

    [SerializeField]
    Tile[] tiles = new Tile[TRACK_SIZE];

    private Tile finalDestinationTile;


    // Constructor
    public Track(GameManager gm, PlayerType owner)
    {
        gameManager = gm;
        this.owner = owner;

        // Create the Piece objects
        for (int i = 0; i < PIECE_COUNT; i++)
        {
            // Determine the proper storage location based on owner
            // Then create a new Piece at that storage location
            stoneStorage = (owner == PlayerType.Player1) ? gameManager.player1StoneStorage : gameManager.player2StoneStorage;
            Piece newPiece = gameManager.CreatePiece(stoneStorage.position, Quaternion.identity).GetComponent<Piece>();

            // GameObject stuff
            newPiece.gameObject.name = owner.ToString() + " Piece " + i;
            newPiece.transform.SetParent(stoneStorage);

            // Piece initializations
            newPiece.Initialize(i, owner);

            // Adding this Piece to the PieceManager
            pieceManager.AddPiece(newPiece, null);
        }

        // Create the Tiles
        /// For the first 4 Tiles and last 2 Tiles in this players Track,
        /// assign thier owner to the corresponding owner.
        /// For the other Tiles, make thier ownership BOTH.
        for (int i = 0; i < 14; i++)
        {
            if ((i >= 0 && i <= 3) || i == 12 || i == 13)
            {
                string tileObjectKey = "Tile " + owner.ToString() + " " + i;
                tiles[i] = new Tile(owner, i, GameObject.Find(tileObjectKey).transform);
            }
            else
                tiles[i] = gameManager.centerTiles[i-4];
        }

        // Create the final destination Tile for scoring
        finalDestinationTile = new Tile(true, owner);
    }

    #region Operations

    /// <summary>
    /// AdvancePiece manages the advancement of a single Piece selected by the player.
    /// This Piece can only move to the expected Tile if that Tile is either unoccupied
    /// or occupied by the opponents Piece that is on a non-rosette Tile (safe Tile).
    /// If there is an opponents Piece on the expected Tile, then bump it off and let
    /// this Piece occupy that Tile
    /// </summary>
    /// <param name="piece">The Piece selected by the player</param>
    /// <param name="moves">The amount of moves for this Piece</param>
    public void AdvancePiece(Piece piece, int moves, out bool landedOnRosette, out bool reselect)
    {
        Queue<Tile> path = new Queue<Tile>();
        Tile destination = null;
        bool scored = false;
        landedOnRosette = false;
        reselect = false;
        

        // For whatever reason, if moves is less than or equal to 0,
        // don't allow this Piece to advance.
        // This should already be checked in the GameManager
        if (moves <= 0)
            return;

        // If the player selected a Piece that has already scored,
        // just make them reselect a different Piece
        if(piece.Scored)
        {
            reselect = true;
            return;
        }

        // If this Piece is not on the board
        if (!pieceManager.IsOnBoard(piece))
        {
            destination = tiles[moves - 1];

            // Build the path
            for (int i = 0; i <= IndexOf(destination); i++)
                path.Enqueue(tiles[i]);
        }
        // If this Piece is already on the board, then
        // get the destination Tile by getting this Piece's current Tile
        // and adding the number of times it will move forward. Now use this value as an
        // index for accessing the correct Tile within the tiles array
        else
        {
            int destIndex = IndexOf(pieceManager.GetTile(piece)) + moves;

            // SCORING MOVE - Player has exact amount of moves to score
            if(destIndex == TRACK_SIZE)
            {
                Debug.Log("Score a point!");
                piece.Scored = true;
                scored = true;

                // Make destination equal to the last Tile on the board,
                // then enqueue the final destination (the scoring spot),
                // ONLY if moves is greater than 1, otherwise just enqueue
                // the last spot (finalDestination)
                if (moves > 1)
                {
                    destination = tiles[tiles.Length - 1];

                    // Build the path
                    int currentIndex = IndexOf(pieceManager.GetTile(piece)) + 1;
                    for (int i = currentIndex; i <= IndexOf(destination); i++)
                        path.Enqueue(tiles[i]);
                }

                destination = finalDestinationTile;
                path.Enqueue(finalDestinationTile);

                gameManager.ScorePoint(owner);
            }
            // More than exact amount of moves to score
            else if(destIndex > TRACK_SIZE)
            {
                Debug.Log("Can only score with exact move value. Went over scoring move.");
                reselect = true;
                return;
            }
            // Somewhere on the board
            else
            {
                destination = tiles[destIndex];

                // Build the path
                int currentIndex = IndexOf(pieceManager.GetTile(piece)) + 1;
                for (int i = currentIndex; i <= IndexOf(destination); i++)
                    path.Enqueue(tiles[i]);
            }

        }
        
        // DESTINATION TILE OBTAINED
        // Now we can get the OccupationData on this Tile to determine our next step in the process
        OccupationData occupationData = destination.GetOccupationData(owner, piece.GetPieceID());
        int occupantId = occupationData.PieceId;
        Track opponentTrack = null;

        // If the Tile is already occupied
        if (occupationData.IsOccupied)
        {
            // If an opponent's Piece occupies this Tile
            if (occupationData.OpponentOverlap && !destination.IsRosette)
            {
                // Bump the opponent's Piece
                opponentTrack = gameManager.GetOpponentTrack(owner);
                gameManager.SetOpponentTrack(opponentTrack);
                destination.Unoccupy();
            }
            // Otherwise this Tile is occupied by our own Piece or
            // destinationTile is a rosette and the opponent's Piece is safe
            else
            {
                reselect = true;
                return;
            }
        }
        
        // TIME TO MOVE
        // Now update occupationData after bumping the opponet's Piece and move in
        bool occupationSuccess = false;
        occupationData = destination.Occupy((int)owner, piece.GetPieceID(), out occupationSuccess);

        // At this point, destinationTile should be clear for this Piece to occupy.
        if (occupationSuccess)
        {
            // Move this Piece object to the destination Tile object
            gameManager.MoveQueue(piece, path, occupantId, scored);

            // Set piece's parent to destinationTile
            piece.transform.SetParent(destination.GetTileTransform().FindChild("Graphic"));

            // Unoccupy the old Tile
            if (pieceManager.IsOnBoard(piece))
                pieceManager.GetTile(piece).Unoccupy();

            // Occupy the new Tile
            if (destination != finalDestinationTile)
                pieceManager.SetTile(piece, destination);
            else
                finalDestinationTile.Unoccupy();

            landedOnRosette = destination.IsRosette;
        }
        else
        {
            // If this side of the if is called, then that means
            // destinationTile is still occupied by an opponent's piece,
            // when it shouldn't be...
            Debug.LogError("Something went wrong.");
        }
    }
    
    #endregion

    #region Helper Functions
    
    /// <summary>
    /// Bump this Piece off the board
    /// </summary>
    /// <param name="pieceid">The ID of the Piece getting bumped off the board</param>
    public void BumpPiece(int pieceid)
    {
        Piece piece = pieceManager.GetPiece(pieceid);

        piece.Die();

        // Unoccupy the old Tile
        pieceManager.SetTile(piece, null);
    }

    int IndexOf(Tile tile)
    {
        int result = -1;

        for (int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] == tile)
            {
                result = i;
                break;
            }
        }

        return result;
    }

    public TilePreviewData GetPreviewTile(Piece piece, int step)
    {
        TilePreviewData previewData = new TilePreviewData();
        int index = IndexOf(pieceManager.GetTile(piece)) + step;
        
        // The preview Tile is a Tile on the board
        if (index < TRACK_SIZE)
        {
            previewData.tile = tiles[index];
            previewData.status = TilePreviewData.PreviewTileStatus.TILE_EXISTS;
        }
        // The preview Tile is the scoring Tile (not an actual Tile object)
        else if (index == TRACK_SIZE)
            previewData.status = TilePreviewData.PreviewTileStatus.SCORING_TILE;
        
        return previewData;
    }

#endregion

}
