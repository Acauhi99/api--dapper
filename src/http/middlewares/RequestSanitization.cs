using System.Text.RegularExpressions;
using System.Text;

namespace api__dapper.http.middlewares;

public class RequestSanitization(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public async Task InvokeAsync(HttpContext context)
  {
    context.Request.EnableBuffering();

    if (context.Request.ContentType?.ToLower().Contains("application/json", StringComparison.CurrentCultureIgnoreCase) == true)
    {
      using var reader = new StreamReader(
        context.Request.Body,
        Encoding.UTF8,
        detectEncodingFromByteOrderMarks: false,
        leaveOpen: true);

      var body = await reader.ReadToEndAsync();
      context.Request.Body.Position = 0;

      if (ContainsMaliciousContent(body))
      {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { message = "Request contains potentially harmful content" });
        return;
      }
    }

    await _next(context);
  }

  private static bool ContainsMaliciousContent(string content)
  {
    var xssPatterns = new[]
    {
      @"<script>", @"javascript:", @"onload=", @"onerror=", @"onclick="
    };

    var sqlInjectionPatterns = new[]
    {
      @"--", @";--", @";", @"/*", @"*/", @"@@", @"@@identity",
      @"exec[(\s)]+(xp_)", @"exec[(\s)]+(@)", @"select.*from", @"insert.*into",
      @"drop\s+table", @"drop\s+database", @"delete\s+from", @"update\s+.+\s+set"
    };

    var allPatterns = xssPatterns.Concat(sqlInjectionPatterns);

    foreach (var pattern in allPatterns)
    {
      if (Regex.IsMatch(content.ToLower(), pattern, RegexOptions.IgnoreCase))
        return true;
    }

    return false;
  }
}

public static class RequestSanitizationMiddlewareExtensions
{
  public static IApplicationBuilder UseRequestSanitization(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<RequestSanitization>();
  }
}
