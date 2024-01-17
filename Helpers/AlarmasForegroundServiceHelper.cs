/*
 * Helper que se encargará de iniciar y detener el foreground service.
 * 
 */


using Android.Content;



namespace GTS.CMMS.RegistroHorario.UI.Maui.Helpers;



public class AlarmasForegroundServiceHelper : IAlarmasForegroundServiceHelper
{
    private bool _estaIniciado = false;



    public void Iniciar()
    {
        if(_estaIniciado == false)
        {
#if ANDROID
            var intent = new Intent(Android.App.Application.Context, typeof(AlarmasForegroundService));
            Android.App.Application.Context.StartForegroundService(intent);
#endif

            _estaIniciado = true;
        }
    }


    public void Detener()
    {
#if ANDROID
        var intent = new Intent(Android.App.Application.Context, typeof(AlarmasForegroundService));
        Android.App.Application.Context.StopService(intent);
#endif

        _estaIniciado = false;
    }
}
