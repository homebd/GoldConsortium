using Game.Enum;
using Photon.Pun;
using System.Collections.Generic;

public class VoteConfirmPacket : RPCPacket
{
    public override PacketType type => PacketType.VoteConfirm;

    public int ActorNumber;
    public int[] SelectionArr;

    public VoteConfirmPacket(int actorNumber, int[] selectionArr)
    {
        ActorNumber = actorNumber;
        SelectionArr = selectionArr;
    }

    public override void Send()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Request), RpcTarget.MasterClient, type, new object[] { ActorNumber, SelectionArr });
        }
        else if (Check())
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, type, new object[] { ActorNumber, SelectionArr });
        }
    }

    public override bool Check()
    {
        if (!PhotonNetwork.IsMasterClient)
            return false;

        var leader = GameManager.Instance.FindPlayer(ActorNumber);
        if (leader == null || !leader.IsLeader) return false;
        if (SelectionArr == null) return false;

        int requiredCount = GameManager.Instance.Config.VotedPlayer;
        if (SelectionArr.Length < requiredCount) return false;

        HashSet<int> selectedSet = new HashSet<int>();
        for (int i = 0; i < SelectionArr.Length; i++)
        {
            int actorNumber = SelectionArr[i];
            if (actorNumber == -1) continue;

            if (GameManager.Instance.FindPlayer(actorNumber) == null)
                return false;

            selectedSet.Add(actorNumber);
        }

        if (selectedSet.Count != requiredCount)
            return false;

        if (!selectedSet.Contains(leader.ActorNumber))
            return false;

        return true;
    }

    public override void Response()
    {
        foreach (var player in GameManager.Instance.Players)
        {
            player.HasShipTicket = false;
        }

        if (SelectionArr == null) return;

        for (int i = 0; i < SelectionArr.Length; i++)
        {
            int selectedActor = SelectionArr[i];
            if (selectedActor == -1) continue;

            var player = GameManager.Instance.FindPlayer(selectedActor);
            if (player != null)
            {
                player.HasShipTicket = true;
            }
        }
    }
}
