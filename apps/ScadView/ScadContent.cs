namespace ScadView;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SharpWebview.Content;

public sealed class ScadContent : IWebviewContent
{
    private readonly WebApplication _webApp;
    private ScadApi _api = new();

    public ScadContent(int port = 0, bool activateLog = true)
    {
        _api.Filename = "test.scad";

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, port));

        if (!activateLog) {
            builder.Logging.ClearProviders();
        }

        _webApp = builder.Build();

        FileServerOptions fileServerOptions = new FileServerOptions{
            FileProvider = new PhysicalFileProvider(Path.Combine(System.Environment.CurrentDirectory, "app")),
            RequestPath = "",
            EnableDirectoryBrowsing = true,
        };

        _webApp.UseFileServer(fileServerOptions);
        _webApp.MapGet("/api/status", () => Results.Ok("XXX"));
        _webApp.MapPost("/api/save", (ScadApi.Content content) => _api.Save(content));
        _webApp.MapGet("/api/load", () => _api.Load());
        _webApp.Start();
    }

    public string ToWebviewUrl()
    {
        return _webApp.Urls.First();
    }
}
