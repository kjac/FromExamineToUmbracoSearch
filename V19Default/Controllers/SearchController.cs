using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Web.Common;

namespace V19Default.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase 
{
    private readonly ISearcher _searcher;
    private readonly UmbracoHelper _umbracoHelper;
    private readonly IPublishedContentQuery _publishedContentQuery;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;


    public SearchController(ISearcher searcher, UmbracoHelper umbracoHelper, IPublishedContentQuery publishedContentQuery, IPublishedContentTypeCache publishedContentTypeCache)
    {
        _searcher = searcher;
        _umbracoHelper = umbracoHelper;
        _publishedContentQuery = publishedContentQuery;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    [HttpGet("umbraco-search")]
    public async Task<IActionResult> SearchWithUmbracoSearch(string query)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return BadRequest("No query");
        }

        var pageContentTypeKey = _publishedContentTypeCache.Get(PublishedItemType.Content, "page").Key;
        
        var result = await _searcher.SearchAsync(
            indexAlias: Constants.IndexAliases.PublishedContent,
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
    public IActionResult SearchWithPublishedContentQuery(string query)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return BadRequest("No query");
        }

        var result = _publishedContentQuery.Search(query);

        var documentNames = result
            .Select(r => r.Content.Name)
            .ToArray();

        return Ok(documentNames);
    }
}