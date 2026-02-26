using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipUI : PhaseUI
{
    [SerializeField] private List<ItemSlot> _inventory;
    [SerializeField] private Button _submit;
    [SerializeField] private TextMeshProUGUI _submittedValueText;

    private void Awake()
    {
        for (int i = 0; i < _inventory.Count; i++)
        {
            var item = _inventory[i];
            item.Init(i, false);
            item.SetInteractable(true);
        }

        _submit.onClick.AddListener(Submit);
    }

    private void OnEnable()
    {
        if (!GameManager.Instance.Player.HasShipTicket) GameManager.Instance.AsyncPhase();

        UpdateInventory();
    }

    public void Submit()
    {
        GameManager.Instance.SubmitItem();
        GameManager.Instance.AsyncPhase();
    }

    public void UpdateInventory()
    {
        int totalValue = 0;

        for (int i = 0; i < _inventory.Count; i++)
        {
            int itemId = GameManager.Instance.Player.Ship[i];
            _inventory[i].UpdateItem(itemId);

            if (itemId >= 0)
            {
                var item = GameManager.Instance.Items.GetItem(itemId);
                if (item != null) totalValue += item.Value;
            }
        }

        if (_submittedValueText != null)
            _submittedValueText.text = $"출하 가치: {totalValue}";
    }
}
