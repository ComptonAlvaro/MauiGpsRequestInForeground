/*
 * Clase que determina cuándo recordar que se tiene que registrar un nuevo registro horario.
 * 
 * Para ello, periódicamente va solicitando la posición GPS y si la distancia es menor o igual
 * a alguna de las localizaciones de las cuales se quiere recordar que se tiene que registrar,
 * entonces avisará.
 * 
 */



using GTS.CMMS.RegistroHorario.Comun.DTO;
using GTS.CMMS.RegistroHorario.UI.Maui.Configuracion;
using GTS.CMMS.RegistroHorario.UI.Maui.Interfaces;
using Java.Sql;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace GTS.CMMS.RegistroHorario.UI.Maui.Servicios;



public class ServicioAlarmas : IServicioAlarmas
{
    #region variables de clase
    //Todas las variables se inicializan en el constructor, para tenerlo todo centralizado.

    //Se utiliza para poder hacer el dispose del método principal, el que determinar si avisar o no.
    private Task? _taskTimer;
    private PeriodicTimer _timer;
    private CancellationTokenSource _cts;
    private readonly IServicioConfiguracion _servicioConfiguracion;
    private readonly ILocalizadorHelper _localizadorHelper;
    private readonly IServicioPuntosInteresAlarmas _servicioPuntosInteresAlarmas;

    //El plugin para las notificaciones.
    private INotificationService _notificationService;

    //El último punto de interés en el que se ha estado.
    private PuntoInteresAlarmaDTO? _puntoInteresActual;
    
    private bool _repetirNotificacion;
    private string? _mensajeNotificacionARepetir;

    //Indica si está funcionando o no. De este modo, si se intenta iniciar cuando ya funcionando,
    //no se iniciará de nuevo, lo que provocaría que se reniciaran las variables y se volviera al
    //estado de referencia. Esto podría ocurrir si se entra a la configuración varias veces
    //cuando se tienen activas las alarmas.
    //Se gestiona aquí, para que el consumidor no se tenga que preocupar.
    private bool _estaFuncionando;
    #endregion variables de clase



    #region constructores
    /// <summary>
    /// El constructor.
    /// </summary>
    /// <param name="paramLocalizadorHelper">El helper que dará la posición de GPS cuando se le
    /// solicite.</param>
    /// <param name="paramMessenger">El mensajero para notificar eventos al exterior. Será un
    /// mensaje indicando el texto de la alarma.</param>
    public ServicioAlarmas(ILocalizadorHelper paramLocalizadorHelper,
        IServicioConfiguracion paramServicioConfiguracion,
        IServicioPuntosInteresAlarmas paramServicioPuntosInteresAlarmas,
        INotificationService paramNotificationService)
    {
        //@#ESTUDIAR: si se tienen los datos correctos para iniciar el servicio. El timer
        //necesita un valor mayor que cero y si no, daría excepción. Si se propaga no llega
        //al view model de configuración, por lo que la comprobación se hace en el view model
        //de configuración, pero es más correcto aquí.




        _localizadorHelper = paramLocalizadorHelper;
        _servicioPuntosInteresAlarmas = paramServicioPuntosInteresAlarmas;
        _servicioConfiguracion = paramServicioConfiguracion;
        _notificationService = paramNotificationService;

        //Las variables no se inicializan aquí, se hará cada vez que se inicie el servicio con el
        //método start(), para asegurase de que siempre se inicia desde el mismo estado de referencia.

        //esta variable sí se establece en el constructor, porque tiene que estar a false. A partir
        //de ahí, se gestionará con los métodos Iniciar() y Detener().
        _estaFuncionando = false;


        _notificationService.NotificationActionTapped += Current_NotificationActionTapped;
    }


    /// <summary>
    /// Inicializa las variables necesarias para partir de un punto inicial de referencia.
    /// </summary>
    /// <remarks>Se inicializará cada vez que se inicie el servicio de alarmas mediante el
    /// método Iniciar().</remarks>
    private void InicializarVariables()
    {
        //_cts se instancia cada vez que se inicia el servicio, ya que una vez cancelado, si
        //permanece con la señal de cancelado y no se puede reutilizar.
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(_servicioConfiguracion.GetIntervaloAlarmas()));
        _cts = new CancellationTokenSource();

