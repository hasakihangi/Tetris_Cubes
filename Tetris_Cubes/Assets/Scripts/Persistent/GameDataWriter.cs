using System.IO;
using UnityEngine;

public class GameDataWriter
{
    private BinaryWriter binaryWriter;

    public GameDataWriter(BinaryWriter binaryWriter)
    {
        this.binaryWriter = binaryWriter;
    }

    public void Write(float value)
    {
        binaryWriter.Write(value);
    }

    public void Write(int value)
    {
        binaryWriter.Write(value);
    }

    public void Write(bool value)
    {
        binaryWriter.Write(value);
    }

    public void Write(Color color)
    {
        Write(color.r);
        Write(color.g);
        Write(color.b);
        Write(color.a);
    }

    public void Write(string value)
    {
        binaryWriter.Write(value);
    }

    public void Write(Vector2Int value)
    {
        Write(value.x);
        Write(value.y);
    }
}