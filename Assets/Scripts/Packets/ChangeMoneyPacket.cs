using Game.Enum;
using Photon.Pun;

public class ChangeMoneyPacket : RPCPacket
{
    public override PacketType type => PacketType.ChangeMoney;

    public int ActorNumber;
    public int Money;

    public ChangeMoneyPacket(int actorNumber, int money)
    {
        ActorNumber = actorNumber;
        Money = money;
    }

    public override void Send()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Request), RpcTarget.MasterClient, type, new object[] { ActorNumber, Money });
        }
        else if (Check())
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, type, new object[] { ActorNumber, Money });
        }
    }

    public override bool Check()
    {
        if (!PhotonNetwork.IsMasterClient) return false;

        return true;
    }

    public override void Response()
    {
        if (!GameManager.Instance.Benefits.ContainsKey(ActorNumber))
        {
            GameManager.Instance.Benefits[ActorNumber] = Money;
        }
        else
        {
            GameManager.Instance.Benefits[ActorNumber] += Money;
        }

        GameManager.Instance.FindPlayer(ActorNumber).Money += Money;
        if (ActorNumber == GameManager.Instance.Player.ActorNumber)
            UIManager.Instance.hud.UpdateMoney();
    }
}
