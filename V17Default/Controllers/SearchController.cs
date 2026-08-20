using Examine;
using Examine.Search;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common;

namespace V17Default.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase 
{
    private readonly IExamineManager _examineManager;
    private readonly UmbracoHelper _umbracoHelper;
    private readonly IPublishedContentQuery _publishedContentQuery;

    public SearchController(IExamineManager examineManager, UmbracoHelper umbracoHelper, IPublishedContentQuery publishedContentQuery)
    {
        _examineManager = examineManager;
        _umbracoHelper = umbracoHelper;
        _publishedContentQuery = publishedContentQuery;
    }

    [HttpGet("examine-manager")]
    public IActionResult SearchWithExamineManager(string query)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return BadRequest("No query");
        }

        if (_examineManager.TryGetIndex(
                Constants.UmbracoIndexes.ExternalIndexName,
                out var index) is false)
        {
            return Problem("Index not found");
        }

        var ids = index
            .Searcher
            .CreateQuery("content")
            .NodeTypeAlias("page")
            .And()
            .Field("nodeName", query)
            .Execute(QueryOptions.SkipTake(0, 10))
            .Select(x => x.Id);

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