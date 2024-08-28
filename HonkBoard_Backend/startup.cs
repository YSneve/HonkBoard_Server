using HonkBoard_Backend.Core;
using HonkBoard_Backend.Core.Controller;
using HonkBoard_Backend.Core.Controller.Lobby;
using System.Net;
using HonkBoard_Backend.Core.Games.JustOne;
using HonkBoard_Backend.Core.Structures;
using Microsoft.Extensions.Options;

ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();
// Add services to the container.
//builder.Services.AddRazorPages();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();


//builder.Services.AddSingleton<JustOneInfo>();

builder.Services.Configure<List<GameInfo>>(builder.Configuration.GetSection("GamesParameters"));

builder.Services.Configure<WrapperOptions>(options =>
    {
        options.imagesServerAddress = builder.Configuration.GetValue<string>("ImagesServerAddress");
        options.wordsLists = builder.Configuration.GetSection("WordsCategories").Get<Dictionary<string, WordsListInfo>>();
    }
);

builder.Services.AddHttpClient<DatabaseWrapper>();
builder.Services.AddSingleton<IDataAccess, DatabaseWrapper>();

builder.Services.AddSingleton<IUsersHandler, HubHandler>();
builder.Services.AddSingleton<Hub>();

builder.Services.AddSingleton<LobbyUsersHandler>();
builder.Services.AddSingleton<LobbySocket>();

builder.Services.AddSingleton<WordsController>();


builder.Services.AddSingleton<ConnectionsHandler>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.Configure<ApiOptions>(options => options.maxIds = builder.Configuration.GetValue<int>("MaxIds"));


var app = builder.Build();
// Configure the HTTP request pipeline.


//app.UseExceptionHandler("/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

app.UseSwagger();
app.UseSwaggerUI();
//app.UseHsts();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();
app.MapRazorPages();

app.MapHub<Hub>("/just-one");
app.MapHub<LobbySocket>("/lobby");


app.UseAuthorization();


app.Run();