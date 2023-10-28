namespace libgwmapi;

public class GwmApiException : Exception
{
    public GwmApiException(string code, string description):base(description)
    {
        Code = code;
    }

    public string Code { get; }
}