using APPingSoft_II.Data;
using APPingSoft_II.Models;
using APPingSoft_II.Repositories;
using APPingSoft_II.Services;

namespace APPingSoft_II;

/// <summary>
/// Clase singleton que centraliza el estado global del sistema.
/// Todas las ventanas trabajan contra esta única instancia.
/// </summary>
public sealed class SistemaApp
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    private static SistemaApp? _instancia;
    private static readonly object _lock = new();

    public static SistemaApp Instancia
    {
        get
        {
            lock (_lock)
            {
                _instancia ??= new SistemaApp();
                return _instancia;
            }
        }
    }

    private SistemaApp() { }

    // ── Estado de sesión ──────────────────────────────────────────────────────
    public Usuario? UsuarioActual { get; private set; }
    public bool SesionActiva => UsuarioActual != null;

    // ── Colecciones en memoria ────────────────────────────────────────────────
    public List<Usuario> Usuarios { get; private set; } = new();
    public List<Instructor> Instructores { get; private set; } = new();
    public List<Curso> Cursos { get; private set; } = new();
    public List<Programa> Programas { get; private set; } = new();
    public List<Participante> Participantes { get; private set; } = new();
    public List<Inscripcion> Inscripciones { get; private set; } = new();
    public List<Evaluacion> Evaluaciones { get; private set; } = new();
    public List<CriterioEvaluacion> Criterios { get; private set; } = new();
    public List<ResultadoEvaluacion> Resultados { get; private set; } = new();

    // ── Repositorios internos ─────────────────────────────────────────────────
    private readonly UsuarioRepository _usuarioRepo = new();
    private readonly InstructorRepository _instructorRepo = new();
    private readonly CursoRepository _cursoRepo = new();
    private readonly ProgramaRepository _programaRepo = new();
    private readonly ParticipanteRepository _participanteRepo = new();
    private readonly InscripcionRepository _inscripcionRepo = new();
    private readonly EvaluacionRepository _evalRepo = new();
    private readonly CriterioEvaluacionRepository _criterioRepo = new();
    private readonly ResultadoEvaluacionRepository _resultadoRepo = new();
    private readonly AutenticacionService _authService = new();

    // ── Inicialización ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifica la conexión y carga todos los datos iniciales.
    /// Lanza InvalidOperationException si la base no es accesible.
    /// </summary>
    public void InicializarSistema()
    {
        if (!ConexionDB.ProbarConexion(out string mensajeConexion))
            throw new InvalidOperationException($"No se pudo conectar a la base de datos.\n{mensajeConexion}\n\nVerifique la cadena de conexión en Config/Configuracion.cs");

        CargarDatosIniciales();
    }

    public void CargarDatosIniciales()
    {
        RecargarUsuarios();
        RecargarInstructores();
        RecargarCursos();
        RecargarProgramas();
        RecargarParticipantes();
        RecargarInscripciones();
        RecargarEvaluaciones();
        RecargarResultados();
    }

    // ── Recarga individual por entidad ────────────────────────────────────────

    public void RecargarUsuarios() => Usuarios = _usuarioRepo.ObtenerTodos();
    public void RecargarInstructores() => Instructores = _instructorRepo.ObtenerTodos();
    public void RecargarCursos() => Cursos = _cursoRepo.ObtenerTodos();
    public void RecargarProgramas() => Programas = _programaRepo.ObtenerTodos();
    public void RecargarParticipantes() => Participantes = _participanteRepo.ObtenerTodos();
    public void RecargarInscripciones() => Inscripciones = _inscripcionRepo.ObtenerTodas();
    public void RecargarEvaluaciones() => Evaluaciones = _evalRepo.ObtenerTodas();
    public void RecargarResultados() => Resultados = _resultadoRepo.ObtenerTodos();
    public void RecargarCriterios(int evaluacionId) =>
        Criterios = _criterioRepo.ObtenerPorEvaluacion(evaluacionId);

    // ── Autenticación ─────────────────────────────────────────────────────────

    /// <summary>
    /// Autentica al usuario y lo establece como usuario actual de la sesión.
    /// </summary>
    public Usuario? AutenticarUsuario(string correo, string contrasena, out string mensaje)
    {
        var usuario = _authService.IniciarSesion(correo, contrasena, out mensaje);
        if (usuario != null)
            UsuarioActual = usuario;
        return usuario;
    }

    public void CerrarSesion()
    {
        UsuarioActual = null;
    }
}
