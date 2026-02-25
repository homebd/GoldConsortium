using Game.Enum;
using Photon.Pun;

public class UpdatePhasePacket : RPCPacket
{
    public override PacketType type => PacketType.UpdatePhase;

    public int ActorNumber;
    public Phase Phase;

    public UpdatePhasePacket(int actorNumber, Phase phase)
    {
        ActorNumber = actorNumber;
        Phase = phase;
    }

    public override void Send()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (Check())
        {
            RPCManager.Instance.photonView.RPC(nameof(RPCManager.RPC_Apply), RpcTarget.All, type, new object[] { ActorNumber, Phase });
        }
    }

    public override bool Check()
    {
        if (!PhotonNetwork.IsMasterClient) return false;

        return true;
    }

    public override void Response()
    {
        if (GameManager.Instance.Player.ActorNumber != ActorNumber) return;

        GameManager.Instance.SetPhase(Phase);
    }
}