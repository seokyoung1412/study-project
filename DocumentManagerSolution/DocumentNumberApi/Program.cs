using DocumentNumberApi.Data;
using DocumentNumberApi.Models;
using DocumentNumberApi.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- MariaDB DB Context 등록 ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        options => options.SchemaBehavior(MySqlSchemaBehavior.Ignore)
    );
});

// DB 기반 서비스 등록
builder.Services.AddScoped<DocumentNumberService>();
// ------------------------------------------

var app = builder.Build();

// 🚀 1. 데이터베이스 마이그레이션 자동 적용 (추가된 부분) 🚀
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ------------------------------------------
// 문서 번호 생성 API 엔드포인트 추가
// ------------------------------------------

app.MapGet("/api/documentnumber/{documentType}/{departmentCode}", async (
    string documentType,
    string departmentCode,
    DocumentNumberService service) =>
{
    // 1. 시퀀스 번호 가져오기 및 증가
    var nextNumber = await service.GetNextSequenceNumberAsync(documentType, departmentCode);

    // 2. 날짜 부분 생성 (년월일: yyMMdd)
    string datePart = DateTime.Now.ToString("yyMMdd");

    // 3. 순번 형식 지정 (3자리: 001)
    string sequencePart = nextNumber.ToString("D3");

    // 4. 문서 번호 형식: [문서종류]-[부서코드]-[년월일]-[순번]
    var documentNumber = $"{documentType}-{departmentCode}-{datePart}-{sequencePart}";

    // 응답
    return Results.Ok(new { DocumentNumber = documentNumber, Sequence = nextNumber });
})
.WithName("GetNextDocumentNumber")
.WithOpenApi();


// 기존 WeatherForecast 코드 제거 또는 유지 (여기서는 제거하지 않고 그대로 둠)
// ...
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
// ...


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}