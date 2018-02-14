using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OccupationData
{
    // Is this Tile occupied?
    private bool isOccupied;
    // If so, by who?
    private int playerId;
    private int pieceId;
    // Self-occupied
    private bool selfOccupied;
    // If this Tile is being occupied by an opponent,
    // then store the opponents Piece id and flag the occupation
    private bool opponentOverlap;
    private int overlapperId;
    private int overlapLocation;

    #region Constructors

    public OccupationData()
    {
        isOccupied = false;
        playerId = -1;
        pieceId = -1;
    }

    public OccupationData(bool isOccupied, int playerId, int pieceId)
    {
        this.isOccupied = isOccupied;
        this.playerId = playerId;
        this.pieceId = pieceId;
    }

    #endregion

    #region Operations

    public void Occupy(int playerid, int pieceid)
    {
        isOccupied = true;
        playerId = playerid;
        pieceId = pieceid;
    }

    public void Unoccupy()
    {
        isOccupied = false;
        playerId = -1;
        pieceId = -1;
        selfOccupied = false;
    }

    #endregion

    #region Setters/Getters

    public bool IsOccupied
    {
        get
        {
            return isOccupied;
        }

        set
        {
            isOccupied = value;
        }
    }

    public int PlayerId
    {
        get
        {
            return playerId;
        }

        set
        {
            playerId = value;
        }
    }

    public int PieceId
    {
        get
        {
            return pieceId;
        }

        set
        {
            pieceId = value;
        }
    }

    public bool SelfOccupied
    {
        get
        {
            return selfOccupied;
        }

        set
        {
            selfOccupied = value;
        }
    }

    public int OverlapperId
    {
        get
        {
            return overlapperId;
        }

        set
        {
            overlapperId = value;
        }
    }

    public bool OpponentOverlap
    {
        get
        {
            return opponentOverlap;
        }

        set
        {
            opponentOverlap = value;
        }
    }

    public int OverlapLocation
    {
        get
        {
            return overlapLocation;
        }

        set
        {
            overlapLocation = value;
        }
    }

    #endregion
}
