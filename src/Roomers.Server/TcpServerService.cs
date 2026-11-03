using System.Net;
using System.Net.Sockets;

namespace Roomers.Server
{
    public class TcpServerService : BackgroundService
    {
        private readonly ILogger<TcpServerService> _logger;
        private readonly IConfiguration _config;
        private readonly RoomEventHandler _handler;

        public TcpServerService(ILogger<TcpServerService> logger, IConfiguration config, RoomEventHandler handler)
        {
            _logger = logger;
            _config = config;
            _handler = handler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int port = _config.GetValue<int>("Tcp:Port");

            TcpListener listener = new TcpListener(IPAddress.Any,port);
            listener.Start();
            _logger.LogInformation("Ecoute sur le port {Port}", port);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);
                    _ = HandleClientAsync(client, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            using (client)
            {
                try
                {
                    _logger.LogInformation("Client connecte : {Client}", client.Client.RemoteEndPoint);

                    using StreamReader reader = new StreamReader(client.GetStream());

                    string? line;
                    while ((line = await reader.ReadLineAsync(ct)) != null)
                    {
                        if (EventParser.TryParse(line, out RoomEvent? ev, out string? error))
                        {
                            if (_handler.TryHandle(ev!, out string? handleError))
                                _logger.LogInformation("Evenement accepte : {Event}", ev);
                            else
                                _logger.LogWarning("Evenement refuse : {Error} ({Event})", handleError, ev);
                        }
                        else
                            _logger.LogWarning("Message ignore : {Error}", error);
                    }

                    _logger.LogInformation("Client deconnecte");
                }
                catch (OperationCanceledException)
                {

                }
                catch (IOException e)
                {
                    _logger.LogWarning(e, "Connexion interrompue");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Erreur inattendue sur un client");
                }
            }
        }
    }
}
