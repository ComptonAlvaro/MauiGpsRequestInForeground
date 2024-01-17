/*
 * Define los métodos que serán de interés de acuerdo a las necesidades de localización.
 * 
 * Es útil porque la localización se necesitará tanto para crear registros horarios como para
 * las alarmas. De modo que se define el interfaz para poder utilizar dependency injection.
 * 
 */


using GTS.CMMS.RegistroHorario.Comun.DTO;



namespace GTS.CMMS.RegistroHorario.UI.Maui.Interfaces;



public interface ILocalizadorHelper
{
    /// <summary>
    /// Devuelve la última localización conocida. Es útil para las alarmas, porque lo que se
    /// necesita es la localización nada más.
    /// </summary>
    /// <returns></returns>
    public Task<Location?> GetUltimaLocalizacionConocidaAsync();

    public Task<Location?> GetLocalizacionAsync(GeolocationAccuracy paramPrecision);

    /// <summary>
    /// Obtiene la localización, de acuerdo a los permisos otorgados por el usuario.
    /// </summary>
    /// <returns>Null si el usuario no ha dado permisos para utilizar la localización.
    /// Una localización con coordenadas 0,0 si ha dado permisos para utilizar la localización pero no
    /// se ha podido obtener, bien porque no hay señal de GPS o bien porque la localización la tiene
    /// desactivada.
    /// Una localización posición con las coordenadas actuales si el usuario ha dado permiso, la
    /// localización está activada y se tiene señal de GPS.</returns>
    public Task<LocalizacionDTO?> GetLocalizacionParaRegistroHorarioAsync();
}
