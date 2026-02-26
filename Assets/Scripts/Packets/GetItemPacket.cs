using Game.Enum;
using Photon.Pun;
using System;

public class GetItemPacket : RPCPacket
{
    public override PacketType type => PacketType.GetItem;

    public int ActorNumber;
    public int Index;

    public GetItemPacket(int actorNumber, int index)
    {
        ActorNumber = actorNumber;
        Index = index;
    }

    public override void Send()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Request), RpcTarget.MasterClient, type, new object[] { ActorNumber, Index });
        }
        else if (Check())
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, type, new object[] { ActorNumber, Index });
        }
    }

    public override bool Check()
    {
        if (!PhotonNetwork.IsMasterClient) return false;

        var player = GameManager.Instance.FindPlayer(ActorNumber);

        if (player == null) return false;
        if (player.HasReceivedTravelReward) return false;

        player.HasReceivedTravelReward = true;

        if (player.AreaIndex == 4) // ÈÞ½Ä
        {
            RPCPacketFactory.Create(PacketType.ChangeMoney, player.ActorNumber, 1).Send();
            return false;
        }

        int itemSize = Index == 0 ? 1 : 2;
        if (Index == 1) {
            if (player.Money < 2) return false;
                RPCPacketFactory.Create(PacketType.ChangeMoney, player.ActorNumber, -2).Send();
        }

        int empty = 6;

        foreach (var slot in player.Inventory) if (slot != -1) empty--;

        itemSize = Math.Min(itemSize, empty);
        var items = new int[itemSize];

        for (int i = 0; i < itemSize; i++)
        {
            items[i] = GameManager.Instance.Items.GetAreaItem(player.AreaIndex).ID;
        }

        RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, PacketType.GiveItem, new object[] { ActorNumber, items });

        return false;
    }

    public override void Response()
    {
    }
}
