using Game.Enum;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private Dictionary<Phase, PhaseUI> _phaseUI = new();
    public HUD hud { get; private set; }
    public ShipUI ship => _phaseUI[Phase.Feed] as ShipUI;
    public CoverUI cover { get; private set; }

    private void Awake()
    {
        var uis = Resources.LoadAll<UIBase>("UIs/");

        foreach (var ui in uis)
        {
            if (ui is PhaseUI)
            {
                var phase = ui as PhaseUI;
                _phaseUI[phase.Phase] = phase;

                GameManager.Instance.AddListener(phase.Phase, false, () => HidePhaseUI(phase.Phase));
                GameManager.Instance.AddListener(phase.Phase, true, () => ShowPhaseUI(phase.Phase));
            }

            if (ui is HUD)
            {
                hud = Instantiate(ui as HUD);
                GameManager.Instance.AddListener(Phase.Travel, true, () =>  hud.gameObject.SetActive(true));
                GameManager.Instance.AddListener(Phase.Travel, true, hud.ShowRound);
                GameManager.Instance.AddListener(Phase.GoHome, true, hud.UpdateArea);
                GameManager.Instance.AddListener(Phase.Travel, true, hud.UpdateArea);
                GameManager.Instance.AddListener(Phase.Vote, true, () => { if (true) hud.HideRound(); });
                GameManager.Instance.AddListener(Phase.VoteResult, false, hud.ShowRound);
                GameManager.Instance.AddListener(Phase.Feed, true, () => hud.SetInventoryInteractable(true));
                GameManager.Instance.AddListener(Phase.Feed, false, () => hud.SetInventoryInteractable(false));
                GameManager.Instance.AddListener(Phase.RoundResult, true, () => hud.gameObject.SetActive(false));
            }

            if (ui is CoverUI)
            {
                cover = Instantiate(ui as CoverUI);
                cover.gameObject.SetActive(false);
            }
        }
    }

    public void ShowPhaseUI(Phase phase)
    {
        if (!TryResolvePhaseUI(phase, out PhaseUI ui))
        {
            Debug.LogError("UI를 찾을 수 없습니다.");
            return;
        }

        if (!ui.gameObject.scene.IsValid())
        {
            ui = Instantiate(ui);
            _phaseUI[ui.Phase] = ui;
        }

        ui.gameObject.SetActive(true);
    }

    public void HidePhaseUI(Phase phase)
    {
        if (!TryResolvePhaseUI(phase, out PhaseUI ui)) return;

        if (!ui.gameObject.scene.IsValid()) return;

        ui.gameObject.SetActive(false);
    }

    private bool TryResolvePhaseUI(Phase phase, out PhaseUI ui)
    {
        ui = null;

        Phase resolved = phase;
        if (phase == Phase.Vote && !GameManager.Instance.Player.IsLeader)
            resolved = Phase.VoteWait;
        else if (phase == Phase.Feed && !GameManager.Instance.Player.HasShipTicket)
            resolved = Phase.FeedWait;

        return _phaseUI.TryGetValue(resolved, out ui);
    }
}
