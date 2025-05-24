using System.Text;
using Microsoft.IO;

namespace UserManagementAPI.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the request
        await LogRequest(context);

        // Capture and log the response
        await LogResponse(context);
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);

        var requestBody = ReadStreamInChunks(requestStream);

        var logMessage = $"""
            Http Request Information:
            Schema: {context.Request.Scheme}
            Host: {context.Request.Host}
            Path: {context.Request.Path}
            QueryString: {context.Request.QueryString}
            Request Body: {requestBody}
            """;

        _logger.LogInformation(logMessage);
        context.Request.Body.Position = 0;
    }

    private async Task LogResponse(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseStream = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseStream;

        await _next(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var logMessage = $"""
            Http Response Information:
            Schema: {context.Request.Scheme}
            Host: {context.Request.Host}
            Path: {context.Request.Path}
            StatusCode: {context.Response.StatusCode}
            Response Body: {responseBody}
            """;

        _logger.LogInformation(logMessage);

        await responseStream.CopyToAsync(originalBodyStream);
    }

    private static string ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream);
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;

        do
        {
            readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);

        return textWriter.ToString();
    }
}
