using System;
using System.Threading.Tasks;
using MassTransit;
using TestMassTransitReconnection.MessageContracts;

namespace TestMassTransitReconnection.Client
{
    public class Program
    {
        
        public static async Task Main(string[] args)
        {
            var bus =
                Bus.Factory.CreateUsingRabbitMq(sbc =>
                {
                    // We have tried both
                    // * Connecting with UseCluster
                    // * Connecting against single node in cluster
                    // * Connecting against load balancer address

                    var host = sbc.Host(new Uri("rabbitmq://rmqcluster/qa"), h => // qa is our virtual host
                    {
                        h.Username("guest");
                        h.Password("guest");
                        
                        h.UseCluster(x =>
                        {
                            x.Node("ipaddress1");
                            x.Node("ipaddress2");
                        });
                    });
                });
            
            await bus.StartAsync().ConfigureAwait(false);

            Console.WriteLine("r for request, p for publish, q to quit");
            while (true)
            {
                var line = Console.ReadLine();
                line = line?.Trim() ?? string.Empty;

                if (line == "q")
                {
                    break;
                }

                if (line == "r")
                {
                    await DoRequest(bus).ConfigureAwait(false);
                }

                if (line == "p")
                {
                    await DoPublish(bus).ConfigureAwait(false);
                }
            }

            await bus.StopAsync().ConfigureAwait(false);
        }

        private static async Task DoPublish(IBus bus)
        {
            var payload = Guid.NewGuid();
            Console.WriteLine($"Publishing with payload: {payload}");
            try
            {
                await bus.Publish<MyPublish>(new
                {
                    Payload = Guid.NewGuid()
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown. Type: {e.GetType().Name} Message: {e.Message}");
            }
        }

        private static async Task DoRequest(IBus bus)
        {
            var client = new MessageRequestClient<MyRequest, MyResponse>(
                bus, 
                new Uri("rabbitmq://rmqcluster/qa/rmq-reconnection-test-app"), 
                TimeSpan.FromSeconds(5), 
                TimeSpan.FromSeconds(5));

            var id = Guid.NewGuid();
            Console.WriteLine($"Sending payload {id}");
            try
            {
                var response = await client.Request(new MyRequestImpl() { Payload = id }).ConfigureAwait(false);
                Console.WriteLine($"Received response payload {response.Payload}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown. Type: {e.GetType().Name} Message: {e.Message}");
            }
        }
        
        private class MyRequestImpl : MyRequest
        {
            public Guid Payload { get; set; }
        } 
    }
}