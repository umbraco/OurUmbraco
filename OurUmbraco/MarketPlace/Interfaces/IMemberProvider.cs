namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IMemberProvider
    {
        IMember GetMemberById(int id);
        IMember GetMemberByEmail(string email);
        IMember GetCurrentMember();
        void SaveOrUpdate(IMember member);
    }
}
