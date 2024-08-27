using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Dapr;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DAPRFunctionProj.StateManagement.Functions;

public static class HttpDAPRStateOutput
{
  /// <summary>
  /// Example to use Dapr State Output binding to persist a new state into statestore
  /// </summary>
  [Function("HttpDAPRStateOutput")]
  [DaprStateOutput("%StateStoreName%", Key = "{key}")]
  public static string Run(
      [HttpTrigger(AuthorizationLevel.Function, "post", Route = "state/{key}")] HttpRequestData req,
      FunctionContext functionContext)
  {
    var log = functionContext.GetLogger("HttpDAPRStateOutput");
    log.LogInformation("C# HTTP trigger function processed a request.");

    string requestBody = new StreamReader(req.Body).ReadToEnd();

    log.LogInformation("Persisting the state: {requestBody}", requestBody);

    return requestBody;
  }
}
