using AndroidX.Work;



namespace MauiGpsRequestInForeground.Helpers;



public class AlarmasWorkerHelper
{
    public void Iniciar()
    {
        PeriodicWorkRequest miWorkPeriodico = new PeriodicWorkRequest.Builder(typeof(AlarmasWorker), TimeSpan.FromSeconds(5)).Build();

        WorkManager.GetInstance(Android.App.Application.Context)
            .Enqueue(miWorkPeriodico);

        //Una vez en cola, se ejecutará, no importa si se cierra la aplicación o se reinicia el sistema,
        //que el manager lo guarda en una base de datos mySql. La única forma de que no se ejecute es
        //quitándolo del manager.
    }

    public void Detener()
    {
        throw new NotImplementedException();
    }
}
