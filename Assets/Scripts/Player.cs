using UnityEngine;

namespace Game.Data
{
    public class Player
    {
        public readonly int ActorNumber;
        public string Name;
        public Sprite Icon;
        public bool hasSelected;
        public int AreaIndex;
        public int Money;
        public bool IsLeader;
        public int[] Inventory;
        public int[] Ship;
        public bool HasShipTicket;
        public bool HasReceivedTravelReward;
        public bool HasSubmittedShipment;
        public bool IsActionFinished { get; set; }
        public int IconNum;

        public Player(int actorNumber, string name, int money)
        {
            ActorNumber = actorNumber;
            Name = name;
            Money = money;
            Inventory = new int[6] { -1, -1, -1, -1, -1, -1 };
            Ship = new int[3] { -1, -1, -1 };
        }
    }
}
