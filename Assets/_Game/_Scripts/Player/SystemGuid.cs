using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class SystemGuid : NetworkBehaviour
{
    public Guid guid;
    public NetworkVariable<FixedString128Bytes> guidstring =  new(writePerm: NetworkVariableWritePermission.Owner);
    
    
    
    public override void OnNetworkSpawn ()
    {
        guidfetch();
        
    }


    void guidfetch()
    {
        if(!IsOwner){return;}
        var tryfecthGuid = PlayerPrefs.GetString("guid", guid.ToString());

        if (tryfecthGuid == "" || tryfecthGuid == "00000000-0000-0000-0000-000000000000")
        {
            Guid guidnew = new Guid();
            guidnew = Guid.NewGuid();

            guid = guidnew;
            tryfecthGuid = guid.ToString();
            PlayerPrefs.SetString("guid", tryfecthGuid);


        }else
        {
            guid = new System.Guid(tryfecthGuid);
        }
      //  Debug.Log(tryfecthGuid);

       
        guidstring.Value = tryfecthGuid.ToString();

    }



    public string GetGuid()
    {
        
        
        
        if (IsOwner)
        {
           // Debug.Log("owner!!! " + guidstring.Value);
            guidstring.Value = guid.ToString();
        }else
        {
           // Debug.Log("not owner!!! " + guidstring.Value);
            
            
            
            
        }

        return guidstring.Value.ToString();
    }
}
