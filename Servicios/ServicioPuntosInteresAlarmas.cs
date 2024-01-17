/*
 * Servicio para obtener los puntos de interés.
 * 
 * Se tiene un sevicio porque aunque inicialmente se solicitarán al servidor los puntos de interés
 * cada vez que se vaya a comprobar la alarma, dejará abierta la opción de por ejemplo de obtener
 * todos los puntos de interés al inicializar la aplicación, guardarlos de forma local y de
 * este modo no hará falta ir a la base de datos siempre.
 * 
 * Tiene el problema de que si se agregan puntos de interés desde otro dispositivo, no se tendrá
 * hasta que no se inicie la aplicación de nuevo.
 * 
 */


using System.Collections.Generic;
using Java.Nio.Channels;
using Plugin.LocalNotification;

namespace MauiGpsreqeustInForeground;



internal class ServicioPuntosInteresAlarmas : IServicioPuntosInteresAlarmas
{
    #region variables de clase
    private readonly IServicioAplicacionUIRegsitroHorario _servicioAplicacion;

    private readonly List<PuntoInteresAlarmaDTO> _puntosInteres = new List<PuntoInteresAlarmaDTO>();


    /// <summary>
    /// Determina si ya se han solicitado los puntos de interés a la base de datos. De este modo,
    /// una vez solicitados, se trabajará siempre con los puntos de interés que se tiene en local
    /// en la colección _puntosInteres.
    /// Será false si se han solicitiado y ha habido algún problema de conexión con la base de datos,
    /// si no, será true en cuanto la primera solicitud sea correcta.
    /// </summary>
    private bool _puntosInteresSolicitados = false;
    #endregion variables de clase



    #region constructores
    public ServicioPuntosInteresAlarmas(IServicioAplicacionUIRegsitroHorario paramServicioAplicacion)
    {
        _servicioAplicacion = paramServicioAplicacion;
    }
    #endregion constructores



    public async Task<PuntoInteresAlarmaDTO> AgregarPuntoInteresAlarmaAsync(double paramLatitud, double paramLongitud, string paramDescripcion)
    {
        PuntoInteresAlarmaDTO miPuntoInteresNuevo = await _servicioAplicacion.AgregarPuntoInteresAlarmasAsync(paramLatitud, paramLongitud, paramDescripcion);

        //Se agrega una vez que se ha asegurado que se ha podido guardar en el servidor.
        _puntosInteres.Add(miPuntoInteresNuevo);

        return miPuntoInteresNuevo;
    }


    public async Task<bool> BorrarPuntoInteresAlarmaAsync(long paramLgIdPuntoInteresParaBorrar)
    {
        await _servicioAplicacion.BorrarPuntoInteresAlarmasAsync(paramLgIdPuntoInteresParaBorrar);

        //Se borra una vez que se ha asegurado que se ha podido borrar del servidor.
        _puntosInteres.Remove(_puntosInteres.First(x => x.Id == paramLgIdPuntoInteresParaBorrar));

        return true;
    }


    public async Task<List<PuntoInteresAlarmaDTO>?> GetPuntosInteresAlarmasAsync()
    {
        try
        {
            //Si no hay puntos de interés, se considera que no se han solicitado aún, por lo que
            //se solicitan.
            if (_puntosInteresSolicitados == false)
            {
                RefrescarPuntosInteres(await _servicioAplicacion.GetPuntosInteresAlarmasDeEmpleadoAsync());
                _puntosInteresSolicitados = true;
            }

            //Se devuelve una lista nueva, porque si no, quien la recibiera, podría modificar la
            //colección al recibir la lista original.
            return _puntosInteres.ToList();
        }
        catch
        {
            //Si no se puede acceder a la base de datos, se devuelve una colección vacía, para
            //que no haya errores. Quizás cuando se soliciten de nuevo los puntos de interés,
            //habrá más suerte.
            return null;
        }
    }



    private void RefrescarPuntosInteres(IEnumerable<PuntoInteresAlarmaDTO> paramIePuntosInteres)
    {
        _puntosInteres.Clear();

        _puntosInteres.AddRange(paramIePuntosInteres);
    }
}
