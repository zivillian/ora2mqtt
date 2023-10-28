using System.Net.Http.Json;
using libgwmapi.DTO.Country;
using libgwmapi.DTO.Vehicle;

namespace libgwmapi;

public partial class GwmApiClient
{
    public Task<Countrys> GetCountrysAsync(CancellationToken cancellationToken)
    {
        return GetH5Async<Countrys>("country/getCountrys", cancellationToken);
    }
}