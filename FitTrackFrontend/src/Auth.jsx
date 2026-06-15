import { useState } from 'react';

export default function Auth({ onLoginSuccess }) {
  const [esLogin, setEsLogin] = useState(true);
  const [nombre, setNombre] = useState('');
  const [correo, setCorreo] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const API_URL = `${import.meta.env.VITE_API_URL}/api/auth`;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const endpoint = esLogin ? '/login' : '/register';
    const bodyData = esLogin 
      ? { correo, password } 
      : { nombre, correo, password };

    try {
      const res = await fetch(`${API_URL}${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(bodyData)
      });

      const data = await res.json();

      if (!res.ok) {
        throw new Error(data.mensaje || 'Ocurrió un error');
      }

      if (esLogin) {
        // Guardamos los datos del usuario en el navegador
        localStorage.setItem('usuarioId', data.usuarioId);
        localStorage.setItem('usuarioNombre', data.nombre);
        
        // Le avisamos a App.jsx que el usuario ya entró
        onLoginSuccess(data.usuarioId, data.nombre);
      } else {
        alert('¡Registro exitoso! Ya puedes iniciar sesión.');
        setEsLogin(true); // Pasamos al login automáticamente
      }
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
      <h2>{esLogin ? 'Iniciar Sesión' : 'Crear Cuenta'}</h2>
      
      {error && <p style={{ color: 'red' }}>{error}</p>}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
        {!esLogin && (
          <input 
            type="text" 
            placeholder="Nombre completo" 
            value={nombre} 
            onChange={(e) => setNombre(e.target.value)} 
            required 
          />
        )}
        <input 
          type="email" 
          placeholder="Correo electrónico" 
          value={correo} 
          onChange={(e) => setCorreo(e.target.value)} 
          required 
        />
        <input 
          type="password" 
          placeholder="Contraseña" 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
          required 
        />
        <button type="submit" style={{ padding: '10px', background: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
          {esLogin ? 'Entrar' : 'Registrarme'}
        </button>
      </form>

      <button 
        onClick={() => setEsLogin(!esLogin)} 
        style={{ marginTop: '15px', background: 'none', border: 'none', color: '#007bff', cursor: 'pointer', textDecoration: 'underline' }}
      >
        {esLogin ? '¿No tienes cuenta? Regístrate aquí' : '¿Ya tienes cuenta? Inicia sesión'}
      </button>
    </div>
  );
}