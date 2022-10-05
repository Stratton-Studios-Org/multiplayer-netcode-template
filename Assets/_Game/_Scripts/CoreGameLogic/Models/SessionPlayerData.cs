using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;


    [Serializable]
    public struct SessionPlayerData : INetworkSerializable, System.IEquatable<SessionPlayerData>
    {
        [SerializeField]public bool IsConnected;
        [SerializeField]public int playerScore;

        [SerializeField]public ulong ClientID;

        [SerializeField] public FixedString128Bytes systemID;
        
        [SerializeField] public NetworkObjectReference playerObject;


        public void diconnect()
        {
            IsConnected = false;
        }
        
        public SessionPlayerData(int score, FixedString128Bytes systemIDd, bool connected, ulong clientID, NetworkObjectReference playerObject)
        {
            playerScore = score;
            systemID = systemIDd;
            IsConnected = connected;
            ClientID = clientID;
            this.playerObject = playerObject;
        }


        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerScore);
            serializer.SerializeValue(ref systemID);
            serializer.SerializeValue(ref IsConnected);
            serializer.SerializeValue(ref ClientID);
            serializer.SerializeValue(ref playerObject);

        }

        public bool Equals(SessionPlayerData other)
        {
            return playerScore == other.playerScore  && systemID == other.systemID &&
                   IsConnected == other.IsConnected && ClientID == other.ClientID;
        }
        
    }
