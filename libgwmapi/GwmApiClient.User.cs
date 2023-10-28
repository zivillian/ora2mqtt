using libgwmapi.DTO.User;

namespace libgwmapi;

public partial class GwmApiClient
{
    public Task<UserBaseInfo> GetUserBaseInfoAsync(CancellationToken cancellationToken)
    {
        if (!HasAccessToken) throw new InvalidOperationException("no accessToken provided");
        return GetH5Async<UserBaseInfo>("user/getUserBaseInfo", cancellationToken);
    }
}