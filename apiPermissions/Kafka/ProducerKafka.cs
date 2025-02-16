namespace apiPermissions.Kafka
{
	using Confluent.Kafka;
	using System;
	using System.Threading.Tasks;
	public class ProducerKafka
	{
		private readonly string _topic;
		private readonly IProducer<Null, string> _producer;

		public ProducerKafka(string bootstrapServers, string topic)
		{
			_topic = topic;
			var config = new ProducerConfig { BootstrapServers = bootstrapServers };
			_producer = new ProducerBuilder<Null, string>(config).Build();
		}

		public async Task SendMessageAsync(string message)
		{
			await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
			Console.WriteLine($"Mensaje enviado====>: {message}");
		}

	}
}
