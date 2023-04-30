using System.Collections;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.Graphics.Models.Chart;
using GrillBot.Common.Services.ImageProcessing.Models;

namespace GrillBot.App.Actions.Commands.Points.Chart;

public abstract class ChartBuilderBase<TData> where TData : IEnumerable
{
    protected ITextsManager Texts { get; }

    protected TData Data { get; private set; } = default!;
    protected IGuild Guild { get; private set; } = default!;

    protected ChartBuilderBase(ITextsManager texts)
    {
        Texts = texts;
    }

    public ChartBuilderBase<TData> Init(TData data, IGuild guild)
    {
        Data = data;
        Guild = guild;
        return this;
    }

    public async Task<ChartRequest> CreateRequestAsync(ChartsFilter filter, string locale)
    {
        var request = new ChartRequest();

        foreach (var chartFilterType in SplitChartsFilter(filter))
        {
            var requestData = ChartRequestBuilder.CreateCommonRequest();
            requestData.Data.TopLabel!.Text = CreateTopLabel(chartFilterType, locale);

            var dataSets = await CreateDatasetsAsync(chartFilterType).ToListAsync();
            requestData.Data.Datasets.AddRange(dataSets);

            request.Requests.Add(requestData);
        }

        return request;
    }

    private static IEnumerable<ChartsFilter> SplitChartsFilter(ChartsFilter filter)
    {
        if ((filter & ChartsFilter.Messages) != ChartsFilter.None)
            yield return ChartsFilter.Messages;

        if ((filter & ChartsFilter.Reactions) != ChartsFilter.None)
            yield return ChartsFilter.Reactions;

        if ((filter & ChartsFilter.Summary) != ChartsFilter.None)
            yield return ChartsFilter.Summary;
    }

    protected abstract string CreateTopLabel(ChartsFilter filter, string locale);
    protected abstract IAsyncEnumerable<Dataset> CreateDatasetsAsync(ChartsFilter filter);
}
