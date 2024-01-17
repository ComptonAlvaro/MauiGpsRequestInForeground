using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MauiGpsRequestInForeground.Platforms.Android.BroadcastReecivers;
using Plugin.LocalNotification;

namespace MauiGpsRequestInForeground.Maui;



[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //Se ejecuta al iniciar la aplicación, por tanto, el worker también se inicia al iniciar
    //la aplicación.
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);


        SolicitarOtrosPermisos();
        SolicitarPermisosPlublicacionNotificacionesYCreacionDeCanalDeNotificacion();
    }



    /// <summary>
    /// Solicita los permisos que necesitará la aplicación para funcionar.
    /// </summary>
    /// <remarks>En lugar de tener un método para solicitar todos los permisos, se utiliza
    /// uno por permiso, ya que para la publicación de notificaciones no es necesario en
    /// Android 12 e inferiores, es a partir de Android 13. Pero para utilizar la localización,
    /// las condiciones son otras.</remarks>
    private void SolicitarPermisosPlublicacionNotificacionesYCreacionDeCanalDeNotificacion()
    {
        ////Permiso cuando la aplicación está en primer plano.
        //PermissionStatus miEstado = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        ////Necesario para poder utilizar la localización cuando la aplicación no está en primer plano.
        //await Permissions.RequestAsync<Permissions.LocationAlways>();

        //Para Android 13, se necesita solicitar de forma explícita que se permitan publicar
        //notificaciones.
        //Si no se pregunta, en configuración de notificaciones, se le puede dar permisos también,
        //también funcionaría. Pero preguntando la primera vez es más cómodo para el usuario.



        //Para versiones anteriores a Android 13 no es necesario solicitar permisos para publicar
        //notificaciones.
        if ((int)Build.VERSION.SdkInt < 33) return;



        //Parece ser que cada solicitud de permisos tiene que tener un código, aunque en el método
        //de solicitud se puede pasar una array de permisos en una misma solicitud.
        const int _codigoSolictudPermisos = 100;

        string[] misPermisosSolicitados =
                {
                    Manifest.Permission.PostNotifications
                };

        if (this.CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            this.RequestPermissions(misPermisosSolicitados, _codigoSolictudPermisos);
        }



        //CREACIÓN DEL CANAL
        //La aplicación solo funciona a partir de la API 27, por lo que siempre se tendrá el manager y no será null.
        var miNotificationManager = (GetSystemService(Context.NotificationService) as NotificationManager)!;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            //El channel siempre es para toda la aplicación.
            NotificationChannel channel = new NotificationChannel("1", "notification", NotificationImportance.Default);
            miNotificationManager.CreateNotificationChannel(channel);
        }
    }

    private async void SolicitarOtrosPermisos()
    {
        //Los permisos se tiene que solicitar después de que la primera pantalla se haya mostrado.
        //await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        //Cuando se solicita LocationAlways, se pregunta antes por LocationWhenInUse.
        await Permissions.RequestAsync<Permissions.LocationAlways>();

        await LocalNotificationCenter.RequestNotificationPermissionAsync();
    }



}
