namespace MauiGpsRequestInForeground.Helpers;

//Se tiene un interfaz porque cuando se invoca para iniciar el foreground en Android se hace
//con un intent, lo que crea una nueva instancia del foreground, independientemente de que
//en depedency injection se haya indicado que fuera Singleton. Por tanto se tiene este helper
//para hacer la llamada.

public interface IAlarmasForegroundServiceHelper
{
    public void Iniciar();


    public void Detener();
}
