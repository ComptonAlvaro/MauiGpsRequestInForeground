using Android.Content;

//Aquí se está utilizando a los servicios básicos de location. Hay una versión de google
//Android.gms.Locations que parece ser que es más rápida y ofrece más cosas.
using Android.Locations;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;
using GTS.CMMS.RegistroHorario.UI.Maui.Localizacion;
//Con esto se define el namespace de Location, ya que se encuentra en Android.Locations
//y en Microsoft... Se quiere el de Android en este caso para utilizar los recursos nativos
//de Android.
using Location = Android.Locations.Location;



namespace GTS.CMMS.RegistroHorario.UI.Maui.Platforms.Android.Services;



public partial class LocationService : Java.Lang.Object, ILocationListener
{
    private LocationManager _androidLocationManager;
    private IMessenger _messenger;


    public LocationService(IMessenger paramMessenger)
    {
        _messenger = paramMessenger;
    }


    /// <summary>
    /// Inicia la escucha del servicio GPS.
    /// </summary>
    public async Task Iniciar()
    {
        await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        //OnStatusChanged($"LocationService->Initialize");
        _androidLocationManager ??= (LocationManager)global::Android.App.Application.Context.GetSystemService(Context.LocationService);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            //Se indica LocationAlways para que funcione siempre, tanto si la aplicación está en
            //primer plano como en segundo plano o cerrada.
            var status = await Permissions.RequestAsync<Permissions.LocationAlways>();

            //Si no se han dado permisos, no se hace nada.
            if (status != PermissionStatus.Granted)
            {
                //OnStatusChanged("Permission for location is not granted, we can't get location updates");
                return;
            }

            //Si no se ha conectado el GPS, no se hace nada.
            if (!_androidLocationManager.IsLocationEnabled)
            {
                //OnStatusChanged("Location is not enabled, we can't get location updates");
                return;
            }

            //Si el proveedor no está habilitado no se hace nada.
            if (!_androidLocationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                //OnStatusChanged("GPS Provider is not enabled, we can't get location updates");
                return;
            }

            //Si se tienen todos los recursos para obtener la localización, se solicita.
            _androidLocationManager.RequestLocationUpdates(LocationManager.GpsProvider, 800, 1, this);
        });
    }


    /// <summary>
    /// Detiene la escucha del servicio de GPS.
    /// </summary>
    public void Detener()
    {
        //OnStatusChanged($"LocationService->Stop");
        _androidLocationManager?.RemoveUpdates(this);
    }


    #region eventos para notificar al exterior
    //Aquí se definen los eventos para notificar al exterior la localización.

    public event EventHandler<LocationModel> LocationChanged;
    private void OnLocationChanged(LocationModel e)
    {
        LocationChanged?.Invoke(this, e);
    }

    public event EventHandler<string> StatusChanged;
    private void OnStatusChanged(string e)
    {
        StatusChanged?.Invoke(this, e);
    }
    #endregion eventos para notificar al exterior



    #region implementación del interfaz ILocationListener
    /// <summary>
    /// Método que se ejecuta cuando se recibe una nueva localización desde LocationManager.
    /// </summary>
    /// <param name="location"></param>
    public void OnLocationChanged(Location location)
    {
        if (location != null)
        {
            _messenger.Send<LocationModel>(new LocationModel(location.Latitude, location.Longitude, location.Bearing));
        }
    }

    public void OnProviderDisabled(string provider)
    {
        //inform your services that we stop getting updates
        //OnStatusChanged($"{provider} has been disabled");
    }

    public void OnProviderEnabled(string provider)
    {
        //inform your services that we start getting updates
        //OnStatusChanged($"{provider} now enabled");
    }

    public void OnStatusChanged(string provider, Availability status, Bundle extras)
    {
        //inform your services that provides status has been changed
        //OnStatusChanged($"{provider} change his status and now it's {status}");
    }
    #endregion implementación del interfaz ILocationListener
}
