using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;


[Serializable]
public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public ulong PlayerID;
    public int Playerscore;

 
   public bool Playerconnected;

    [SerializeField] public FixedString128Bytes systemID;
    public PlayerData(ulong id, int score, FixedString128Bytes systemIDd, bool connected)
    {
        PlayerID = id;
        Playerscore = score;
        systemID = systemIDd;
        Playerconnected = connected;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref Playerscore);
        serializer.SerializeValue(ref systemID);
        serializer.SerializeValue(ref Playerconnected);
    }

    public bool Equals(PlayerData other)
    {
        return Playerscore == other.Playerscore && PlayerID == other.PlayerID && systemID == other.systemID && Playerconnected == other.Playerconnected;
    }
}