using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Search.BackOffice.DependencyInjection;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Provider.Examine.DependencyInjection;

namespace V17UmbracoSearch.DependencyInjection;

public sealed class SiteComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder
            // add core services for search abstractions
            .AddSearchCore()
            // use Umbraco Search for backoffice search
            .AddBackOfficeSearch()
            // add the Examine search provider
            .AddExamineSearchProvider();

        // optimize server resources by disabling the (now-unused) V17 Examine indexes
        builder.DisableDefaultExamineIndexes();
    }
}