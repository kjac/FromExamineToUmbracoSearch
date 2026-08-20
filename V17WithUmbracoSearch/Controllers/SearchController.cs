using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Web.Common;

namespace V17UmbracoSearch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase 
{
    private readonly ISearcher _searcher;
    private readonly UmbracoHelper _umbracoHelper;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;

    public SearchController(ISearcher searcher, UmbracoHelper umbracoHelper, IPublishedContentTypeCache publishedContentTypeCache)
    {
        _searcher = searcher;
        _umbracoHelper = umbracoHelper;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    [HttpGet("umbraco-search")]
    public async Task<IActionResult> SearchWithUmbracoSearch(string query)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return BadRequest("No query");
        }

        // Umbraco Search only indexes the content type key for filtering, so let's resolve the alias from cache
        var pageContentTypeKey = _publishedContentTypeCache.Get(PublishedItemType.Content, "page").Key;
        
        var result = await _searcher.SearchAsync(
            indexAlias: Umbraco.Cms.Search.Core.Constants.IndexAliases.PublishedContent,
            filters: [
                // add filter for doctype "page"
                new KeywordFilter(
                    Umbraco.Cms.Search.Core.Constants.FieldNames.ContentTypeId,
                    [pageContentTypeKey.AsKeyword()],
                    Negate: false
                ),
                // use query to filter for content name
                new TextFilter(
                    Umbraco.Cms.Search.Core.Constants.FieldNames.Name,
                    [query],
                    Negate: false
                )
            ]
        );

        var ids = result.Documents.Select(document => document.Id);

        var documentNames = ids
            .Select(id => _umbracoHelper.Content(id)?.Name)
            .WhereNotNull()
            .ToArray();

        return Ok(documentNames);
    }

    [HttpGet("content-query")]
    public async Task<IActionResult> SearchWithPublishedContentQuery(string query)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return BadRequest("No query");
        }

        var result = await _searcher.SearchAsync(
            indexAlias: Umbraco.Cms.Search.Core.Constants.IndexAliases.PublishedContent,
            query: query,
            take: int.MaxValue
        );

        var ids = result.Documents.Select(document => document.Id);

        var documentNames = ids
            .Select(id => _umbracoHelper.Content(id)?.Name)
            .WhereNotNull()
            .ToArray();

        return Ok(documentNames);
    }
}