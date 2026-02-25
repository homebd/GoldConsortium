using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoteResultUI : PhaseUI
{
    [SerializeField] private List<PlayerProfile> _playerInfos;
    private WaitForSeconds delay = new WaitForSeconds(5f);


    private void OnEnable()
    {
        StartCoroutine(nameof(NextPhase));

        foreach (var info in _playerInfos)
        {
            info.gameObject.SetActive(false);
        }

        int i = 0;
        foreach (var player in GameManager.Instance.Players)
        {
            if (!player.HasShipTicket)
            {
                continue;
            }

            _playerInfos[i].UpdateIcon(player.Icon);
            _playerInfos[i].UpdateName(player.Name);
            _playerInfos[i].gameObject.SetActive(true);
            i++;
        }
    }

    private IEnumerator NextPhase()
    {
        yield return delay;

        GameManager.Instance.AsyncPhase();
    }
}
