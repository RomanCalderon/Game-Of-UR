using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The Tile class is responsible for storing data,
/// which manages each instances owner, position on the board,
/// whether it is occupied by a Piece, by which player and more.
/// </summary>
[System.Serializable]
public class Tile
{
    // Used for Inspector
    string tileName;

    [SerializeField]
    Transform tileObject;

    public enum OwnerTypes { P1, P2, BOTH }
    // Attributes / Data
    /// <summary>
    /// Tracks which player is able to occupy this Tile
    /// (ie. Player 1, Player 2, BOTH (center of board))
    /// 0 = Player 1
    /// 1 = Player 2
    /// 2 = BOTH
    /// </summary>
    private PlayerType owner;
    /// <summary>
    /// Where this Tile is located on the board
    /// </summary>
    private int position;
    /// <summary>
    /// If this Tile is currently occupied by a Piece
    /// </summary>
    private bool shared;
    /// <summary>
    /// If this Tile has a rosette,
    /// then which ever player that lands on this Tile
    /// is safe AND gets another turn
    /// </summary>
    private bool isRosette;
    /// <summary>
    /// This Tile's OccupationData
    /// </summary>
    private OccupationData occupationData = new OccupationData();
    
    #region Constructors
    
    // Mainly used for creating player-owned Tiles
    public Tile(PlayerType owner, int position, Transform tileObject)
    {
        tileName = owner.ToString() + " Tile " + position;
        this.tileObject = tileObject;
        this.owner = owner;
        this.position = position;
        isRosette = (position == 3 || position == 7 || position == 13);
        shared = owner != PlayerType.Neutral;
    }

    // Mainly used for creating center/shared Tiles
    public Tile(PlayerType owner, int position)
    {
        tileName = owner.ToString() + " Tile " + position;
        this.owner = owner;
        this.position = position;
        isRosette = (position == 3 || position == 7 || position == 13);
        shared = owner != PlayerType.Neutral;
    }

    /// <summary>
    /// Don't see any use for these constructors, but I'll just leave it
    /// </summary>
    public Tile(int pos)
    {
        position = pos;
        isRosette = (position == 3 || position == 7 || position == 13);
        shared = owner != PlayerType.Neutral;
    }
    public Tile()
    {
        position = -1;
        isRosette = (position == 3 || position == 7 || position == 13);
    }

    public Tile(bool isFinalDestination, PlayerType owner)
    {
        tileName = "Final Destination Tile";
        this.owner = owner;
        shared = false;
        tileObject = GameObject.FindWithTag(owner + " Final Destination").transform;
    }

    #endregion

    #region Setters/Getters

    // Accessors
    public int GetPosition()
    {
        return position;
    }
    public bool IsShared()
    {
        return shared;
    }

    // Mutators
    public void SetPosition(int pos)
    {
        position = pos;
    }

    public bool IsRosette
    {
        get
        {
            return isRosette;
        }

        set
        {
            isRosette = value;
        }
    }

    #endregion

    #region Operations

    public OccupationData GetOccupationData(PlayerType owner, int pieceid)
    {
        if(occupationData.IsOccupied)
        {
            // If a player is overtaking thier own piece
            if (occupationData.PlayerId == (int)owner)
                occupationData.SelfOccupied = true;
            // If a player is overtaking an opponent's piece
            else
            {
                occupationData.OverlapperId = pieceid;
                occupationData.OpponentOverlap = true;
            }

            occupationData.OverlapLocation = position;
        }

        return occupationData;
    }

    public OccupationData Occupy(int playerid, int pieceid, out bool success)
    {
        // If this Tile is not occupied, occupy it!
        if (!occupationData.IsOccupied)
        {
            occupationData.Occupy(playerid, pieceid);
            success = true;
        }
        // This Tile is occupied
        else
        {
            success = false;

            // If a player is overtaking thier own piece
            if (occupationData.PlayerId == playerid)
                occupationData.SelfOccupied = true;
            // If a player is overtaking an opponent's piece
            else
            {
                occupationData.OverlapperId = pieceid;
                occupationData.OpponentOverlap = true;
            }

            occupationData.OverlapLocation = position;
        }

        return occupationData;
    }

    public void Occupy(int playerid, int pieceid)
    {
        occupationData.Occupy(playerid, pieceid);
    }

    public void Unoccupy()
    {
        occupationData.Unoccupy();
    }

    public void Reveal()
    {
        tileObject.GetComponent<TileObject>().Reveal();
    }

#endregion

    #region Helper Functions

    public Vector3 GetObjectPosition()
    {
        return tileObject.position;
    }

    public Transform GetTileTransform()
    {
        return tileObject;
    }

    public override string ToString()
    {
        return tileObject.name;
    }

    #endregion
}