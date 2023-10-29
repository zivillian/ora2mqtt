using libgwmapi.DTO.User;

namespace libgwmapi;

public partial class GwmApiClient
{
    public Task<UserBaseInfo> GetUserBaseInfoAsync(CancellationToken cancellationToken)
    {
        return GetH5Async<UserBaseInfo>("user/getUserBaseInfo", cancellationToken);
    }
}