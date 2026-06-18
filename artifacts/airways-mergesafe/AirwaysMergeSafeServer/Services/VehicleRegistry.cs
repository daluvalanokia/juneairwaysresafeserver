namespace AirwaysMergeSafeServer.Services;

public sealed record VehicleSpec(
    string   Type,
    string   Make,
    string   Model,
    string   Size,
    string   Icon,
    string[] Colors,
    float    LengthM,
    float    WidthM,
    float    HeightM
);

public static class VehicleRegistry
{
    public static readonly IReadOnlyList<VehicleSpec> All = new[]
    {
        // ── Sedans ──────────────────────────────────────────────────────────
        new VehicleSpec("sedan","Toyota","Camry","medium","🚗",
            new[]{"#c0392b","#2c3e50","#bdc3c7","#e8d5b7","#16a085"},4.2f,1.80f,0.85f),
        new VehicleSpec("sedan","Honda","Civic","small","🚗",
            new[]{"#3498db","#e74c3c","#2ecc71","#ecf0f1","#9b59b6"},3.9f,1.70f,0.78f),
        new VehicleSpec("sedan","Ford","Fusion","medium","🚗",
            new[]{"#2980b9","#7f8c8d","#c0392b","#f39c12","#1a1a2e"},4.3f,1.85f,0.87f),
        new VehicleSpec("sedan","Chevrolet","Malibu","medium","🚗",
            new[]{"#d35400","#8e44ad","#16a085","#bdc3c7","#2c2c54"},4.2f,1.82f,0.86f),
        new VehicleSpec("sedan","BMW","3 Series","medium","🚗",
            new[]{"#2c2c2c","#f5f5f5","#a0522d","#4169e1","#708090"},4.1f,1.80f,0.82f),
        new VehicleSpec("sedan","Mercedes","C-Class","medium","🚗",
            new[]{"#1a1a2e","#c0c0c0","#000080","#8b0000","#f5f5f5"},4.2f,1.82f,0.83f),

        // ── SUVs ────────────────────────────────────────────────────────────
        new VehicleSpec("suv","Ford","Explorer","large","🚙",
            new[]{"#1a1a2e","#4682b4","#8b4513","#696969","#006400"},4.9f,2.00f,1.25f),
        new VehicleSpec("suv","Chevrolet","Tahoe","large","🚙",
            new[]{"#1c1c1c","#f5f5dc","#556b2f","#8b0000","#4169e1"},5.1f,2.05f,1.35f),
        new VehicleSpec("suv","Toyota","RAV4","medium","🚙",
            new[]{"#cc0000","#1a1a2e","#808080","#f0f0f0","#2e8b57"},4.4f,1.86f,1.22f),
        new VehicleSpec("suv","Honda","CR-V","medium","🚙",
            new[]{"#b22222","#708090","#2f4f4f","#ffd700","#4682b4"},4.3f,1.84f,1.20f),
        new VehicleSpec("suv","Jeep","Wrangler","medium","🚙",
            new[]{"#ff4500","#2f4f4f","#f5f5f5","#ffd700","#1a1a2e"},4.0f,1.88f,1.40f),
        new VehicleSpec("suv","Tesla","Model X","large","🚙",
            new[]{"#f5f5f5","#cc0000","#1a1a2e","#808080","#000000"},5.0f,2.00f,1.28f),

        // ── Trucks ──────────────────────────────────────────────────────────
        new VehicleSpec("truck","Ford","F-150","large","🛻",
            new[]{"#1a1a2e","#cc0000","#696969","#f5f5dc","#006400"},5.5f,2.00f,1.45f),
        new VehicleSpec("truck","Chevrolet","Silverado","large","🛻",
            new[]{"#c0392b","#2c3e50","#bdc3c7","#8b4513","#1abc9c"},5.4f,2.00f,1.42f),
        new VehicleSpec("truck","Ram","1500","large","🛻",
            new[]{"#1a1a2e","#cc0000","#808080","#f5f5f5","#8b4513"},5.5f,2.02f,1.45f),
        new VehicleSpec("truck","Toyota","Tacoma","medium","🛻",
            new[]{"#cc0000","#696969","#f5f5dc","#2f4f4f","#1a1a2e"},4.9f,1.90f,1.38f),
        new VehicleSpec("truck","GMC","Sierra","large","🛻",
            new[]{"#1a1a2e","#8b0000","#bdc3c7","#5c4033","#2e8b57"},5.4f,2.01f,1.43f),

        // ── Motorcycles ─────────────────────────────────────────────────────
        new VehicleSpec("motorcycle","Harley-Davidson","Street Glide","medium","🏍",
            new[]{"#1a1a2e","#cc0000","#f5f5f5","#ffd700","#696969"},2.4f,0.80f,1.10f),
        new VehicleSpec("motorcycle","Honda","CBR600","small","🏍",
            new[]{"#cc0000","#1a1a2e","#f5f5f5","#ffa500","#0000cd"},2.0f,0.70f,1.05f),
        new VehicleSpec("motorcycle","Yamaha","R1","small","🏍",
            new[]{"#1a1a2e","#cc0000","#696969","#0000cd","#f5f5f5"},2.0f,0.68f,1.05f),
        new VehicleSpec("motorcycle","Kawasaki","Ninja 400","small","🏍",
            new[]{"#228b22","#1a1a2e","#ff4500","#f5f5f5","#696969"},2.1f,0.70f,1.08f),
        new VehicleSpec("motorcycle","Ducati","Monster","medium","🏍",
            new[]{"#cc0000","#1a1a2e","#f5f5f5","#ffd700","#696969"},2.2f,0.78f,1.10f),

        // ── Vans ────────────────────────────────────────────────────────────
        new VehicleSpec("van","Toyota","Sienna","large","🚐",
            new[]{"#f5f5f5","#696969","#cc0000","#1a1a2e","#ffd700"},5.1f,1.98f,1.75f),
        new VehicleSpec("van","Honda","Odyssey","large","🚐",
            new[]{"#c0c0c0","#1a1a2e","#cc0000","#f5f5f5","#696969"},5.0f,1.96f,1.72f),
        new VehicleSpec("van","Ford","Transit","large","🚐",
            new[]{"#f5f5f5","#1a1a2e","#ffd700","#cc0000","#808080"},5.5f,2.06f,2.00f),
        new VehicleSpec("van","Mercedes","Sprinter","large","🚐",
            new[]{"#f5f5f5","#c0c0c0","#1a1a2e","#808080","#696969"},5.6f,1.99f,2.20f),
    };
}
