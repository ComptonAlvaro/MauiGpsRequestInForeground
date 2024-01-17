using Android.App;
using Android.Content;
using Android.Gms.Common.Api.Internal;
using Android.OS;
using AndroidX.Core.App;
using static Android.InputMethodServices.Keyboard;

namespace GTS.CMMS.RegistroHorario.UI.Maui.Platforms.Android.BroadcastReecivers;


//Esta clase tiene la lógica que se tiene que ejecutar cuando se lance la alarma. Por ello
//es un BroadcastReceiver, que recive la señal cuando la alarma se lance.
//La alarma se podrá crear en cualquier parte, por ejemplo en el MainActivity o bien en una
//clase a nivel de MAUI que se inicie cuando se activa un switch.

[BroadcastReceiver]
public class AlarmBroadcastReceiver : BroadcastReceiver
{
  public async override void OnReceive(Context? context, Intent? intent)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High);
            var location = await Geolocation.GetLocationAsync(request);


            var notificationBuilder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.logo32x32)
                //.SetWhen(System.currentTimeMillis())  //When the event occurred, now
                .SetTicker("Message on status bar")  //message shown on the status bar
                .SetContentTitle("Marquee Message")   //Title message top row.
                .SetContentText($"{DateTime.Now}: La posición GPS es {location?.Latitude}, {location?.Longitude}.")  //second row
                .SetAutoCancel(false);  //finally build and return a Notification


            var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel("1000", "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                if (notificationManager != null)
                {
                    notificationBuilder.SetChannelId("1000");
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }




            notificationManager.Notify(1, notificationBuilder.Build());
        }
        catch(Exception ex)
        {
            var notificationBuilder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.logo32x32)
                //.SetWhen(System.currentTimeMillis())  //When the event occurred, now
                .SetTicker("Message on status bar")  //message shown on the status bar
                .SetContentTitle("Marquee Message")   //Title message top row.
                .SetContentText($"{DateTime.Now}: ERROR: {ex.Message}.")  //second row
                .SetAutoCancel(false);  //finally build and return a Notification


            var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;

            if (global::Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel("1000", "Title", NotificationImportance.High);
                notificationChannel.Importance = NotificationImportance.High;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);
                notificationChannel.SetShowBadge(true);
                notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300 });

                if (notificationManager != null)
                {
                    notificationBuilder.SetChannelId("1000");
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }
        }

        



        




        //Notification notification = new Notification.Builder(context, "1000")
        //    //este texto es lo que se mostrará en la barra de notificaciones del móvil cuando se
        //    //inicie el foreground.
        //    .SetContentTitle("Servicio trabajando (Título)")
        //    .SetContentText($"{DateTime.Now}: La posición GPS es {location?.Latitude}, {location?.Longitude}.")
        //    .SetSmallIcon(Resource.Drawable.logo32x32)
        //    .SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
        //    .SetAutoCancel(false)
        //    .SetOngoing(true)
        //    .Build();



        ////No se utiliza StartForegroundService() para actualizar la notificación, sino el
        ////manager, mediante el método Notify.
        ////Esto utiliza la notificación con el ID indicado y es actualizado.
        //_notificationManager.Notify(1, notification);
    }
}
