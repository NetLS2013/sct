namespace BlockApp.Enum
{
    public enum StatusTransaction
    {
        Created = 1,
        UnconfirmedDeposit = 2,
        PartiallyDeposit = 3,
        FullDeposit = 4,
        UnconfirmedWithdraw = 5,
        PartiallyWithdraw = 6,
        FullWithdraw = 7,
        Deployed = 8
    }
}