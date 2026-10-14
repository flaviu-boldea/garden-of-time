using Azure.Data.Tables;
using GardenOfTime.Web;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);

var storage = builder.Configuration["Storage:ConnectionString"];
if (string.IsNullOrEmpty(storage))
    builder.Services.AddSingleton<IBookingStore, InMemoryBookingStore>();
else
    builder.Services.AddSingleton<IBookingStore>(new TableBookingStore(new TableClient(storage, "bookings")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<BookingService>();

var aoai = builder.Configuration.GetSection("AzureOpenAI");
if (!string.IsNullOrEmpty(aoai["Endpoint"]) && !string.IsNullOrEmpty(aoai["ApiKey"]))
{
    var kb = builder.Services.AddKernel();
    kb.AddAzureOpenAIChatCompletion(aoai["Deployment"] ?? "gpt-4o-mini", aoai["Endpoint"]!, aoai["ApiKey"]!);
    kb.Plugins.AddFromType<GardenInfoPlugin>("Garden");
    kb.Plugins.AddFromType<BookingPlugin>("Bookings");
}

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["https://sites.google.com"])
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/availability/{date}", async (DateOnly date, BookingService s) => Results.Ok(new { date, available = await s.IsAvailableAsync(date) }));

app.MapPost("/api/bookings", async (BookingRequest r, BookingService s) =>
{
    try { return Results.Created($"/api/bookings/{(await s.CreateAsync(r)).Id}", new { message = "Booking created." }); }
    catch (BookingException e) { return Results.BadRequest(new { error = e.Message }); }
});

const string SystemPrompt = "You are the assistant of Grădina Timpului (Garden of Time) in La Boldici. Answer questions about the garden and the area using the provided functions, and help visitors book events: collect name, email, date, event type and guests, check availability, then create the booking. Reply in the user's language. Never invent facts.";

app.MapPost("/api/chat", async (ChatRequest req, IServiceProvider sp) =>
{
    var kernel = sp.GetService<Kernel>();
    if (kernel is null) return Results.Json(new { error = "Chatbot is not configured." }, statusCode: 503);
    if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length > 2000) return Results.BadRequest(new { error = "Invalid message." });

    var history = new ChatHistory(SystemPrompt);
    foreach (var m in (req.History ?? []).TakeLast(20))
    {
        if (m.Role == "user") history.AddUserMessage(m.Content);
        else if (m.Role == "assistant") history.AddAssistantMessage(m.Content);
    }
    history.AddUserMessage(req.Message);

    var result = await kernel.GetRequiredService<IChatCompletionService>().GetChatMessageContentAsync(
        history,
        new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
        kernel);
    return Results.Ok(new { reply = result.Content });
});

app.Run();

public record ChatMessage(string Role, string Content);
public record ChatRequest(string Message, List<ChatMessage>? History);

public partial class Program;
