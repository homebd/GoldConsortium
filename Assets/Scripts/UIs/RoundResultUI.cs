using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoundResultUI : PhaseUI
{
    private WaitForSeconds delay = new WaitForSeconds(5f);

    [SerializeField] private TextMeshProUGUI _round;

    [SerializeField] private List<PlayerProfile> _players;
    [SerializeField] private List<TextMeshProUGUI> _deltas;

    private void OnEnable()
    {
        foreach (var player in _players)
        {
            player.gameObject.SetActive(false);
        }

        _round.text = $"{GameManager.Instance.Round}라운드 종료";

        var players = GameManager.Instance.Players.ToList();
        players.Sort((a,b) => (b.Money - a.Money));

        int i = 0;
        foreach (var player in players)
        {
            _players[i].UpdateIcon(player.Icon);
            _players[i].UpdateMoney(player.Money);
            _players[i].UpdateName(player.Name);
            _players[i].gameObject.SetActive(true);

            int delta = 0;
            if (GameManager.Instance.Benefits.Keys.Contains(player.ActorNumber))
            {
                delta = GameManager.Instance.Benefits[player.ActorNumber];
            }

            if (delta == 0)
            {
                _deltas[i].text = "";
                _deltas[i].gameObject.SetActive(false);
            }

            _deltas[i].text = $"({delta})";
            _deltas[i].color = delta > 0 ? Color.green : Color.red;
            _deltas[i].gameObject.SetActive(true);
            i++;
        }

        StartCoroutine(nameof(NextPhase));
    }

    private IEnumerator NextPhase()
    {
        yield return delay;

        GameManager.Instance.AsyncPhase();
    }
}