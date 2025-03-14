using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    public class PlayerCheatSync : MonoBehaviourPunCallbacks
    {
        [PunRPC]
        public void SpawnItemRPC(Vector3 spawnPosition)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Hax2.Log1("Master Client received RPC and spawned item at: " + spawnPosition);
            }
        }
    }
}