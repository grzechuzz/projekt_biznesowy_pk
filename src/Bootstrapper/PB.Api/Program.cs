using PB.Modules.AttractionDefinition.Api;
using PB.Modules.AttractionDefinition.Infrastructure;
using PB.Modules.Catalog.Api;
using PB.Modules.Catalog.Infrastructure;
using PB.Modules.Preference.Api;
using PB.Modules.Preference.Infrastructure;
using PB.Modules.TripSelection.Api;
using PB.Modules.TripSelection.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(AttractionDefinitionModule).Assembly)
    .AddApplicationPart(typeof(CatalogModule).Assembly)
    .AddApplicationPart(typeof(PreferenceModule).Assembly)
    .AddApplicationPart(typeof(TripSelectionModule).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAttractionDefinitionModule();
builder.Services.AddCatalogModule();
builder.Services.AddPreferenceModule();
builder.Services.AddTripSelectionModule();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
