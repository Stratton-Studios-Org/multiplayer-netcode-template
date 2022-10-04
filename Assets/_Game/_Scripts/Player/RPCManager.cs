using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

public class RPCManager : NetworkBehaviour
{
    Dictionary<ulong, CharacterController> players = new Dictionary<ulong, CharacterController>();
    Dictionary<ulong, Animator> players_anims = new Dictionary<ulong, Animator>();
    
    Animator anim;
    CharacterController controller;
   // public List<CharacterController> _playerController = new CharacterController<List>();
    void Start()
    {
      //_playerController = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }
    
    [ServerRpc]
    public void PlayerInputServerRpc(float x,float y, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        clientMovement(clientId,x,y);
    }
    
    [ServerRpc]
    public void AnimationMovementResetServerRpc(ServerRpcParams serverRpcParams = default)
    {
     
        var clientId = serverRpcParams.Receive.SenderClientId;
        CharacterController player = CheckIfInList(clientId);

        players_anims[clientId].SetFloat("Movespeed", 0);

        
    }
    
    
    void clientMovement(ulong clientId, float x, float y)
    {

        CharacterController player = CheckIfInList(clientId);
        Vector3 scaledMovement;
        scaledMovement = 200 * Time.fixedUnscaledDeltaTime * new Vector3(x, 0, y);
        player.transform.LookAt(player.transform.position + scaledMovement, Vector3.up);
        players_anims[clientId].SetFloat("Movespeed", scaledMovement.magnitude);
        
        player.SimpleMove(scaledMovement);
    }

   public void WinnerAnimState(ulong winnerId)
   {
       Debug.Log(winnerId);
        foreach (var VARIABLE in players_anims)
        {
           // Debug.Log(winnerId + "||" + VARIABLE.Key);

            if (VARIABLE.Key == winnerId)
            {
                VARIABLE.Value.SetBool("Victory", true);
            }
            else
            {
//                Debug.Log("not winner");
                VARIABLE.Value.SetBool("Lose", true);
            }
        }
    }
   

   public CharacterController CheckIfInList(ulong clientId)
    {
        if (!players.ContainsKey(clientId))
        {
            NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkedClient);
            var r = networkedClient.PlayerObject.gameObject.GetComponent<CharacterController>();
            var a = networkedClient.PlayerObject.gameObject.GetComponent<Animator>();

            players.Add(clientId, r);
            players_anims.Add(clientId, a);
            return players[clientId];

        }else
        {
            return players[clientId];
        }
    }
    
}
