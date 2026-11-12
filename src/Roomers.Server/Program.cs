using Roomers.Server;

var builder = WebApplication.CreateBuilder(args);

RoomConfig[] rooms = builder.Configuration.GetSection("Rooms").Get<RoomConfig[]>() ?? Array.Empty<RoomConfig>();

builder.Services.AddSingleton(new RoomRegistry(rooms));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IRoomNotifier, SignalRRoomNotifier>();
builder.Services.AddSingleton<RoomEventHandler>();
builder.Services.AddHostedService<TcpServerService>();

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
            policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

app.UseCors();
app.MapHub<RoomsHub>("/hub/rooms");

app.Run();