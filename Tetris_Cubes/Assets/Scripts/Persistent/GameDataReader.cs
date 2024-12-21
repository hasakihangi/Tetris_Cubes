using System.IO;
using UnityEngine;

public class GameDataReader
{
    private BinaryReader binaryReader;

    public GameDataReader(BinaryReader binaryReader)
    {
        this.binaryReader = binaryReader;
    }

    public float ReadFloat()
    {
        return binaryReader.ReadSingle();
    }

    public int ReadInt()
    {
        return binaryReader.ReadInt32();
    }

    public bool ReadBool()
    {
        return binaryReader.ReadBoolean();
    }

    public string ReadString()
    {
        return binaryReader.ReadString();
    }

    private float r;
    private float g;
    private float b;
    private float a;
    public Color ReadColor()
    {
        r = ReadFloat();
        g = ReadFloat();
        b = ReadFloat();
        a = ReadFloat();
        return new Color(r, g, b, a);
    }

    public Vector2Int ReadVector2Int()
    {
        int x = ReadInt();
        int y = ReadInt();
        return new Vector2Int(x, y);
    }
}