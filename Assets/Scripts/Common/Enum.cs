namespace Game.Enum
{
    public enum Phase
    {
        InLobby,
        TravelSelection,
        Travel,
        GoHome,
        Vote,
        VoteResult,
        Feed,    
        RoundResult,
        GameResult,
        Calculate,
        VoteWait,
        FeedWait,
    }

    public enum PacketType
    {
        TravelSelection,
        ChangeMoney,
        GetItem,
        ItemSubmit,
        Chat,
        VoteConfirm,
        AsyncPhase,
        GiveItem,
        SetLeader,
        ChangeIcon,
        UpdatePhase,
    }
}