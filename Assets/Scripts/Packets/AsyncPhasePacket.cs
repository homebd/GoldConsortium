using Game.Enum;
using Photon.Pun;
using UnityEngine;

public class AsyncPhasePacket : RPCPacket
{
    public override PacketType type => PacketType.AsyncPhase;

    public int ActorNumber;

    public AsyncPhasePacket(int actorNumber)
    {
        ActorNumber = actorNumber;
    }

    public override void Send()
    {
        UIManager.Instance.cover.gameObject.SetActive(true);

        if (!PhotonNetwork.IsMasterClient)
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Request), RpcTarget.MasterClient, type, new object[] { ActorNumber });
        }
        else if (Check())
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, type, new object[] { ActorNumber });
        }
    }

    public override bool Check()
    {
        if (!PhotonNetwork.IsMasterClient) return false;

        GameManager.Instance.FindPlayer(ActorNumber).IsActionFinished = true;

        foreach (var player in GameManager.Instance.Players)
        {
            if (player.IsActionFinished == false) return false;
        }

        var phase = GameManager.Instance.Phase;
        Phase nextPhase = phase switch
        {
            Phase.InLobby => Phase.TravelSelection,
            Phase.TravelSelection => Phase.Travel,
            Phase.Travel => Phase.GoHome,
            Phase.GoHome => GameManager.Instance.MustTravel ? Phase.TravelSelection : Phase.Vote,
            Phase.Vote => Phase.VoteResult,
            Phase.VoteResult => Phase.Feed,
            Phase.Feed => Phase.Calculate,
            Phase.Calculate => Phase.RoundResult,
            Phase.RoundResult => GameManager.Instance.Round < GameManager.Instance.Config.Round ? Phase.TravelSelection : Phase.GameResult,
            _ => Phase.InLobby,
        };

        foreach (var player in GameManager.Instance.Players)
        {
            bool autoReady = false;

            if (nextPhase == Phase.Vote)
            {
                autoReady = !player.IsLeader;
            }
            else if (nextPhase == Phase.Feed)
            {
                autoReady = !player.HasShipTicket;
            }

            player.IsActionFinished = autoReady;

            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, PacketType.UpdatePhase, new object[] { player.ActorNumber, nextPhase });
        }

        return true;
    }

    public override void Response()
    {
        UIManager.Instance.cover.gameObject.SetActive(false);
    }
}
