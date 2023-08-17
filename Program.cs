using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


const string connectionUri = "mongodb+srv://MyNotesUser:BxvCFtalAwZgx0y1@cluster0.rqwxsl8.mongodb.net/?retryWrites=true&w=majority";
var settings = MongoClientSettings.FromConnectionString(connectionUri);
// Set the ServerApi field of the settings object to Stable API version 1
settings.ServerApi = new ServerApi(ServerApiVersion.V1);
// Create a new client and connect to the server
var client = new MongoClient(settings);
var dabatase = client.GetDatabase("MyNotes");
var metricsCollection = dabatase.GetCollection<Metric>("metrics");

app.Urls.Add("http://0.0.0.0:8080");

app.MapGet("/metrics", () =>
{
    List<Metric> allMetrics = metricsCollection.Find(_ => true).ToList();
    return Results.Ok(allMetrics);
});

app.MapGet("/metrics/unique", () =>
{
    int count = metricsCollection.AsQueryable<Metric>().Select(m => m.Username).Distinct().Count();
    return Results.Ok(count);
});

app.MapGet("/metrics/{id:length(24)}", (string id) => {
    var metric = metricsCollection.Find(m => m.Id == id).SingleOrDefault();
    if (metric is null)
        return Results.NotFound();

    return Results.Ok(metric);
});

app.MapPost("/metrics", ([FromBody] MetricBody body) =>
{
    Metric metric = new()
    {
        Description = body.UserAction,
        Username = body.Username,
        CreatedAt = DateTime.UtcNow
    };

    metricsCollection.InsertOne(metric);
    return Results.Created("/metrics/someid", metric);
});

app.Run();

public class MetricBody
{
    public string? Username { get; set; }
    public string? UserAction { get; set; }
}

public class Metric
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}