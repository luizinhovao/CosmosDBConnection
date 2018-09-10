using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBConnection.Constants;
using CosmosDBConnection.CosmosDB;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CosmosDBConnection.Functions
{
	public static class GetIntentAnswer
	{
		[FunctionName("GetIntentAnswer")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("GetIntentAnswer message received");
			try
			{
				string intentName = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "intent", true) == 0)
					.Value;

				if (string.IsNullOrWhiteSpace(intentName))
					throw new Exception("Missing intentName data");

				CosmoOperation cosmoOperation = await CosmosDBOperations.QueryDBAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = $"SELECT * FROM c WHERE c.type = 'IntentAnswer' AND c.intent = '{intentName}'"
				});

				return req.CreateResponse(HttpStatusCode.OK, cosmoOperation.Results as object);
			}
			catch (Exception ex)
			{
				log.Error("Error: ", ex);
				return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
			}
		}
	}
}