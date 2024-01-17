/*
 * Record que se utilizará como argumentos de eventos cuando el listener de localización
 * quiera notificar una posición GPS.
 * 
 */



namespace MauiGpsRequestInForeground.Localizacion;



public record LocationModel(double Latitude, double Longitude, double Bearing)
{
    public double Latitude { get; } = Latitude;
    public double Longitude { get; } = Longitude;
    public double Bearing { get; } = Bearing;
}
