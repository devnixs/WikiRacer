using System.Text.Json;
using System.Text.Json.Serialization;
using WikiRacer.Api.Lobbies;
using WikiRacer.Application.Articles;
using WikiRacer.Application.Abstractions.Lobbies;
using WikiRacer.Application.Abstractions.Matches;
using WikiRacer.Application.Abstractions.Sessions;
using WikiRacer.Application.Abstractions.Tokens;
using WikiRacer.Application.Abstractions.Articles;
using WikiRacer.Application.Lobbies;
using WikiRacer.Application.Matches;
using WikiRacer.Contracts.Errors;
using WikiRacer.Infrastructure.Articles;
using WikiRacer.Infrastructure.Clock;
using WikiRacer.Infrastructure.Identifiers;
using WikiRacer.Infrastructure.Lobbies;
using WikiRacer.Infrastructure.Matches;
using WikiRacer.Infrastructure.Sessions;
using WikiRacer.Infrastructure.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<WikiRacer.Application.Abstractions.Clock.IClock, SystemClock>();
builder.Services.AddSingleton<WikiRacer.Application.Abstractions.Identifiers.IPublicLobbyIdGenerator, PublicLobbyIdGenerator>();
builder.Services.AddSingleton<ISessionTokenFactory, SessionTokenFactory>();
builder.Services.AddSingleton<ILobbyRepository, InMemoryLobbyRepository>();
builder.Services.AddSingleton<IMatchRepository, InMemoryMatchRepository>();
builder.Services.AddSingleton<IPlayerSessionStore, InMemoryPlayerSessionStore>();
builder.Services.AddSingleton<ILobbyService, LobbyService>();
builder.Services.AddSingleton<MatchService>();
builder.Services.AddSingleton<IPlayableArticleEligibilityScorer, RecognizableArticleEligibilityScorer>();
builder.Services.AddSingleton<IPlayableArticleSelector, CuratedPlayableArticleSelector>();
builder.Services.AddSingleton<WikipediaArticleService>();
builder.Services.AddSingleton<LobbyRealtimeHub>();
builder.Services.AddHttpClient<IWikipediaArticleClient, WikipediaArticleClient>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("WikiRacer/1.0 (https://github.com/wikiracer)");
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("frontend");
app.UseWebSockets();
app.MapControllers();
app.MapLobbyRealtimeEndpoints();

app.Run();

public partial class Program;
