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

        if (player == null) return false;
        if (!player.HasShipTicket) return false;
        if (player.HasSubmittedShipment) return false;

        var submittedItems = new List<int>();
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] != -1)
                submittedItems.Add(Items[i]);
        }

        Dictionary<int, int> count = new Dictionary<int, int>();

        bool isMasterSubmit = player.ActorNumber == GameManager.Instance.Player.ActorNumber;

        var validationSource = isMasterSubmit ? player.Ship : player.Inventory;

        foreach (int item in validationSource)
        {
            if (item == -1) continue;

            if (!count.ContainsKey(item))
                count[item] = 0;

            count[item]++;
        }

        for (int i = 0; i < submittedItems.Count; i++)
        {
            int item = submittedItems[i];
            if (!count.ContainsKey(item) || count[item] == 0)
                return false;

            count[item]--;
        }

        GameManager.Instance.AddProducts(submittedItems.ToArray());

        if (!GameManager.Instance.Shippers.Contains(player.ActorNumber))
            GameManager.Instance.Shippers.Add(player.ActorNumber);

        player.HasSubmittedShipment = true;
        player.HasShipTicket = false;

        return true;
    }

    public override void Response()
    {
        var player = GameManager.Instance.FindPlayer(ActorNumber);
        if (player == null) return;

        foreach (int deleted in Items)
        {
            if (deleted == -1) continue;

            for (int i = 0; i < player.Inventory.Length; i++)
            {
                if (player.Inventory[i] == deleted)
                {
                    player.Inventory[i] = -1;
                    break;
                }
            }
        }

        for (int i = 0; i < player.Ship.Length; i++)
        {
            player.Ship[i] = -1;
        }

        player.HasShipTicket = false;
        player.HasSubmittedShipment = true;

        if (GameManager.Instance.Player.ActorNumber == ActorNumber)
        {
            UIManager.Instance.hud.UpdateInventory();
            UIManager.Instance.ship.UpdateInventory();
        }
    }
}
