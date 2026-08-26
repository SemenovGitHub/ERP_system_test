using System.Reflection;
using ERP.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Tests;

public class TimeEntriesControllerHttpMethodsTests
{
    [Fact]
    public void Create_uses_http_post()
    {
        var method = typeof(TimeEntriesController).GetMethod(
            nameof(TimeEntriesController.Create));

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttribute<HttpPostAttribute>());
        Assert.Null(method.GetCustomAttribute<HttpPutAttribute>());
    }

    [Fact]
    public void Update_uses_http_put_with_id_route()
    {
        var method = typeof(TimeEntriesController).GetMethod(
            nameof(TimeEntriesController.Update));

        Assert.NotNull(method);

        var put = method.GetCustomAttribute<HttpPutAttribute>();

        Assert.NotNull(put);
        Assert.Equal("{id:guid}", put.Template);
        Assert.Null(method.GetCustomAttribute<HttpPostAttribute>());
    }
}
