using System.Windows;

namespace APPingSoft_II.Logic;

/// <summary>
/// Centraliza la lógica de permisos del sistema por rol.
///
/// Matriz de permisos:
/// ┌────────────────────────────────┬───────────────┬─────────────┬────────────┐
/// │ Acción                         │ Administrador │ Coordinador │ Instructor │
/// ├────────────────────────────────┼───────────────┼─────────────┼────────────┤
/// │ Gestionar Programas (CRUD)     │      ✅        │     ✅      │     ❌     │
/// │ Gestionar Participantes (CRUD) │      ✅        │     ✅      │     ❌     │
/// │ Gestionar Evaluaciones (CRUD)  │      ✅        │     ✅      │     ❌     │
/// │ Registrar Resultados (CRUD)    │      ✅        │     ❌      │     ✅     │
/// │ Ver Reportes                   │      ✅        │     ✅      │     ✅     │
/// │ Ver Métricas de Programas      │      ✅        │     ✅      │     ❌     │
/// │ Acceder navbar Programas       │      ✅        │     ✅      │     ❌     │
/// │ Acceder navbar Participantes   │      ✅        │     ✅      │     ❌     │
/// │ Acceder navbar Evaluaciones    │      ✅        │     ✅      │     ❌     │
/// └────────────────────────────────┴───────────────┴─────────────┴────────────┘
/// </summary>
public static class Permisos
{
    private static string Rol => SistemaApp.Instancia.UsuarioActual?.Rol ?? string.Empty;

    // ── Permisos de escritura ─────────────────────────────────────────────────

    /// <summary>Puede crear, modificar y eliminar Programas.</summary>
    public static bool PuedeGestionarProgramas()     => Rol is "Administrador" or "Coordinador";

    /// <summary>Puede crear, modificar y desactivar Participantes.</summary>
    public static bool PuedeGestionarParticipantes() => Rol is "Administrador" or "Coordinador";

    /// <summary>Puede crear, modificar y eliminar Evaluaciones y Criterios.</summary>
    public static bool PuedeGestionarEvaluaciones()  => Rol is "Administrador" or "Coordinador";

    /// <summary>Puede crear, modificar y eliminar Resultados de evaluación.</summary>
    public static bool PuedeRegistrarResultados()    => Rol is "Administrador" or "Instructor";

    // ── Permisos de navegación ────────────────────────────────────────────────

    /// <summary>Puede ver el link de Gestión de Programas en la barra de navegación.</summary>
    public static bool PuedeAccederGestionProgramas()     => Rol is "Administrador" or "Coordinador";

    /// <summary>Puede ver el link de Gestión de Participantes en la barra de navegación.</summary>
    public static bool PuedeAccederGestionParticipantes() => Rol is "Administrador" or "Coordinador";

    /// <summary>Puede ver el link de Gestión de Evaluaciones en la barra de navegación.</summary>
    public static bool PuedeAccederGestionEvaluaciones()  => Rol is "Administrador" or "Coordinador";

    /// <summary>Puede ver métricas de programas (pestaña administrativa en reportes).</summary>
    public static bool PuedeVerMetricasPrograma() => Rol is "Administrador" or "Coordinador";

    // ── Validación en capa Logic ──────────────────────────────────────────────

    /// <summary>
    /// Lanza UnauthorizedAccessException si el permiso es false.
    /// Llamar al inicio de cualquier método de escritura en Logic.
    /// La excepción se propaga a la UI donde el catch la muestra como MessageBox.
    /// </summary>
    public static void Requerir(bool permiso,
        string mensaje = "No tiene permisos para realizar esta acción.")
    {
        if (!permiso)
            throw new UnauthorizedAccessException(mensaje);
    }

    // ── Helper de visibilidad para code-behind ────────────────────────────────

    /// <summary>
    /// Retorna Visible si la condición es true, Collapsed en caso contrario.
    /// Usado en ventanas para mostrar/ocultar elementos según el rol.
    /// </summary>
    public static Visibility VisibleSi(bool condicion) =>
        condicion ? Visibility.Visible : Visibility.Collapsed;
}
