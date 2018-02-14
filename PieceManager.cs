using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager
{
    private List<PieceLocationData> pieces = new List<PieceLocationData>();

    public void AddPiece(Piece p, Tile t)
    {
        PieceLocationData newData = new PieceLocationData(p, t);
        pieces.Add(newData);
    }

    public void RemovePiece(Piece p)
    {
        foreach (PieceLocationData pld in pieces)
        {
            if(pld.GetPiece() == p)
            {
                pieces.Remove(pld);
                return;
            }
        }
        Debug.LogError("Could not find PLD with p " + p.ToString());
    }

    public bool ContainsPiece(Piece key)
    {
        foreach (PieceLocationData pld in pieces)
            if(pld.GetPiece() == key)
                return true;

        return false;
    }

    public bool OccupiesTile(Tile key)
    {
        foreach (PieceLocationData pld in pieces)
            if (pld.GetTile() == key)
                return true;

        return false;
    }

    public PieceLocationData this[int index]
    {
        get
        {
            return pieces[index];
        }
    }

    public bool IsOnBoard(Piece piece)
    {
        foreach (PieceLocationData pld in pieces)
            if (pld.GetPiece() == piece)
                return pld.IsOnBoard();

        Debug.LogError("Piece not found");
        return false;
    }

    public Piece GetPiece(int pieceid)
    {
        Piece result = null;

        foreach (PieceLocationData pld in pieces)
            if (pld.GetPiece().GetPieceID() == pieceid)
                result = pld.GetPiece();

        return result;
    }

    public Tile GetTile(Piece piece)
    {
        Tile result = null;

        foreach (PieceLocationData pld in pieces)
            if(pld.GetPiece() == piece)
                result = pld.GetTile();

        return result;
    }

    public int IndexOf(Piece piece)
    {
        for (int i = 0; i < pieces.Count; i++)
            if(ContainsPiece(piece))
                return i;

        Debug.LogError("Could not find " + piece);
        return -1;
    }

    public PieceLocationData GetPLD(Piece piece)
    {
        foreach (PieceLocationData pld in pieces)
            if(pld.GetPiece() == piece)
                return pld;

        return null;
    }

    public void SetTile(Piece piece, Tile tile)
    {
        GetPLD(piece).SetTile(tile);
    }
}


public class PieceLocationData
{
    private Piece piece;
    private Tile tile;
    private bool hasTile = false;

    public PieceLocationData(Piece piece, Tile tile = null)
    {
        this.piece = piece;
        this.tile = tile;
        hasTile = tile != null;
    }

    public void SetPiece(Piece p)
    {
        piece = p;
    }

    public Piece GetPiece()
    {
        return piece;
    }

    public void SetTile(Tile t)
    {
        tile = t;
        hasTile = tile != null;
    }

    public Tile GetTile()
    {
        return tile;
    }

    public bool IsOnBoard()
    {
        //Debug.Log(piece.ToString() + " hasTile = " + hasTile);
        return hasTile;
    }
}
