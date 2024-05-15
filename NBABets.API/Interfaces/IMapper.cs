namespace NBABets.API
{
    public interface IMapper<in TFrom, out TTo>
    {
        TTo Map(TFrom item);
    }
}
