using System.Text.Json.Serialization;

namespace libgwmapi.DTO.Vehicle;

public class SendCmd
{
    [JsonPropertyName("instructions")]
    public SendCmdInstruction Instructions { get; set; }
    [JsonPropertyName("remoteType")]
    public string RemoteType { get; set; }

    [JsonPropertyName("securityPassword")]
    public string SecurityPassword { get; set; }

    [JsonPropertyName("seqNo")]
    /*  public static final String i() {
     *    StringBuilder stringBuilder = new StringBuilder();
     *    String str = UUID.randomUUID().toString();
     *    Intrinsics.checkNotNullExpressionValue(str, "randomUUID().toString()");
     *    stringBuilder.append(StringsKt__StringsJVMKt.replace$default(str, "-", "", false, 4, null));
     *    stringBuilder.append("1234");
     *    return stringBuilder.toString();
     * }*/
    public string SeqNo { get; } = Guid.NewGuid().ToString().Replace("-", String.Empty) + "1234";

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("vin")]
    public string Vin { get; set; }
}

public class SendCmdInstruction
{
    [JsonPropertyName("0x04")]
    public Instruction0x04 X04 { get; set; }
}

public class Instruction0x04
{
    [JsonPropertyName("airConditioner")]
    public AirConditionerInstruction AirConditioner { get; set; }
}

public class AirConditionerInstruction
{
    [JsonPropertyName("operationTime")]
    public string OperationTime { get; set; }

    [JsonPropertyName("switchOrder")]
    public string SwitchOrder { get; set; }

    [JsonPropertyName("temperature")]
    public string Temperature { get; set; }
}