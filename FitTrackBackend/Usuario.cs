public class Usuario
{
    public int id { get; set; }
    public string nombre { get; set; } = string.Empty;
    public string correo { get; set; } = string.Empty;
    public string password_hash { get; set; } = string.Empty;
    public DateTime fecha_creacion { get; set; } = DateTime.UtcNow;
}

public class UsuarioRegistroDto
{
    public string nombre { get; set; } = string.Empty;
    public string correo { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}
public class UsuarioLoginDto
{
    public string correo { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
}