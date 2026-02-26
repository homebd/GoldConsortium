using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemberSelectionUI : PhaseUI
{
    [SerializeField] private List<PlayerInfo> _playerInfos;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private TextMeshProUGUI _count;

    private readonly Dictionary<int, PlayerInfo> _playerInfoByActor = new();
    private readonly HashSet<int> _selectedActors = new();

    private int[] _selectionArr;
    private int _requiredCount;
    private int _leaderActorNumber = -1;

    private void Awake()
    {
        InitPlayerButtons();
        _confirmBtn.onClick.AddListener(Confirm);
    }

    private void OnEnable()
    {
        _requiredCount = GameManager.Instance.Config.VotedPlayer;
        _selectionArr = new int[_requiredCount];
        _selectedActors.Clear();

        for (int i = 0; i < _selectionArr.Length; i++)
        {
            _selectionArr[i] = -1;
        }

        InitPlayerButtons();
        RefreshLeaderSelection();
        UpdateConfirmState();
        UpdateCount();
    }

    private void InitPlayerButtons()
    {
        for (int i = 0; i < _playerInfos.Count; i++)
        {
            _playerInfos[i].gameObject.SetActive(false);
        }

        _playerInfoByActor.Clear();

        int index = 0;
        foreach (var player in GameManager.Instance.Players)
        {
            if (index >= _playerInfos.Count) break;

            var info = _playerInfos[index];
            info.gameObject.SetActive(true);
            info.UpdateIcon(player.Icon);
            info.UpdateName(player.Name);
            info.UpdateMoney(player.Money);
            info.Button.onClick.RemoveAllListeners();

            int actorNumber = player.ActorNumber;
            info.Button.onClick.AddListener(() => ToggleSelection(actorNumber));

            _playerInfoByActor[actorNumber] = info;
            index++;
        }
    }

    private void RefreshLeaderSelection()
    {
        _leaderActorNumber = -1;

        var leader = GameManager.Instance.Leader;
        if (leader != null)
        {
            _leaderActorNumber = leader.ActorNumber;
            _selectedActors.Add(_leaderActorNumber);
        }

        foreach (var pair in _playerInfoByActor)
        {
            bool isLeader = pair.Key == _leaderActorNumber;
            pair.Value.Button.interactable = !isLeader;
        }

        RebuildSelectionArray();
    }

    private void ToggleSelection(int actorNumber)
    {
        if (actorNumber == _leaderActorNumber) return;

        if (_selectedActors.Contains(actorNumber))
        {
            _selectedActors.Remove(actorNumber);
        }
        else
        {
            if (_selectedActors.Count >= _requiredCount)
            {
                return;
            }

            _selectedActors.Add(actorNumber);
        }

        RebuildSelectionArray();
        UpdateConfirmState();
        UpdateCount();
    }

    private void RebuildSelectionArray()
    {
        for (int i = 0; i < _selectionArr.Length; i++)
        {
            _selectionArr[i] = -1;
        }

        int iSelection = 0;
        foreach (int actor in _selectedActors)
        {
            if (iSelection >= _selectionArr.Length) break;
            _selectionArr[iSelection++] = actor;
        }
    }

    private void Confirm()
    {
        if (_selectedActors.Count != _requiredCount) return;

        GameManager.Instance.DeliverSelectionArray(_selectionArr);
        GameManager.Instance.AsyncPhase();
    }

    private void UpdateCount()
    {
        _count.text = $"확정 ({_selectedActors.Count}/{_requiredCount})";
    }

    private void UpdateConfirmState()
    {
        _confirmBtn.interactable = _selectedActors.Count == _requiredCount;
    }
}
