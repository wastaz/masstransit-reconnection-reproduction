using System;
using System.Threading.Tasks;
using MassTransit;
using TestMassTransitReconnection.MessageContracts;

namespace TestMassTransitReconnection
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
                        
                        h.Heartbeat(1);
                          
                        h.UseCluster(x =>
                        {
                            x.Node("ipaddress1");
                            x.Node("ipaddress2");
                        });
                    });
                    
                    sbc.ReceiveEndpoint(host, "rmq-reconnection-test-app", x =>
                    {
                        x.Consumer(() => new MyRequestConsumer());
                        x.Consumer(() => new MyPublishConsumer());
                    });
                });
            
            await bus.StartAsync().ConfigureAwait(false);

            Console.WriteLine("Enter to shut down.");
            Console.ReadLine();

            await bus.StopAsync().ConfigureAwait(false);
        }
    }

    public class MyRequestConsumer : IConsumer<MyRequest>
    {
        public async Task Consume(ConsumeContext<MyRequest> context)
        {
            Console.WriteLine($"Received request payload: {context.Message.Payload}");
            await context.RespondAsync<MyResponse>(new
            {
                context.Message.Payload,
                ReceivedAt = DateTime.UtcNow
            }).ConfigureAwait(false);
        }
    }

    public class MyPublishConsumer : IConsumer<MyPublish>
    {
        public Task Consume(ConsumeContext<MyPublish> context)
        {
            Console.WriteLine($"Received publish payload: {context.Message.Payload}");
            return Task.CompletedTask;
        }
    }
}