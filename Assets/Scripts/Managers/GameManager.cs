using Game.Data;
using Game.Enum;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Phase Phase { get; private set; }

    private Dictionary<(Phase, bool), Action> _phaseEvents = new();

    //Master Client
    private Dictionary<int, Player> _players = new();
    private Dictionary<int, int> _products = new();
    public Dictionary<int, int> Benefits = new();
    public List<int> Shippers = new();

    public IReadOnlyCollection<Player> Players => _players.Values;
    public Player Player => _players[_actorNumber];
    public Player FindPlayer(int actorNumber) => _players.ContainsKey(actorNumber) ? _players[actorNumber] : null;

    private int _actorNumber;

    private int _travelCount;
    public bool MustTravel => _travelCount < Config.TravelCycle;
    public int Round { get; private set; } = 1;
    public ItemSO Items;
    public ConfigSO Config;

    private void Awake()
    {
        Items = Resources.Load<ItemSO>("SOs/ItemSO");
        Config = Resources.Load<ConfigSO>("SOs/ConfigSO");
    }

    private void Start()
    {
        AddListener(Phase.GoHome, true, () => _travelCount++);
        AddListener(Phase.Vote, true, () => _travelCount = 0);
        AddListener(Phase.Calculate, true, CaculateMoney);
        AddListener(Phase.RoundResult, false, () => { _products.Clear(); Benefits.Clear(); Shippers.Clear(); Round++; SetLeader(); UIManager.Instance.hud.Init(); });
    }

    public void InitPlayers()
    {
        foreach (var p in PhotonNetwork.CurrentRoom.Players.Values)
        {
            var player = new Player(p.ActorNumber, p.NickName, Config.DefaultMoney);
            _players[p.ActorNumber] = player;

            AddListener(Phase.TravelSelection, true, () => player.hasSelected = false);
        }

        _actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        ChangeIcon(UnityEngine.Random.Range(0, Config.Icons.Count));
        SetLeader();
    }

    public void SetLeader()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int leader = _actorNumber;

        if (Round == 1)
        {
            var random = UnityEngine.Random.Range(0, Players.Count);
            leader = Players.ToList()[random].ActorNumber;
        }
        else
        {
            int maxMoney = -1;

            foreach (var player in Players)
            {
                if (maxMoney < player.Money) {
                    leader = player.ActorNumber;
                    maxMoney = player.Money;
                }
            }
        }

        SetLeader(leader);
    }

    public void SetLeader(int actorNumber) => RPCPacketFactory.Create(PacketType.SetLeader, actorNumber).Send();
    public void ChangeMoney(int actorNumber, int money) => RPCPacketFactory.Create(PacketType.ChangeMoney, actorNumber, money).Send();
    public void ChangeIcon(int sprite) => RPCPacketFactory.Create(PacketType.ChangeIcon, _actorNumber, sprite).Send();
    public void DeliverSelectionArray(int[] selectionArr) => RPCPacketFactory.Create(PacketType.VoteConfirm, _actorNumber, selectionArr).Send();
    public void ClickArea(int index) => RPCPacketFactory.Create(PacketType.TravelSelection, _actorNumber, index).Send();
    public void SendChat(string chat) => RPCPacketFactory.Create(PacketType.Chat, _actorNumber, chat).Send();
    public void SubmitItem() => RPCPacketFactory.Create(PacketType.ItemSubmit, _actorNumber, Player.Ship).Send();
    public void SelectOption(int index) => RPCPacketFactory.Create(PacketType.GetItem, _actorNumber, index).Send();
    public void AsyncPhase() { RPCPacketFactory.Create(PacketType.AsyncPhase, _actorNumber).Send(); }

    public void ClickItem(int index, bool isInventory)
    {
        if (isInventory)
        {
            for (int i = 0; i < Player.Ship.Length; i++)
            {
                if (Player.Ship[i] == -1)
                {
                    Player.Ship[i] = Player.Inventory[index];
                    Player.Inventory[index] = -1;
                    break;
                }
            }
        }
        else
        {
            var itemId = Player.Ship[index];
            Player.Ship[index] = -1;

            for (int i = 0; i < Player.Inventory.Length; i++)
            {
                if (Player.Inventory[i] == -1)
                {
                    Player.Inventory[i] = itemId;
                    break;
                }
            }
        }

        UIManager.Instance.hud.UpdateInventory();
        UIManager.Instance.ship.UpdateInventory();
    }


    public void CheckAllPlayersSelected()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool selected = true;
        foreach(var p in _players.Values)
        {
            selected &= p.hasSelected;
        }

        if (selected) { } //�ٷ� ����?
    }
    public void SetPhase(Phase phase)
    {
        if (_phaseEvents.TryGetValue((Phase, false), out Action entry))
        {
            entry?.Invoke();
        }

        Phase = phase;

        if (_phaseEvents.TryGetValue((Phase, true), out entry))
        {
            entry?.Invoke();
        }
    }

    public void AddProducts(int[] products)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        foreach (int product in products)
        {
            if (!_products.ContainsKey(product))
                _products[product] = 0;

            _products[product]++;
        }
    }

    public void CaculateMoney()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_products == null || _products.Count == 0) return;

        int v = 0;
        foreach ((int id, int count) in _products)
        {
            if (id == -1) continue;
            var item = Items.GetItem(id);
            v += item.Value * count;
        }

        foreach (var player in Players)
        {
            if (Shippers.Contains(player.ActorNumber))
                ChangeMoney(player.ActorNumber, v / Shippers.Count);
        }
    }

    public void AddListener(Phase phase, bool onEntered, Action action)
    {
        if (_phaseEvents.TryGetValue((phase, onEntered), out Action entry))
        {
            entry += action;
            _phaseEvents[(phase, onEntered)] = entry;
        }
        else
        {
            _phaseEvents.Add((phase, onEntered), action);
        }
    }

    public void RemoveListener(Phase phase, bool onEntered, Action action)
    {
        if (_phaseEvents.TryGetValue((phase, onEntered), out Action entry))
        {
            entry -= action;
            _phaseEvents[(phase, onEntered)] = entry;
        }
    }
}
