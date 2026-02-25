using Game.Enum;
using Photon.Pun;
using System.Collections.Generic;

public class ItemSubmitPacket : RPCPacket
{
    public override PacketType type => PacketType.ItemSubmit;

    public int ActorNumber;
    public int[] Items;

    public ItemSubmitPacket(int actorNumber, int[] items)
    {
        ActorNumber = actorNumber;
        Items = items;
    }

    public override void Send()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Request), RpcTarget.MasterClient, type, new object[] { ActorNumber, Items });
        }
        else if (Check())
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, type, new object[] { ActorNumber, Items });
        }
    }

    public override bool Check()
    {
        if (!PhotonNetwork.IsMasterClient) return false;
        
        var player = GameManager.Instance.FindPlayer(ActorNumber);

        if (!player.HasShipTicket) return false;

        Dictionary<int, int> count = new Dictionary<int, int>();

        // 인벤토리 개수 세기
        foreach (int item in player.Inventory)
        {
            if (!count.ContainsKey(item))
                count[item] = 0;

            count[item]++;
        }

        // 제출 카드 검증
        foreach (int item in Items)
        {
            if (!count.ContainsKey(item) || count[item] == 0)
                return false;

            count[item]--;
        }

        GameManager.Instance.AddProducts(Items);
        GameManager.Instance.Shippers.Add(player.ActorNumber);


        return true;
    }

    public override void Response()
    {
        var player = GameManager.Instance.FindPlayer(ActorNumber);

        foreach (int deleted in Items)
        {
            for (int i = 0; i < player.Inventory.Length; i++)
            {
                if (player.Inventory[i] == deleted)
                {
                    player.Inventory[i] = -1;
                    continue;
                }
            }
        }

        for(int i = 0; i < player.Ship.Length; i++)
        {
            player.Ship[i] = -1;
        }
        player.HasShipTicket = false;
    }
}