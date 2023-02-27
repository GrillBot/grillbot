using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.App.Actions.Commands.Unverify;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Unverify;

[TestClass]
public class SelfUnverifyKeepablesTests : CommandActionTest<SelfUnverifyKeepables>
{
    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override SelfUnverifyKeepables CreateInstance()
    {
        var apiAction = new GetKeepablesList(new ApiRequestContext(), DatabaseBuilder);
        return InitAction(new SelfUnverifyKeepables(apiAction, TestServices.Texts.Value));
    }

    private async Task InitDataAsync()
    {
        var groups = new[] { "1BIT", "2BIT", "3BIT", "4BIT", "1MIT", "2MIT", "3MIT", "_" };
        var subjects = new[]
        {
            "AEU", "AIT", "ALG", "APD", "ASD", "ATA", "BAN1-3", "BAN4", "BAYA", "BAYA-PRIVATE", "BID", "BIN", "BIO", "BMS", "C2P", "C3P", "CCS", "CPS", "ACS",
            "OAC", "ZSA", "DJA", "DMA", "1DP", "C-T", "K1D", "P-S", "ZZE", "UDE", "VDE", "VOF", "ADF", "CEF", "IKF", "ITF", "LP-PRIVATE", "FVS", "FYO", "EGA", "LGJA",
            "GMU", "GUX", "GZN", "HKO", "HSC", "EHU", "MAN", "ITN", "IHV", "RI1", "CI2", "CIA", "MIA", "NIB", "SIB", "TEI", "CPI", "CSI", "CUL", "ICU", "ZID", "DLI",
            "DDZ", "IDF", "1ID", "F2I", "FAI", "IFA", "NIF", "EAI", "FSI", "IPD", "IIZ", "IJA", "IJ", "CIS", "KPTI", "LIIM", "A", "EIMFIM", "IEIN", "IIP", "1IP2I", "P3IPAI",
            "PMAI", "PP", "KIP", "SIPS", "OIPZI", "SA-1", "ISA", "-2IS", "A-3I", "CI", "SDI", "SJIS", "MIS", "PITP", "ITSI", "TTEIT", "WI", "TYIU", "CE", "I", "UM", "IIA", "V108",
            "IV", "GIVHI", "VP1I", "VP", "2I", "VS", "W1I", "W2I", "W5", "IZAI", "ZEP", "IZF", "II", "GIZG-", "P", "RIV", "ATE", "IZMAIZ", "SLIZV", "JA3J", "A6", "DJAD", "J",
            "S1KK", "OKNN", "KRD", "KR", "Y", "LOGM", "BAMI", "DM", "LDMM", "DMSDM", "TIAM", "ULM", "ULEM", "Z", "DNAV", "NSBO", "PDOP", "ORIDPB", "DPCGPC", "SPD", "B", "EPD",
            "DPDIP", "SXP", "SEP", "FTDP", "G", "DPH", "D+PK", "SPMA", "PND", "PSS", "SP", "O", "VA", "PP1PP", "2P", "M", "PD", "RETRG", "DRO", "BA", "TS", "SAVS", "COSE", "SEPASEPE",
            "SID", "SL", "S", "OAS", "TSO", "DSP", "PSR", "IS", "SCR", "PSUR", "SUR-", "PRIVA", "TET", "ADTAM", "ATHETIDT", "JDT", "KDTO", "ITOI-P", "RIVAT", "EUXIA", "VD",
            "DVGEV", "GEEVI", "NVIZ", "AVKDVN", "DV", "PDV", "YF", "VYP", "AWAPZ", "HAZPD", "ZPJA", "ZP", "OZ", "POE", "ZPD", "X", "RE", "ZZD", "ZZN"
        };

        var keepables = groups.SelectMany(
            group => subjects.Select(subject => new SelfunverifyKeepable { GroupName = group, Name = subject })
        );

        await Repository.AddCollectionAsync(keepables);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ListAsync_WithData()
    {
        await InitDataAsync();

        var result = await Instance.ListAsync();

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Fields.Length > 0);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ListAsync_WithoutData()
    {
        await Instance.ListAsync();
    }
}
