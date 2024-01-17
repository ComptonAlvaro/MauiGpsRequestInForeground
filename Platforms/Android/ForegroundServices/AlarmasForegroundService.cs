using Android.App;
using Android.Content;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;
using GeolocatorPlugin.Abstractions;
using GeolocatorPlugin;
using Plugin.LocalNotification;



//Tiene que pertenecer al namespace raíz para poder tener acceso a Android.Content y otros
//recursos.
namespace MauiGpsRequestInForeground.Maui;



[Service]
public class AlarmasForegroundService : Service, IDisposable
{
    #region variables de clase
    private IServicioAlarmas _servicioAlarmas;
    private LocationService _locationListener;

    //Se sabe que es el 1, en canal se crea en el MainActivity.
    private string NOTIFICATION_CHANNEL_ID = "1";
    private int NOTIFICATION_ID = 1;
    #endregion variables de clase



    #region constructores
    //Se necesita el constructor por defecto porque es como trabaja android, no se pueden utilizar
    //constructores con parámetros para crear el foreground.

    //Para instanciar los recursos neceasrios, como el servicio de alarmas, se hará en el método
    //OnCreate().
    #endregion constructores



    #region creación del foreground
    /// <summary>
    /// Crea una notificación y el canal necesario e iniciar el foreground service.
    /// </summary>
    /// <remarks>Para que un foreround service no hace falta crear una notificación, por ejemplo
    /// si lo que se quiere es un servicio para sincronizar datos con una base de datos. Pero
    /// en este caso sí se quiere notificar al usuario, para avisarle de que necesita crear
    /// un registro horario, por lo que es creará la notificación.</remarks>
    private void StartForegroundService()
    {
        //NOTA: el canal se crea en el MainActivity, porque se tiene que utilizar uno a nivel
        //de aplicación y además dependiendo de la versión de Android se tiene que crear o no.
        //Eso no responsabilidad del foreground service.


        //Para crear una notificación nueva se tiene que crear el builder.
        //Una vez que se tenga la instancia, se tiene que pasar al Manager para publicarla.
        Notification notification = new Notification.Builder(this, NOTIFICATION_CHANNEL_ID)
            //este texto es lo que se mostrará en la barra de notificaciones del móvil cuando se
            //inicie el foreground.
            .SetContentTitle("GTS Registros Horarios")
            .SetContentText($"{DateTime.Now.ToString("yyyy/MM/dd")}: Se ha iniciado el servicio de alarmas. No olvides registrar la hora.")
            .SetSmallIcon(Resource.Drawable.logo32x32)
            .SetAutoCancel(true)
            .SetOngoing(true)
            .Build();


        //Esto es un método del servicio, no del contexto.
        //A partir de Android 14 es obligatorio indicar el tipo de servicio. En este caso se indica que
        //es location.
        //NOTA: si se indica aquí cómo parámetro, no es necesario indicarlo en el manifiesto.
        //NOTA: antes de llamar a este método se tiene que haber solicitado al usuario permisos
        //de ACCESS_COARSE_LOCATION o ACCESS_FINE_LOCATION.
        StartForeground(NOTIFICATION_ID, notification);
    }


    public override IBinder OnBind(Intent? intent)
    {
        return null;
    }

    //Método que se ejecuta siempre que se llame al método StartService o StartForegroundService
    //de las clases de Android.
    //Aquí es donde se tiene que gestionar el el inicio y la detención del servicio. Si se hace
    //fuera, no funciona.
    //[return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        if (_servicioAlarmas.EstaFuncionando() == false)
        {
            //Aquí solo se inicia el forground.
            //Iniciar o detener otros recursos, como el servicio de alarmas, se tiene que hacer en
            //los métodos OnCreate() y OnDestroy().
            StartForegroundService();

            //SERVICIO ALARMAS
            //El servicio de alarmas se tiene que hacer aquí, en el OnStartCommand, y después de
            //iniciar el foreground.
            _servicioAlarmas.Iniciar();
        }




        //LOCATION LISTENER
        ////////////_locationListener = MauiApplication.Current.Services.GetService<LocationService>()!;
        ////////////IMessenger messenger = MauiApplication.Current.Services.GetService<IMessenger>()!;
        ////////////messenger.Register<LocationModel>(this, (recipient, message) =>
        ////////////{
        ////////////    OnPosicionNuevaHandler(message);
        ////////////});
        ////////////_locationListener.Iniciar();