        //Inicialmente null, porque se considera que no se ha estado en ninguno cuando se inicien
        //las alarmas.
        _puntoInteresActual = null;
        //Siempre se repiten las alarmas salvo que el usuario indique lo contrario.
        _repetirNotificacion = true;
        //Inicialmente no se tiene ningún mensaje de notificación, lo determina las circunstancias.
        _mensajeNotificacionARepetir = null;
    }
    #endregion constructores



    #region implementación del interfaz
    public bool EstaFuncionando()
    {
        return _estaFuncionando;
    }



    //No es async porque lo que se quiere es lanzar y olvidar la tarea async. Se detendrá cuando
    //se pare y se hará el dispose.
    public async void Iniciar()
    {
        await Permissions.RequestAsync<Permissions.LocationAlways>();

        //Solo se inicia si no está funcionando, para evitar que se reinicien las variables
        //al estado inicial.
        if (_estaFuncionando == false)
        {
            //Se inicializan siempre las variables, para asegurarse de que siempre se parte de un
            //estado inicial de referencia.
            //Esto también evita depender de quien consuma el servicio de alarmas decida que sea
            //singleton or transient. Ya que si lo hicera  singlenton y la inicialización se hiciera
            //solo en el constructor, no siempre se partiría del mismo estado de referencia.
            InicializarVariables();

            _estaFuncionando = true;

            _taskTimer = DoWorkAsync();
        }
    }


    public async Task DetenerAsync()
    {
        //Lo primero es indicar que ya no funciona. Porque como tarda un poco, se puede
        //dar el caso de que si se inicia y detine varias veces rápido, que en un momento
        //de inicio aún estuviera a false y daría error en el cts de que está disposed.
        _estaFuncionando = false;

        if (_taskTimer == null) return;

        _cts.Cancel();
        await _taskTimer;
        _cts.Dispose();
    }
    #endregion implementación del interfaz



    #region lógica de negocio
    /// <summary>
    /// Determina si se tiene que notificar una alarma o no.
    /// </summary>
    /// <returns></returns>
    private async Task DoWorkAsync()
    {
        try
        {
            //Siempre se ejecuta. Si la ubicación no está activa, será null, por lo que no dará
            //excepción y seguirá con la siguiente iteración.
            while (await _timer.WaitForNextTickAsync(_cts.Token))
            {
                try
                {
                    //La información necesaria se obtiene siempre en cada interación, así si hay modificaciones
                    //en configuración por ejemplo, siempre se tendrá la información más actualizada sin necesidad
                    //de reniciar las alarmas.
                    List<PuntoInteresAlarmaDTO>? miLstPuntosInteresAlarmas = await _servicioPuntosInteresAlarmas.GetPuntosInteresAlarmasAsync();
                    Location? miLocalizacionActual = null;
                    //Si se produce alguna excepción, se esperará a la siguiente interación. Puede que puntualmente
                    //no haya señal de GPS o puede que esté desactivado. Pero el servicio de alarmas no tiene que notificar
                    //esto al usuario, solo alarmas.
                    try
                    {
                        //Se asegura de solicitar el permiso para utilizarlo. Porque si se tienen activadas las
                        //alarmas y se deshabilita el GPS. Aunque se active, no puede obtener las localización.
                        //NOTA: no se hace en el helper, porque aquí se quiere el permiso de poder utilizarlo siempre,
                        //que es una necesidad del servicio de alarmas.
                        await Permissions.RequestAsync<Permissions.LocationAlways>();
                        miLocalizacionActual = await _localizadorHelper.GetLocalizacionAsync(GeolocationAccuracy.Best);
                    }
                    catch { }
                    

                    //Solo se hace algo si se tiene la información necesaria. Si ha habido algún problema porque no se
                    //ha podido conectar con el servidor o no se tiene señal de GPS, entonces se espera a la siguiente
                    //iteración.
                    //De este modo, se evitan falsa información, como por ejemplo estar dentro de un punto de interés,
                    //a la siguiente iteración no tener GPS y como es null, considerar que se está fuera cuando no es cierto.
                    //Esto es posible porque si se está dentro de un edificio, puede que no se tenga GPS, por lo que no
                    //se puede deducir que se ha salido.
                    if(miLstPuntosInteresAlarmas != null && miLocalizacionActual != null)
                    {
                        int miIntDistnacia = _servicioConfiguracion.GetDistanciaAlarmas();

                        PuntoInteresAlarmaDTO? miPuntoInteresNuevo = GetPuntoInteresEnElQueEstoy(miLocalizacionActual, miLstPuntosInteresAlarmas, miIntDistnacia);

                        string? miMensajeAlarma = GetMensajeAlarma(_puntoInteresActual, miPuntoInteresNuevo);

                        //Solo se notifica si el mensaje es diferente.
                        if (string.CompareOrdinal(miMensajeAlarma, _mensajeNotificacionARepetir) != 0)
                        {
                            _mensajeNotificacionARepetir = miMensajeAlarma;
                            //Se tiene un nuevo mensaje, que se tendrá que notificar al menos una vez.
                            _repetirNotificacion = true;
                        }

                        //Una vez procesada la información, el punto de interés nuevo pasa a ser el actual.
                        _puntoInteresActual = miPuntoInteresNuevo;



                        //Solo se notifica algo cuando se tiene toda la información necesaria para determinar
                        //la alarma.
                        if (_repetirNotificacion == true)
                        {
                            PublicarNotificacion(_mensajeNotificacionARepetir!);
                        }
                    }
                    else
                    {
                        //Se notifican posibles fallos para indicar el porqué las alarmas no funcionan.
                        string miMensajeError;

                        if (miLstPuntosInteresAlarmas == null && miLocalizacionActual == null)
                        {
                            miMensajeError = "No se puede conectar con el serivdor ni se tiene activado el GPS.";
                        }
                        else if (miLstPuntosInteresAlarmas == null && miLocalizacionActual != null)
                        {
                            miMensajeError = "No se puede conectar con el servidor.";
                        }
                        else
                        {
                            //Aquí solo queda el caos de que no se tenga activado el GPS.
                            miMensajeError = "no se tiene activado el GPS.";
                        }

                        PublicarNotificacion(miMensajeError);
                    }
                }
                catch (Exception ex)
                {
                    //Cualquier excepción se notifica, ya que así el usuario sabrá si algo falla.
                    PublicarNotificacion($"Se ha producido un error. {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            //No se notifica, ya que el servicio de alarmas solo se puede detener desde la configuración
            //de la aplicación. Es mejor gestionarlo ahí.
        }
    }


    /// <summary>
    /// Devuelve el punto de interés en que estoy actualmente.
    /// </summary>
    /// <param name="paramLocalizacionActual">La localización actual.</param>
    /// <param name="paramIePuntosInteres">La colección de puntos de interés.</param>
    /// <param name="paramIntDistancia">Distancia máxima a la que se considera estar en un punto
    /// de interés.</param>
    /// <returns>Null: si no se tiene localización, si no se tienen puntos de interés o porque se está fuera.
    /// El primer punto de interés en el que se está. Si se puede estar en varios, el primero, sin
    /// garantizar que sea el más próximo.</returns>
    private PuntoInteresAlarmaDTO? GetPuntoInteresEnElQueEstoy(Location? paramLocalizacionActual, IEnumerable<PuntoInteresAlarmaDTO>? paramIePuntosInteres,
        int paramIntDistancia)
    {
        //Si no se tiene localización por algún motivo, se considera que no se está en ningún
        //punto de interés.
        if (paramLocalizacionActual == null) return null;

        if (paramIePuntosInteres?.Count() > 0 == false) return null;

        //@#MEJORAR: se podría mejorar si se obtuviese el punto de interés más cercano, si varios
        //radios se solapasen. Pero se considera que con obtener el primero es suficiente.
        return paramIePuntosInteres
            .FirstOrDefault(x => (Location.CalculateDistance(paramLocalizacionActual, x.Latitud, x.Longitud, DistanceUnits.Kilometers) * 1000) <= paramIntDistancia);
    }


    /// <summary>
    /// Determina la variación del punto de interes.
    /// </summary>
    /// <returns>1 si se pasa de no estar en un punto de interés a estarlo.
    /// 0 Si no ha variación, bien porque no se estaba en un punto de interés ni antes ni ahora
    /// o al revés, porque se estaba en punto de interés y se sigue estando en el mismo punto de
    /// interés.
    /// 1 Si hay variación de punto de interés, bien porque no se estaba en ninguno y se pasa a
    /// estar en uno o bien porque se estaba en uno y se pasa a estar en otro diferente.</returns>
    private int GetDireccionDeVariacionPuntoInteres(PuntoInteresAlarmaDTO? paramPuntoInteresActual, PuntoInteresAlarmaDTO? paramPuntoInteresNuevo)
    {
        //Se permanece en el mismo punto de interés o fuera en ambos casos.
        if(paramPuntoInteresActual?.Id == paramPuntoInteresNuevo?.Id)
        {
            return 0;
        }

        //Se entra en punto de interés.
        else if(paramPuntoInteresActual == null && paramPuntoInteresNuevo != null)
        {
            return 1;
        }
        //Se sale de punto de interés.
        else if(paramPuntoInteresActual != null && paramPuntoInteresNuevo == null)
        {
            return -1;
        }
        else
        {
            //Solo queda la posibilidad de los dos están en punto de interés y son diferentes.
            return 1;
        }
    }

    /// <summary>
    /// Devuelve el mensaje de alarma que se tendrá que notificar.
    /// Realmente tiene toda la lógica de negocio para determinar se realizar la notificación o no.
    /// </summary>
    /// <returns>El mensaje que se tiene que notificar.</returns>
    /// <remarks>Nunca es null, porque si se pasa null al plugin LocalNotification, tarda más tiempo
    /// en procesarlo, y si se ha indicado por ejemplo notificar cada 6 segundos, lo limita a 30 segundos
    /// al menos. Es un caso que no se dará en la práctica, pero mejor tenerlo en cuenta.</remarks>
    private string GetMensajeAlarma(PuntoInteresAlarmaDTO? paramPuntoInteresActual, PuntoInteresAlarmaDTO? paramPuntoInteresNuevo)
    {
        int miIntDireccionDeVariacion = GetDireccionDeVariacionPuntoInteres(paramPuntoInteresActual, paramPuntoInteresNuevo);

        if(miIntDireccionDeVariacion == -1)
        {
            //Si el sistema se inicia por aquí no pasa, porque el punto actual es null y se está
            //fuera de un punto de interés, por lo que no hay varíación. El sistema ya habrá avisado
            //de forma genérica que no olvide registrar la hora, sin especificar si es la de entrada
            //o la de salida.
            return $"Has salido de {paramPuntoInteresActual!.Descripcion}. No olvides registrar la hora de salida.";
        }
        else if(miIntDireccionDeVariacion == 1)
        {
            //Si el sistema se inicia y se está dentro de un punto de interés, avisará de que no olvide
            //registrar la entrada.
            return $"Has entrado en {paramPuntoInteresNuevo!.Descripcion}. No olvides registrar la hora de entrada.";
        }
        else
        {
            //Aquí solo se puede llegar porque el punto de interés actual y nuevo son null. Es porque
            //se ha iniciado el servicio fuera de un punto de interés. En este caso, el mensaje a
            //repetir es null también. Se pondrá un mensaje genérico al no saber si se tiene que registrar
            //la hora de entrada o de salida.
            //En caso de que el mensaje a repetir no sea null, se notificará el mismo mensaje al no haber
            //variación.

            return (_mensajeNotificacionARepetir == null) ? "No olvides registrar la hora." : _mensajeNotificacionARepetir;
        }
    }
    #endregion lógica de negocio



    #region Notificaciones
    private void PublicarNotificacion(string paramStrmensaje)
    {
        NotificationRequest miRequest = new NotificationRequest
        {
            //ID de notificacion 1, el mismo que la notificación que se utiliza para iniciar
            //el foreground. De este modo no se tienen dos notificaciones y ésta
            //"sobreescribirá" la inicial, que no se quiere realmente, solo es necesaria para
            //iniciar el foreground.
            NotificationId = 1,
            Title = "GTS Registros Horarios",
            Subtitle = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"),
            Description = paramStrmensaje,
            BadgeNumber = 42,
            Image = new NotificationImage(),
            //Si se quieren botones de acción, tiene que ser del mismo tipo que el indicado en
            //MAUIProgram.cs
            CategoryType = NotificationCategoryType.Alarm,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now,
            },
        };

        _notificationService.Show(miRequest);
    }


    private void Current_NotificationActionTapped(NotificationActionEventArgs e)
    {
        if(e.IsDismissed
            || e.IsTapped)
        {
            //Se considera que si el usuario a hecho tap en la notificación, es que ya no quiere
            //que se repita más dicha alarma.

            //NOTA: no se pone a null _mensajeNotificacionARepetir porque se necesita para lógica
            //de negocio, ya que se comprueba si es el mismo o no para determinar si se tiene que
            //publicar un nuevo mensaje. Solo tiene que ser null en la instanciación o cuando
            //se inicia el servicio. Aquí solo se quiere detener la repetición del aviso.
            _repetirNotificacion = false;
        }



        ////////////switch (e.ActionId)
        ////////////{
        ////////////    case 100:
        ////////////        Device.BeginInvokeOnMainThread(async () =>
        ////////////        {
        ////////////            await Shell.Current.DisplayAlert("ERROR", "OK y notificar actual", "OK");
        ////////////        });
        ////////////        break;

        ////////////    case 101:
        ////////////        await Shell.Current.DisplayAlert("ERROR", "OK y notificar siguiente", "OK");
        ////////////        break;
        ////////////}
    }
    #endregion Notificaciones
}
