using AndroidX.Work;



namespace GTS.CMMS.RegistroHorario.UI.Maui.Helpers;



public class AlarmasWorkerHelper
{
    public void Iniciar()
    {
        //PRUEBA 01: ejecutar una vez. Funciona.
        ////OneTimeWorkRequest miSolicitudDeWorker = new OneTimeWorkRequest.Builder(typeof(AlarmasWorker))
        ////    //Se necesita un tag para loaclizar el trabajo.
        ////    .AddTag("RegistroHorarioAlarmas")
        ////    //Se le puede indicar que una vez se ponga en cola del worker manager, espere X tiempo para
        ////    //su ejecución.
        ////    //.SetInitialDelay(TimeSpan.FromSeconds(90))
        ////    .Build();

        //////PRUEBA 01: ejecutar una sola vez. Funciona.
        //////El work manager se encarga de gestionar los workers.
        ////WorkManager.GetInstance(Android.App.Application.Context)
        ////    //Aquí se indica iniciar el único trabajo que se tiene. Con BeginWith se idicaría
        ////    //con cual comenzar de entre ellos para ir enlanzado varios cuando uno termina,
        ////    //que empiece el otro.
        ////    //ExistingWorkPolicy indica cómo gestionar qué pasa cuando se inicia la apliacación,
        ////    //qué pasa si ya hay un worker de estipo tipo en ejecución... etc. Replace en este caso,
        ////    //para que cada vez que se quiera ejecutar, sea una instancia limpia.
        ////    .BeginUniqueWork("RegistroHorarioAlarmas", ExistingWorkPolicy.Replace, miSolicitudDeWorker)
        ////    //Se le dice al WorkerManager que lo ponga en cola de ejecución, que dependerá de lo que
        ////    //decida, si hay suficiente batería, otros permisos y otras variables.
        ////    .Enqueue();


        //PRUEBA 2: ejecutar periódicamente
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