        //Parece que evita que se detenga solo por orden de Android.
        //Esto se tiene que hacer siempre al final.
        return StartCommandResult.Sticky;
    }
    #endregion creación del foreground


    private void InicializarServicioAlarmas()
    {
        _servicioAlarmas = MauiApplication.Current.Services.GetService<IServicioAlarmas>()!;
    }


    //En OnCreate, se tienen que inicializar los recursos necesarios, en este caso puede ser
    //el servicio de alarmas.
    //En Android se utiliza OnCreate en lugar del constructor, porque no se instancian realmente
    //Activities.
    public override void OnCreate()
    {
        base.OnCreate();

        //Esto se tiene que hacer aquí, no en el constructor, porque si no la suscripción al
        //evento no se hace correctamente, parece ser que porque el foreground y el servicio de
        //alarmas están en dos threads diferentes.
        //Se obtiene el servicio de alarmas registrado por dependency injection en MauiProgram.cs

        //SERVICIO ALARMAS
        //Se tiene que hacer aquí, porque si no, el campo sería null y al intentar detenerlo
        //daría error de nuull. Parece ser que la instanciación en OnStartCommand no asigna
        //el campo.
        InicializarServicioAlarmas();



        //LISTENER
        ////////////_locationListener = MauiApplication.Current.Services.GetService<LocationService>()!;
        ////////////IMessenger messenger = MauiApplication.Current.Services.GetService<IMessenger>()!;
        ////////////messenger.Register<LocationModel>(this, (recipient, message) =>
        ////////////{
        ////////////    OnPosicionNuevaHandler(message);
        ////////////});
        ////////////_locationListener.Iniciar();


        //GEOLOCATOR PLUGIN
        ////////////try
        ////////////{
        ////////////    if (await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 5, true, new ListenerSettings
        ////////////    {
        ////////////        ActivityType = ActivityType.AutomotiveNavigation,
        ////////////        AllowBackgroundUpdates = true,
        ////////////        DeferLocationUpdates = false,
        ////////////        ListenForSignificantChanges = false,
        ////////////        PauseLocationUpdatesAutomatically = false,
        ////////////    }))
        ////////////    {
        ////////////        CrossGeolocator.Current.PositionChanged += CrossGeolocator_Current_PositionChanged!;
        ////////////        CrossGeolocator.Current.PositionError += CrossGeolocator_Current_PositionError!;
        ////////////    }
        ////////////}
        ////////////catch (Exception ex)
        ////////////{
        ////////////    //Logger.Instance.Report(ex);
        ////////////}
    }



    public async override void OnDestroy()
    {
        //SERVICIO ALARMAS
        //Se marca aquí el quitar la suscripción al método, ya que es donde se tiene acceso
        //al servicio de alarmas. En el método detener no.
        //Se tiene que detener aquí el servicio de alarmas, porque si se hace en el método
        //deneter, el cts no queda marcado como true para ser cancelado. Si se hace aquí, sí.
        await _servicioAlarmas.DetenerAsync();



        //LOCATION LISTENER
        ////////////_locationListener.Detener();


        //GEOLOCATOR PLUGIN
        ////////////if (await CrossGeolocator.Current.StopListeningAsync())
        ////////////{
        ////////////    CrossGeolocator.Current.PositionChanged -= CrossGeolocator_Current_PositionChanged!;
        ////////////    CrossGeolocator.Current.PositionError -= CrossGeolocator_Current_PositionError!;
        ////////////}


        //@#ESTUDIAR: ¿Esto es necesario? si es cierto que cuando se detiene y se cierra la
        //aplicación, visual studio aún sigue depurando, hay que dar al stop, por lo que parece
        //que algo queda en ejecución. Quizás esto lo solucione.
        //StopForeground(true);
        //O quizás esto.
        //StopSelf();


        base.OnDestroy();
    }





    //ESTO SOLO SE NECESITA SI SE QUIERE ESCUCHAR AL GPS
    #region GEOLOCATOR PLUGIN
    //Método que se suscribe a la notificación de posición de Geolocator. Por lo que es un
    //Listener. En lugar de solicitar la posición GPS, lo que hace es esperar a que se notifique
    //un cambio en la posición.
    private void CrossGeolocator_Current_PositionChanged(object sender, PositionEventArgs e)
    {
        //Se usa Plugin.LocalNotification para las notificaciones.
        NotificationRequest miRequest = new NotificationRequest
        {
            NotificationId = 1000,
            Title = "GTS Registros Horarios",
            Description = $"{DateTime.Now}: Posición {e.Position.Latitude}, {e.Position.Longitude}.",
            BadgeNumber = 42,
            //Image = new NotificationImage(),
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now,
            },
        };

        LocalNotificationCenter.Current.Show(miRequest);
    }

    private void CrossGeolocator_Current_PositionError(object sender, PositionErrorEventArgs e)
    {
        string dummy = "";
        dummy = " ";
    }
    #endregion GEOLOCATOR PLUGIN
}
