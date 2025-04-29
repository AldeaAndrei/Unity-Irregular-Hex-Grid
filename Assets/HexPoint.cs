using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexPoint
{
    public int row;
    public int index;
    public Vector3 position;

    public HexPoint(int row, int index, Vector3 position)
    {
        this.row = row;
        this.index = index;
        this.position = position;
    }

    public override bool Equals(object obj)
    {
        if (obj is HexPoint other)
        {
            return this.row == other.row && this.index == other.index;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return row * 397 ^ index;
    }
}
