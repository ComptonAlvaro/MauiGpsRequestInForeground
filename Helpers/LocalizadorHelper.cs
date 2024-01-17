using GTS.CMMS.RegistroHorario.Comun.DTO;
using GTS.CMMS.RegistroHorario.UI.Maui.Interfaces;

namespace GTS.CMMS.RegistroHorario.UI.Maui.Helpers;



public class LocalizadorHelper : ILocalizadorHelper
{
    public Task<Location?> GetUltimaLocalizacionConocidaAsync()
    {
        try
        {
            return Geolocation.Default.GetLastKnownLocationAsync();
        }
        catch(Exception ex)
        {
            return null;
        }
    }
    

    public async Task<Location?> GetLocalizacionAsync(GeolocationAccuracy paramPrecision)
    {
        try
        {
            GeolocationRequest miSolicitud = new GeolocationRequest(paramPrecision, TimeSpan.FromSeconds(30));
            return await Geolocation.Default.GetLocationAsync(miSolicitud);
        }
        catch(FeatureNotEnabledException)
        {
            throw new InvalidOperationException("No se ha podido obtener las coordenadas porque el GPS no está activado.\r\n\r\n"
                + "Compruebe que está activado e inténtelo de nuevo.");
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("");
        }
    }


    public async Task<LocalizacionDTO?> GetLocalizacionParaRegistroHorarioAsync()
    {
        //NOTA: si la primera vez que se pregunta se deniegan los permisos, la única opción ir a ajustes, permisos,
        //y permitir el uso de la localización.
        //O bien, desinstalar la aplicación e instalarla de nuevo.
        //En caso de que se diga solo esta vez, siempre que se ejecuta este comando, se preguntará, hasta que se indique
        //siempre o nunca.
        PermissionStatus miEstado = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        //Aquí el usuario ya habrá indicado de forma expresa que no quiere dar permisos a la localización.
        if (miEstado == PermissionStatus.Denied) return null;



        //Se obtiene la localización actual.
        //Toma como 1,5 segundos en obtener el dato, por lo que se utilizar la última localización conocida.
        //GeolocationRequest miRequest = new GeolocationRequest(GeolocationAccuracy.Low);
        //Location? miLocalizacion = await Geolocation.Default.GetLocationAsync(miRequest);
        Location? miLocalizacion = await Geolocation.Default.GetLastKnownLocationAsync();

        //Si la localización actual no es posible, se intenta encontrar la última conocida.
        //if(miLocalizacion == null)
        //{
        //    miLocalizacion = await Geolocation.Default.GetLastKnownLocationAsync();
        //}

        //Si es null, es porque la localización está desconectada, y esto pasará tanto si se utiliza
        //GetLocationAsync() como si se utiliza GetLastKnownLocationAsync(), por lo que si no está activado el GPS,
        //no coge la última posición conocida que pudiera haber el una caché.
        if (miLocalizacion == null) return new LocalizacionDTO(0, 0);



        //Solo se permite la posición desde el GPS del dispositivo, no desde uno externo.
        if (miLocalizacion.IsFromMockProvider == true)
        {
            throw new Exception("Se tiene que utilizar la localización del móvil.\r\n\r\n"
                + "- Si está utilizando un receptor GPS externo, desactívelo y active el GPS del móvil.");
        }


        return new LocalizacionDTO(miLocalizacion?.Latitude, miLocalizacion?.Longitude);
    }
}
