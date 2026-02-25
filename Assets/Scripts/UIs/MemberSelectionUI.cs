using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemberSelectionUI : PhaseUI
{
    [SerializeField] private List<PlayerInfo> _playerInfos;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private TextMeshProUGUI _count;

    //선택된 인원 리스트로 저장
    private int[] selectionArr;
    private int selectionCnt = 0;

    private void Awake()
    {
        var playerSize = GameManager.Instance.Config.MaxPlayer;

        int i = 0;
        foreach (var player in GameManager.Instance.Players)
        {
            var info = _playerInfos[i];

            info.gameObject.SetActive(true);
            info.UpdateIcon(player.Icon);
            info.UpdateName(player.Name);
            info.UpdateMoney(player.Money);
            info.Button.onClick.AddListener(() => SelectionArray(player.ActorNumber));
            info.Button.onClick.AddListener(() => info.Button.interactable = false);

            i++;
        }

        _confirmBtn.onClick.AddListener(Confirm);
    }

    private void OnEnable()
    {
        selectionCnt = 0;
        selectionArr = new int[GameManager.Instance.Config.VotedPlayer];

        for(int i = 0; i < selectionArr.Length; i++)
        {
            selectionArr[i] = -1;
        }

        for (int i = 0; i < _playerInfos.Count; i++)
        {
            _playerInfos[i].Button.interactable = true;
        }

        UpdateCount();
    }

    public void SelectionArray(int index)
    {
        //check
        for(int i = 0; i < selectionCnt; i++)
        {
            if (selectionArr[i] == index)
            {
                Debug.Log("Already Selected Player");
                return;
            }
        }

        if (selectionCnt < 4)
            selectionArr[selectionCnt++] = index;
        else
            Debug.Log("selection array full");

        UpdateCount();
    }

    private void Confirm()
    {
        GameManager.Instance.DeliverSelectionArray(selectionArr);
        GameManager.Instance.AsyncPhase();
    }

    private void UpdateCount() => _count.text = $"확정 ({selectionCnt}/{selectionArr.Length})";
}
