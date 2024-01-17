/*
 * Un servicio para gestionar los puntos de interés.
 * 
 * Servirá tanto para crear nuevos puntos de interés y agregarlos a la base de datos, lo que
 * se hará desde configuración, hasta devolver los puntos de interés, que será utilizado por
 * el servicio de alarmas.
 * 
 * De este modo se tiene toda la gestión de puntos de interés centralizado, y una vez que se
 * hayan obtenido los puntos de interés una primera vez, se trabajará siempre de forma local,
 * de este modo no se están solicitando periodicamente por parte del servicio de alarmas, a la
 * base de datos.
 * 
 * Esto tiene el problema de que si se agregan puntos de interés desde otro dispositivo no se
 * tendrán encuenta hasta que no se reinicie la aplicación o si las alarmas están activas, hasta
 * que no se reinicie el móvil. Pero se considera que la aplicación estará instalada solo en
 * un dispositivo, no se utilizarán en varios.
 * 
 */




namespace MauiGpsRequestInForeground.Interfaces;



public interface IServicioPuntosInteresAlarmas
{
    /// <summary>
    /// agrega el punto de interés indicado.
    /// </summary>
    /// <param name="paramLatitud">La latitud del punto de interés.</param>
    /// <param name="paramLongitud">La longitud del punto de interés.</param>
    /// <param name="paramDescripcion">La descripción del punto de interés, o el nombre.</param>
    /// <returns>El punto de interés nuevo crado. Nunca será null porque de no poderse crear,
    /// se recebiría una excepción.</returns>
    /// <remarks>Si la base de datos se ha podido actualizar, entonces se borrará también
    /// de la colección local, por lo que siempre estará sincronizada con la información
    /// de la base de datos.</remarks>
    public Task<PuntoInteresAlarmaDTO> AgregarPuntoInteresAlarmaAsync(double paramLatitud, double paramLongitud, string paramDescripcion);

    /// <summary>
    /// Borra el punto de interés indicado.
    /// </summary>
    /// <param name="paramLgIdPuntoInteresParaBorrar"></param>
    /// <returns>Siempre true, porque de no poderse borrar el punto de interés, se recibiría
    /// una excepción.</returns>
    /// <remarks>Si la base de datos se ha podido actualizar, entonces se borrará también
    /// de la colección local, por lo que siempre estará sincronizada con la información
    /// de la base de datos.</remarks>
    public Task<bool> BorrarPuntoInteresAlarmaAsync(long paramLgIdPuntoInteresParaBorrar);

    /// <summary>
    /// Devuelve los puntos de interés actuales.
    /// La primera vez se solicitan a la base de datos y la sucesivas veces se utilizan los
    /// puntos de interés en local.
    /// </summary>
    /// <returns>Null si no se han podido obtener los puntos de interés. Normalmente será porque
    /// ha habido problemas con el acceso a la base de datos.
    /// Una colección con los puntos de interés. Podrá estar vacía, si aún no se han creado
    /// puntos de interés. De este modo se diferencia con el retorno null, ya que null indica
    /// problemas y la colección vacía que aún no hay puntos de interés.</returns>
    /// <remarks>Toda la gestión los puntos de interés se hace a través de este servicio,
    /// de modo que tras una primera solicitud a la base de datos, cuando se agregan o
    /// borran puntos de interés, si se ha podido modificar en la base de datos, se actualiza
    /// la colección local que se tiene, por lo que siempre estará sincronizado, por lo que se
    /// mejora el rendimiento y se tienen los puntos de interés siempre disponibles aunque
    /// hubiese problemas con la conexión.</remarks>
    public Task<List<PuntoInteresAlarmaDTO>?> GetPuntosInteresAlarmasAsync();
}
