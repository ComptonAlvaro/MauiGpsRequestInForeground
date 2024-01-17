using Android;
using Android.App;
using Android.Content;
using Android.OS;

[assembly: UsesPermission(Manifest.Permission.ReceiveBootCompleted)]
//Tiene que pertenecer al namespace raíz para poder tener acceso a Android.Content y otros
//recursos.
namespace MauiGpsRequestInForeground.Maui;


//NOTA: en elagunos dispositivos es necesario en ajustes, en inicio automático, permitir a la
//aplicación iniciar automáticamente. Si no, no iniciará.
[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted })]
public class ActionBootCompletedBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        IniciarAlarmasForegroundService();
    }



    private void CrearCanalNotificaciones(Context paramContexto)
    {
        //La aplicación solo funciona a partir de la API 27, por lo que siempre se tendrá el manager y no será null.
        var miNotificationManager = (paramContexto.GetSystemService(Context.NotificationService) as NotificationManager)!;

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            //El channel siempre es para toda la aplicación.
            NotificationChannel channel = new NotificationChannel("1", "notification", NotificationImportance.Default);
            miNotificationManager.CreateNotificationChannel(channel);
        }
    }



    private void IniciarAlarmasForegroundService()
    {
        IServicioConfiguracion miServicioConfiguracion = MauiApplication.Current.Services.GetService<IServicioConfiguracion>()!;

        bool miBlActivarAlarmas = miServicioConfiguracion.GetAlarmasActivas();

        if (miBlActivarAlarmas == true)
        {
            IAlarmasForegroundServiceHelper miForegroundService = MauiApplication.Current.Services.GetService<IAlarmasForegroundServiceHelper>()!;

            miForegroundService.Iniciar();
        }
    }
}
