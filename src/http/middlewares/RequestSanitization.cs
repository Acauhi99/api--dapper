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
    // Padrões XSS mais específicos
    var xssPatterns = new[]
    {
      @"<script[^>]*>.*?</script>",
      @"javascript\s*:",
      @"on\w+\s*=\s*[""'][^""']*[""']",
      @"<iframe[^>]*>",
      @"<object[^>]*>",
      @"<embed[^>]*>"
    };

    // Padrões SQL Injection mais específicos
    var sqlInjectionPatterns = new[]
    {
      @";\s*drop\s+table",
      @";\s*drop\s+database",
      @";\s*delete\s+from",
      @";\s*insert\s+into",
      @"union\s+select",
      @"exec\s*\(\s*xp_",
      @"exec\s+xp_",
      @"--\s*$",
      @"/\*.*?\*/"
    };

    var allPatterns = xssPatterns.Concat(sqlInjectionPatterns);

    foreach (var pattern in allPatterns)
    {
      if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
      {
        Console.WriteLine($"Malicious pattern detected: {pattern}");
        Console.WriteLine($"Content: {content}");
        return true;
      }
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
