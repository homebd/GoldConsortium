using System.Text;
using TMPro;
using UnityEngine;

public class PhaseWaitStatusUI : PhaseUI
{
    private enum WaitType
    {
        Vote,
        Feed
    }

    [SerializeField] private WaitType _waitType;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private float _refreshInterval = 0.2f;

    private float _nextRefreshTime;

    private void OnEnable()
    {
        _nextRefreshTime = 0f;
        RefreshText();
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextRefreshTime) return;

        _nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, _refreshInterval);
        RefreshText();
    }

    private void RefreshText()
    {
        if (_statusText == null) return;

        if (_waitType == WaitType.Vote)
        {
            var leader = GameManager.Instance.Leader;
            string leaderName = leader != null ? leader.Name : "???";
            _statusText.text = $"{leaderName}이 출하 인원을 선별하고 있습니다..";
            return;
        }

        StringBuilder sb = new StringBuilder(128);
        int shipperCount = 0;

        foreach (var player in GameManager.Instance.Players)
        {
            if (!player.HasShipTicket) continue;

            if (shipperCount > 0)
                sb.Append(", ");

            sb.Append($"[{player.Name}]");
            shipperCount++;
        }

        if (shipperCount == 0)
        {
            _statusText.text = "아무도 출하하고 있지 않습니다..?";
            return;
        }

        _statusText.text = $"{sb}이 출하하고 있습니다..";
    }
}
