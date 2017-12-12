namespace IntelliTect.Coalesce
{
    public interface IAuthorizable
    {
        (bool Authorized, string Message) IsAuthorized();
    }
}