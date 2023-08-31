using System.Diagnostics;
using System.Net.Http.Headers;
using static Android.Preferences.PreferenceActivity;

namespace PipelineApproval.Models;

public class HttpLoggingHandler : DelegatingHandler
{
    #region Fields

    private readonly string[] _types = new[] { "html", "text", "xml", "json", "txt", "x-www-form-urlencoded" };
    private readonly bool _shouldLogHttpResponseContent;

    #endregion

    #region Constructors

    /// <summary>
    /// Cria uma nova instância de <c>HttpLoggingHandler</c>.
    /// </summary>
    /// <param name="innerHandler">Manipulador de mensagem HTTP interno.</param>
    public HttpLoggingHandler(HttpMessageHandler innerHandler = null)
        : this(true, innerHandler)
    {
    }

    /// <summary>
    /// Cria uma nova instância de <c>HttpLoggingHandler</c>.
    /// </summary>
    /// <param name="shouldLogHttpResponseContent">Indica se deve ler e escrever log do conteúdo de resposta HTTP (informar <c>False</c> para conexão via SSE, por exemplo).</param>
    /// <param name="innerHandler">Manipulador de mensagem HTTP interno.</param>
    public HttpLoggingHandler(bool shouldLogHttpResponseContent, HttpMessageHandler innerHandler = null)
        : base(innerHandler ?? new HttpClientHandler())
    {
        _shouldLogHttpResponseContent = shouldLogHttpResponseContent;
    }

    #endregion

    #region Overrides

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var req = request;
        var id = Guid.NewGuid().ToString();
        var msg = $"[{id} - Request]";

        _ = Task.Run(() => LogRequestAsync(req, id));
        var start = DateTime.Now;

        var response = await base.SendAsync(request, cancellationToken)
                                                .ConfigureAwait(false);

        var end = DateTime.Now;
        _ = Task.Run(() => LogResponseAsync(req, response, id, msg, start, end));

        return response;
    }

    #endregion

    #region Private methods

    private async Task LogResponseAsync(HttpRequestMessage req, HttpResponseMessage response, string id, string msg, DateTime start, DateTime end)
    {
        try
        {
            Debug.WriteLine($"{msg} Duration: {end - start}\n" +
                                                     $"{msg}==========End==========");

            msg = $"[{id} - Response]";

            Debug.WriteLine($"{msg}=========Start=========\n" +
                                                     $"{msg} Thread: {GetCurrentThreadName()}");

            var resp = response;

            Debug.WriteLine($"{msg} {req.RequestUri.Scheme.ToUpper()}/{resp.Version} {(int)resp.StatusCode} {resp.ReasonPhrase}");

            foreach (var header in resp.Headers)
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");

            if (_shouldLogHttpResponseContent && resp.Content != null)
            {
                foreach (var header in resp.Content.Headers)
                    Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}");
            }

            Debug.WriteLine($"{msg}==========End==========\n");
        }
        catch
        {
            // Só ignora
        }
    }

    private async Task LogRequestAsync(HttpRequestMessage req, string msg)
    {
        try
        {
            var log = $"{msg}========Start==========\n" +
                      $"{msg} Thread: {GetCurrentThreadName()}\n" +
                      $"{msg} {req.Method} {req.RequestUri.PathAndQuery} {req.RequestUri.Scheme}/{req.Version}\n" +
                      $"{msg} Host: {req.RequestUri.Scheme}://{req.RequestUri.Host}";

            Debug.WriteLine(log);

            foreach (var header in req.Headers)
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}\n");

            if (req.Content is null)
                return;

            foreach (var header in req.Content.Headers)
                Debug.WriteLine($"{msg} {header.Key}: {string.Join(", ", header.Value)}\n");

            if (req.Content is StringContent || IsTextBasedContentType(req.Headers) || IsTextBasedContentType(req.Content.Headers))
            {
                var result = await req.Content.ReadAsStringAsync()
                                                    .ConfigureAwait(false);

                Debug.WriteLine($"{msg} Content:\n" +
                                                         $"{msg} {string.Join("", result.Cast<char>().Take(255))}...");
            }
        }
        catch
        {
            // Só ignora
        }
    }

    private bool IsTextBasedContentType(HttpHeaders headers)
    {
        IEnumerable<string> values;
        if (!headers.TryGetValues("Content-Type", out values))
            return false;
        var header = string.Join(" ", values).ToLowerInvariant();

        return _types.Any(t => header.Contains(t));
    }

    private string GetCurrentThreadName() => Thread.CurrentThread?.Name ?? "unknown";

    #endregion
}
