using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using libgwmapi.DTO.UserAuth;
using libgwmapi.DTO.Vehicle;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace libgwmapi.test;

public class GwmApiClientTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GwmApiClientTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task CanGetCountrys()
    {
        var client = GetClient();
        var countries = await client.GetCountrysAsync(CancellationToken.None);
        Assert.NotNull(countries);
        Assert.NotNull(countries.G);
        Assert.Contains(countries.G, x => x.CountryName == "Germany");
    }

    [Fact]
    public async Task CanGetCustomerServicePhone()
    {
        var client = GetClient();
        var customerServicePhone = await client.GetCustomerServicePhoneAsync("DE", CancellationToken.None);
        Assert.NotNull(customerServicePhone);
        Assert.Equal("08009886044", customerServicePhone.Phone);
    }

    [Fact]
    public async Task CanLoginAccount()
    {
        var client = GetClient();
        var request = new LoginAccountRequest
        {
            Account = "ora@example.com",
            Country = "DE",
            IsEncrypt = false,
            DeviceId = "<known device id>",
            Model = "ora2mqtt",
            Password = "<password>"
        };
        var response = await client.LoginAccountAsync(request, CancellationToken.None);
        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        _testOutputHelper.WriteLine(System.Text.Json.JsonSerializer.Serialize(response));
    }

    [Fact]
    public async Task CanGetSmsCode()
    {
        var client = GetClient();
        var request = new GetSmsCode { Email = "ora@example.com" };
        await client.GetSmsCodeAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task CanLoginWithSms()
    {
        var client = GetClient();
        var request = new LoginWithSmsRequest
        {
            Email = "ora@example.com",
            Country = "DE",
            DeviceId = "8453F1C6-29E9-421C-8C28-7E860C94527D",
            Model = "ora2mqtt",
            SmsCode = "<code>"
        };
        var response = await client.LoginWithSmsAsync(request, CancellationToken.None);
        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(response));
    }

    [Fact]
    public async Task CanGetUserBaseInfo()
    {
        var client = GetAuthenticatedClient();
        client.Country = "DE";
        var info = await client.GetUserBaseInfoAsync(CancellationToken.None);
        Assert.NotNull(info);
        Assert.NotNull(info.LastName);
    }

    [Fact]
    public async Task CanAcquireVehicles()
    {
        var client = GetAuthenticatedClient();
        var vehicles = await client.AcquireVehiclesAsync(CancellationToken.None);
        Assert.NotNull(vehicles);
        Assert.NotEmpty(vehicles);
        var vehicle = vehicles[0];
        Assert.NotNull(vehicle);
        Assert.NotNull(vehicle.AgreementVersion);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(vehicles));
    }

    [Fact]
    public async Task CanGetVehicleBasicsInfo()
    {
        var client = GetAuthenticatedClient();
        var info = await client.GetVehicleBasicsInfoAsync("<vin>", CancellationToken.None);
        Assert.NotNull(info);
        Assert.NotNull(info.Config);
        Assert.NotNull(info.Config.Vin);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(info));
    }

    [Fact]
    public async Task CanGetVehicleStatus()
    {
        var client = GetAuthenticatedClient();
        var status = await client.GetLastVehicleStatusAsync("<vin>", CancellationToken.None);
        Assert.NotNull(status);
        Assert.NotNull(status.Items);
        Assert.NotNull(status.Items[0].Code);
        _testOutputHelper.WriteLine(JsonSerializer.Serialize(status));
    }

    [Fact]
    public async Task CanCheckSecurityPassword()
    {
        var client = GetAuthenticatedClient();
        var request = new CheckSecurityPassword("<pin>");
        await client.CheckSecurityPasswordAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task CanModifyVehicleRemoteCtl()
    {
        var client = GetAuthenticatedClient();
        var request = new ModifyVecicleRemoteCtl
        {
            AirConditionerTemperature = "30",
            AirConditionerTime = "200",
            Vin = "<vin>"
        };
        await client.ModifyVehicleRemoteCtlInfoAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task CanSendCmd()
    {
        var client = GetAuthenticatedClient();
        var request = new SendCmd
        {
            Instructions = new SendCmdInstruction
            {
                X04 = new Instruction0x04
                {
                    AirConditioner = new AirConditionerInstruction
                    {
                        OperationTime = "5",
                        SwitchOrder = "1",
                        Temperature = "20"
                    }
                }
            },
            RemoteType = "0",
            SecurityPassword = new CheckSecurityPassword("<pin>").Md5Hash,
            Type = 2,
            Vin = "<vin>"
        };
        _testOutputHelper.WriteLine(request.SeqNo);
        await client.SendCmdAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task CanGetRemoteCtrlResult()
    {
        var client = GetAuthenticatedClient();
        var response = await client.GetRemoteCtrlResultAsync("263b1c60ae1e4eb6b28e6d680b37b7931234", CancellationToken.None);
        Assert.NotNull(response);
        Assert.NotEmpty(response);
        var first = response[0];
        Assert.NotNull(first);
        Assert.NotNull(first.HwCommandId);
    }

    [Fact]
    public async Task CanAddAppDeviceInfo()
    {
        var client = GetClient();
        var request = new AddAppDevice
        {
            DeviceId = Guid.NewGuid().ToString("N")
        };
        await client.AddAppDeviceInfoAsync(request, CancellationToken.None);
    }

    [Fact]
    public async Task CanRefreshToken()
    {
        var client = GetClient();
        var request = new RefreshTokenRequest
        {
            AccessToken = "<accessToken>",
            RefreshToken = "<refreshToken>",
            DeviceId = "<deviceId>"
        };
        await client.RefreshTokenAsync(request, CancellationToken.None);
    }

    private GwmApiClient GetClient()
    {
        var certHandler = new CertificateHandler();
        var httpHandler = new HttpClientHandler();
        httpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        using (var cert = certHandler.CertificateWithPrivateKey)
        {
            var pkcs12 = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
            httpHandler.ClientCertificates.Add(pkcs12);
        }
        //add intermediates to local cert store, so they get sent with the request
        //https://github.com/dotnet/runtime/issues/55368#issuecomment-876775809
        //TODO check how this behaves on linux and mac
        var store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        var certs = certHandler.Chain;
        foreach (var cert in certs)
        {
            if (cert.Issuer != cert.Subject)
            {
                store.Add(cert);
            }
        }
        return new GwmApiClient(new HttpClient(), new HttpClient(httpHandler), new NullLoggerFactory());
    }

    private GwmApiClient GetAuthenticatedClient()
    {
        var client = GetClient();
        client.SetAccessToken("<accessToken>");
        client.Country = "<country>";
        return client;
    }
}