namespace apiPermissions.Elastic
{
	using apiPermissions.Models;
	using Elasticsearch.Net;
	using Nest;
	using System;
	using System.Threading.Tasks;


	public class ServiceElastic
	{
		private readonly ElasticClient _client;

		public ServiceElastic()
		{
			var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
				.DefaultIndex("permissions"); // Índice por defecto
			_client = new ElasticClient(settings);
		}

		public async Task IndexPermissionAsync(Permissions permission)
		{
			var response = await _client.IndexDocumentAsync(permission);
			if (!response.IsValid)
			{
				Console.WriteLine($"Error al indexar: {response.DebugInformation}");
			}
			else
			{
				Console.WriteLine("Documento indexado correctamente!.");
			}
		}


	}
}
