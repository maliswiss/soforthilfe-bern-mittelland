using Soforthilfe.Dienste;

var builder = WebApplication.CreateBuilder(args);

// Schicht Controller: Schnittstellen werden über Attribute in den Controllern registriert.
builder.Services.AddControllers();

// Fachdienste inklusive Datenzugriff. Die Schnittstelle kennt nur die Dienstschicht.
builder.Services.AddDienste();

var app = builder.Build();

// Alle Controller-Routen aktivieren, zum Beispiel GET /api/info.
app.MapControllers();

app.Run();
