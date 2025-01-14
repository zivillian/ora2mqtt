using libgwmapi.DTO.Vehicle;

namespace libgwmapi;

public partial class GwmApiClient
{
    public Task<Vehicle[]> AcquireVehiclesAsync(CancellationToken cancellationToken)
    {
        return GetAppAsync<Vehicle[]>("globalapp/vehicle/acquireVehicles", cancellationToken);
    }

    public Task<VehicleBasicsInfo> GetVehicleBasicsInfoAsync(string vin, CancellationToken cancellationToken)
    {
        return GetAppAsync<VehicleBasicsInfo>($"vehicle/vehicleBasicsInfo?vin={vin}&flag=true", cancellationToken);
    }

    public Task<VehicleStatus> GetLastVehicleStatusAsync(string vin, CancellationToken cancellationToken)
    {
        return GetAppAsync<VehicleStatus>($"vehicle/getLastStatus?vin={vin}&seqNo=", cancellationToken);
    }

    public Task ModifyVehicleRemoteCtlInfoAsync(ModifyVecicleRemoteCtl request, CancellationToken cancellationToken)
    {
        return PostH5Async("vehicle/modifyVehicleRemoteCtlInfo", request, cancellationToken);
    }

    public Task SendCmdAsync(SendCmd request, CancellationToken cancellationToken)
    {
        return PostAppAsync("vehicle/T5/sendCmd", request, cancellationToken);
    }

    public Task<RemoteCtrlResultT5[]> GetRemoteCtrlResultAsync(string seqNo, CancellationToken cancellationToken)
    {
        return GetAppAsync<RemoteCtrlResultT5[]>($"vehicle/getRemoteCtrlResultT5?seqNo={seqNo}", cancellationToken);
    }
}