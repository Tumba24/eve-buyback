using Microsoft.AspNetCore.Mvc.Formatters;

namespace EveBuyback.App;

public class PlainTextInputFormatter : InputFormatter
{
    private const string ContentType = "text/plain";

    public PlainTextInputFormatter()
    {
        SupportedMediaTypes.Add(ContentType);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        var request = context.HttpContext.Request;
        using (var reader = new StreamReader(request.Body))
        {
            var content = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(content);
        }
    }

    public override bool CanRead(InputFormatterContext context)
    {
        var contentType = context.HttpContext.Request.ContentType;
        return contentType?.StartsWith(ContentType) ?? false;
    }
}