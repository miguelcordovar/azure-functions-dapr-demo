# .NET Azure Function (dotnet isolated mode) + DAPR

This example will demonstrate how to use Azure Functions programming model to integrate with Dapr components in dotnet isolated mode.

This example is based on [azure-functions-dapr-extension](https://github.com/Azure/azure-functions-dapr-extension/tree/master/samples/dotnet-isolated-azurefunction)

## Prerequisites

This sample requires you to have the following installed on your machine:

- Setup Dapr: Follow instructions to [download and install the Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/).
- [Install Azure Functions Core Tool](https://github.com/Azure/azure-functions-core-tools/blob/master/README.md#windows)

## Step 1 - Understand the Settings

Now that we've locally set up Dapr, clone the repo:

```bash
git clone https://github.com/miguelcordovar/azure-functions-dapr-demo.git
cd azure-functions-dapr-demo
```

In this folder, you will find `local.settings.json`, which lists a few app settings by the trigger/binding attributes.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=false",
    "AzureWebJobsSecretStorageType": "Files",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "StateStoreName": "statestore"
  }
}
```

Dapr components: This example uses default dapr components `(redis state store)` which gets installed in local when you perform `dapr init`.

You can find default dapr componentst at below location

**Windows:**

```command
C:\Users\<username>\.dapr
```

**Mac:**

```bash
/Users/<username>/.dapr
```

## Step 2 - Run Function App with Dapr

Run function host with Dapr:

Windows (requires Dapr 1.12+)

```command
dapr run -f .
```

Linux/Mac OS (requires Dapr 1.12+)

```bash
dapr run -f .
```

The command should output the dapr logs that look like the following:

```bash
ℹ️  Validating config and starting app "functionapp"
== APP - functionapp == MSBuild version 17.8.3+195e7f5a3 for .NET
ℹ️  Started Dapr with app id "functionapp". HTTP Port: 3500. gRPC Port: 65086
...
```

> **Note**: there are three ports in this service. The `--app-port`(3001) is where our function host listens on for any Dapr trigger. The `--dapr-http-port`(3501) is where Dapr APIs runs on as well as the  grpc port. The function port (default 7071) is where function host listens on for any HTTP triggred function using `api/{functionName}` URl path. All of these ports are configurable.
>

## Step 3 - Understand the Sample

### 1. State Management: Create New Order and Retrieve Order

Below is the Http Trigger function which internally does [Dapr State Management](https://docs.dapr.io/developing-applications/building-blocks/state-management/state-management-overview/) using Dapr invoke output binding.

```csharp
[Function("HttpDAPRStateOutput")]
[DaprStateOutput("%StateStoreName%", Key = "{key}")]
public static string Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "state/{key}")] HttpRequestData req,
      FunctionContext functionContext)
{
      var log = functionContext.GetLogger("HttpDAPRStateOutput");
      log.LogInformation("C# HTTP trigger function processed a request.");

      string requestBody = new StreamReader(req.Body).ReadToEnd();

      log.LogInformation("Persisting the state: {requestBody}", requestBody);

      return requestBody;
}
```

Now you can invoke this function by using the Dapr cli in a new command line terminal.

Mac

```bash
curl -X POST -H "Content-Type: application/json" -d "{ \"value\": {\"order\" : 1 }}" http://localhost:7071/api/state/order
{ "value": {"order" : 1 }}%
```

In your terminal window, you should see logs indicating that the message was received and state was updated:

```bash
== APP - functionapp == [2024-08-27T05:48:17.576Z] C# HTTP trigger function processed a request.
== APP - functionapp == [2024-08-27T05:48:17.577Z] Persisting the state: { "value": {"order" : 1 }}
== APP - functionapp == [2024-08-27T05:48:17.801Z] Executed 'Functions.HttpDAPRStateOutput' (Succeeded, Id=52320896-5ccb-4311-bf2f-4ff7ea9a50b0, Duration=229ms)
```

Now get the state you just saved:

```bash
curl http://localhost:3500/v1.0/state/statestore/order
{"order":1}%
```
