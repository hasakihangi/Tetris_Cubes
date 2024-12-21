using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPersistable
{
    void Save(GameDataWriter gameDataWriter);
    void Load(GameDataReader gameDataReader);
}
