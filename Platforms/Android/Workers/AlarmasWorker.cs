using Android.Content;
using AndroidX.Work;
using Plugin.LocalNotification;



//namespace GTS.CMMS.RegistroHorario.UI.Maui.Platforms.Android.Workers;
namespace MauiGpsRequestInForeground.Maui;



public class AlarmasWorker : Worker
{
    //Se necesita el constructor por defecto.
    public AlarmasWorker(Context context, WorkerParameters workerParams) : base(context, workerParams) { }



    //Aquí en el worker se especifica lo que tiene que hacer, una iteración. Luego será fuera
    //donde se decida cuándo y como ejecutarlo (si una sola vez o de forma periódica).
    //Eso se definirá en el MainActivity.
    public override Result DoWork()
    {
        PublicarNotificacion();

        //Si se llega aquí se dice que la ejecución ha sido correcta.
        return Result.InvokeSuccess();
    }



    private void PublicarNotificacion()
    {
        //Se usa Plugin.LocalNotification para las notificaciones.
        NotificationRequest miRequest = new NotificationRequest
        {
            NotificationId = 1000,
            Title = "GTS Registros Horarios (Worker)",
            BadgeNumber = 42,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now,
            },
        };

        LocalNotificationCenter.Current.Show(miRequest);
    }
}
