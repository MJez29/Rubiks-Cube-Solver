using UnityEngine;
using System.Collections;

public class Block
{
    public Color top, front, right, back, left, bottom;
    public Vector3 pos;

    public GameObject go;

    public void ColorAllSides(Color col)
    {
        top = front = right = back = left = bottom = col;
    }

    public void SetSide(string side, Color col)
    {
        switch (side)
        {
            case "TOP":
                top = col;
                break;
            case "FRONT":
                front = col;
                break;
            case "RIGHT":
                right = col;
                break;
            case "BACK":
                back = col;
                break;
            case "LEFT":
                left = col;
                break;
            case "BOTTOM":
                bottom = col;
                break;
        }
    }
}