using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TravelUI : PhaseUI
{
    [SerializeField] private List<PlayerProfile> _profiles;

    [SerializeField] private List<Button> _options;

    [SerializeField] private TextMeshProUGUI _timer;

    [SerializeField] private List<Sprite> _bgs;
    [SerializeField] private Image _bg;

    private int _curMember;
    private float _time;
    private bool _isTimeOver;
    private bool _hasSubmittedOption;

    private void Awake()
    {
        for (int i = 0; i < _options.Count; i++) {
            int index = i;
            _options[index].onClick.AddListener(() => SelectOption(index));
        }
    }

    private void OnEnable()
    {
        _bg.sprite = _bgs[GameManager.Instance.Player.AreaIndex];

        _options[1].gameObject.SetActive(GameManager.Instance.Player.AreaIndex != 4);

        Init(GameManager.Instance.Config.DefaultTravelTime);
    }

    public void Init(float time)
    {
        _curMember = 0;
        foreach (var profile in _profiles)
        {
            profile.gameObject.SetActive(false);
        }

        var player = GameManager.Instance.Player;

        foreach (var p in GameManager.Instance.Players)
        {
            if (p.ActorNumber == player.ActorNumber) continue;

            if (p.AreaIndex == player.AreaIndex)
            {
                _profiles[_curMember].UpdateIcon(p.Icon);
                _profiles[_curMember].UpdateName(p.Name);
                _profiles[_curMember].gameObject.SetActive(true);
                _curMember++;
            }
        }

        _time = time;
        _isTimeOver = false;
        _hasSubmittedOption = false;
    }

    private void Update()
    {
        if (_isTimeOver) return;

        if (_time < 0f)
        {
            _isTimeOver = true;

            if (!_hasSubmittedOption)
                SelectOption(0);
        }

        _time -= Time.deltaTime;
        _timer.text = Mathf.CeilToInt(_time).ToString();
    }

    public void SelectOption(int index)
    {
        if (_hasSubmittedOption) return;

        if (GameManager.Instance.Player.AreaIndex != 4
            && index == 1 && GameManager.Instance.Player.Money < 2) return;

        _hasSubmittedOption = true;
        GameManager.Instance.SelectOption(index);

        GameManager.Instance.AsyncPhase();
    }
}
