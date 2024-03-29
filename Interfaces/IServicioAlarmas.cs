﻿/*
 * Se especifica el interfaz para el servicio que se encargará de las alarmas porque se quiere
 * utilizar para dependency injection.
 * 
 */


namespace MauiGpsRequestInForeground.Interfaces;



public interface IServicioAlarmas
{
    public void Iniciar();
    public Task DetenerAsync();
    public bool EstaFuncionando();
}
