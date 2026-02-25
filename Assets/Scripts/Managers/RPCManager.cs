using Game.Enum;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RPCManager : MonoBehaviourPunCallbacks
{
    private static RPCManager _instance;
    public static RPCManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RPCManager>();
            }

            if (_instance == null)
            {
                Debug.LogError("RPC Manager Can't Found");
            }

            return _instance;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LoadLevel("Start");
    }

    [PunRPC]
    public void RPC_Request(PacketType packetType, object[] parameters)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        RPCPacket packet = RPCPacketFactory.Create(packetType, parameters);

        if (packet != null && packet.Check())
        {
            photonView.RPC(nameof(RPC_Apply), RpcTarget.All, packetType, parameters);
        }
    }

    [PunRPC]
    public void RPC_Apply(PacketType packetType, object[] parameters)
    {
        RPCPacket packet = RPCPacketFactory.Create(packetType, parameters);

        packet?.Response();
    }
}