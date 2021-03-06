using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace KestrelTcpDemo
{
    public class MyEchoConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<MyEchoConnectionHandler> _logger;
        public MyEchoConnectionHandler(ILogger<MyEchoConnectionHandler> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            try
            {
                _logger.LogInformation(connection.ConnectionId + " connected");

                while (true)
                {
                    var result = await connection.Transport.Input.ReadAsync();
                    var buffer = result.Buffer;

                    foreach (var segment in buffer)
                    {
                        await connection.Transport.Output.WriteAsync(segment);
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }

                    connection.Transport.Input.AdvanceTo(buffer.End);
                }

                _logger.LogInformation(connection.ConnectionId + " disconnected");
            }
            finally
            {
                // Today, Kestrel expects the ConnectionHandler to complete the transport pipes
                // this will be resolved in a future release

                // We're done reading
                connection.Transport.Input.Complete();

                // We're done writing
                connection.Transport.Output.Complete();
            }
        }
    }
}