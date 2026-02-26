using System.Text;
using Game.Data;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [Header("Refresh")]
    [SerializeField] private float _refreshInterval = 0.25f;

    [Header("Player (Local)")]
    [SerializeField] private TextMeshProUGUI _localPlayerText;

    [Header("Player (All)")]
    [SerializeField] private TextMeshProUGUI _allPlayersText;

    [Header("Photon")]
    [SerializeField] private TextMeshProUGUI _photonStatusText;

    private float _nextRefreshTime;

    private void OnEnable()
    {
        _nextRefreshTime = 0f;
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextRefreshTime) return;

        _nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, _refreshInterval);
        Refresh();
    }

    private void Refresh()
    {
        UpdatePhotonStatus();
        UpdateLocalPlayer();
        UpdateAllPlayers();
    }

    private void UpdatePhotonStatus()
    {
        if (_photonStatusText == null) return;

        var sb = new StringBuilder(256);

        sb.AppendLine("[Photon]");
        sb.AppendLine($"IsConnected: {PhotonNetwork.IsConnected}");
        sb.AppendLine($"InRoom: {PhotonNetwork.InRoom}");
        sb.AppendLine($"IsMasterClient: {PhotonNetwork.IsMasterClient}");
        sb.AppendLine($"ClientState: {PhotonNetwork.NetworkClientState}");
        sb.AppendLine($"Ping: {PhotonNetwork.GetPing()} ms");
        sb.AppendLine($"Server: {PhotonNetwork.ServerAddress}");

        if (PhotonNetwork.LocalPlayer != null)
            sb.AppendLine($"LocalActorNumber: {PhotonNetwork.LocalPlayer.ActorNumber}");

        if (PhotonNetwork.CurrentRoom != null)
        {
            sb.AppendLine($"Room: {PhotonNetwork.CurrentRoom.Name}");
            sb.AppendLine($"PlayersInRoom: {PhotonNetwork.CurrentRoom.PlayerCount}");
        }

        _photonStatusText.text = sb.ToString();
    }

    private void UpdateLocalPlayer()
    {
        if (_localPlayerText == null) return;

        var manager = GameManager.Instance;
        if (manager == null || !PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            _localPlayerText.text = "[Local Player]\nNot ready";
            return;
        }

        Player player = manager.FindPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
        if (player == null)
        {
            _localPlayerText.text = "[Local Player]\nPlayer not initialized";
            return;
        }

        var sb = new StringBuilder(512);
        sb.AppendLine("[Local Player]");
        sb.AppendLine($"Phase: {manager.Phase}");
        sb.AppendLine($"Round: {manager.Round}/{manager.Config.Round}");
        sb.AppendLine($"MustTravel: {manager.MustTravel}");
        sb.AppendLine($"ActorNumber: {player.ActorNumber}");
        sb.AppendLine($"Name: {player.Name}");
        sb.AppendLine($"IconNum: {player.IconNum}");
        sb.AppendLine($"Icon: {(player.Icon != null ? player.Icon.name : "null")}");
        sb.AppendLine($"hasSelected: {player.hasSelected}");
        sb.AppendLine($"AreaIndex: {player.AreaIndex}");
        sb.AppendLine($"Money: {player.Money}");
        sb.AppendLine($"IsLeader: {player.IsLeader}");
        sb.AppendLine($"HasShipTicket: {player.HasShipTicket}");
        sb.AppendLine($"IsActionFinished: {player.IsActionFinished}");
        sb.AppendLine($"Inventory: {FormatItemArray(player.Inventory)}");
        sb.AppendLine($"Ship: {FormatItemArray(player.Ship)}");

        _localPlayerText.text = sb.ToString();
    }

    private void UpdateAllPlayers()
    {
        if (_allPlayersText == null) return;

        var manager = GameManager.Instance;
        if (manager == null)
        {
            _allPlayersText.text = "[Players]\nGameManager not ready";
            return;
        }

        var sb = new StringBuilder(512);
        sb.AppendLine($"[Players] Count: {manager.Players.Count}");

        foreach (var p in manager.Players)
        {
            sb.AppendLine(
                $"#{p.ActorNumber} {p.Name} | Money:{p.Money} | Area:{p.AreaIndex} | Leader:{p.IsLeader} | Selected:{p.hasSelected} | Done:{p.IsActionFinished}");
        }

        _allPlayersText.text = sb.ToString();
    }

    private string FormatItemArray(int[] itemIds)
    {
        if (itemIds == null || itemIds.Length == 0) return "[]";

        var manager = GameManager.Instance;
        var sb = new StringBuilder(itemIds.Length * 12);
        sb.Append("[");

        for (int i = 0; i < itemIds.Length; i++)
        {
            if (i > 0) sb.Append(", ");

            int id = itemIds[i];
            if (id < 0)
            {
                sb.Append("-");
                continue;
            }

            if (manager != null && manager.Items != null)
            {
                var item = manager.Items.GetItem(id);
                sb.Append(item != null ? $"{id}:{item.Name}" : id.ToString());
            }
            else
            {
                sb.Append(id);
            }
        }

        sb.Append("]");
        return sb.ToString();
    }
}
