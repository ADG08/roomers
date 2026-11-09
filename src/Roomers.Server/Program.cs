using Roomers.Server;

var builder = WebApplication.CreateBuilder(args);

RoomConfig[] rooms = builder.Configuration.GetSection("Rooms").Get<RoomConfig[]>() ?? Array.Empty<RoomConfig>();

builder.Services.AddSingleton(new RoomRegistry(rooms));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<RoomEventHandler>();
builder.Services.AddHostedService<TcpServerService>();

var app = builder.Build();
app.Run();