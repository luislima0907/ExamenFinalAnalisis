namespace ExamenFinalAnalisis.Services;

/// <summary>Excepción para violaciones de reglas de negocio (se traduce a HTTP 400).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

/// <summary>Recurso no encontrado (se traduce a HTTP 404).</summary>
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje) { }
}
