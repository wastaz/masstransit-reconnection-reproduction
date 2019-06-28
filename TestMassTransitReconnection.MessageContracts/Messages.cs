using System;

namespace TestMassTransitReconnection.MessageContracts
{
    public interface MyRequest
    {
        Guid Payload { get; }
        
    }

    public interface MyResponse
    {
        Guid Payload { get; }
        DateTime ReceivedAt { get; }
    }

    public interface MyPublish
    {
        Guid Payload { get; }
    }
    
}