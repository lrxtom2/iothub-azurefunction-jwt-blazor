namespace temperature.frontend.Shared
{
    public partial class WeatherForecast
    {
        public string? DeviceId { get; set; }

        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}