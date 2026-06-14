import { useState, useEffect } from 'react'
import './App.css'
const API_URL = import.meta.env.VITE_API_URL;

function App() {
  const [ejercicios, setEjercicios] = useState([])
  
  const [nombre, setNombre] = useState('')
  const [musculo, setMusculo] = useState('Piernas')
  const [series, setSeries] = useState('')
  const [repeticiones, setRepeticiones] = useState('')

  const obtenerEjercicios = () => {
    fetch(`${API_URL}/ejercicios`)
      .then(response => response.json())
      .then(data => setEjercicios(data))
      .catch(error => console.error("Error cargando:", error))
  }

  useEffect(() => {
    obtenerEjercicios()
  }, [])

  const manejarEnvio = (e) => {
    e.preventDefault()

    const nuevo = {
      nombre,
      musculo,
      series: parseInt(series),
      repeticiones: parseInt(repeticiones)
    }

    fetch(`${API_URL}/ejercicios`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(nuevo)
    })
    .then(response => {
      if (response.ok) {
        obtenerEjercicios() 
        setNombre('')
        setSeries('')
        setRepeticiones('')
      }
    })
    .catch(error => console.error("Error al guardar:", error))
  }
  const eliminarEjercicio = (id) => {
    fetch(`${API_URL}/ejercicios/${id}`, {
      method: 'DELETE'
    })
    .then(response => {
      if (response.ok) {
        obtenerEjercicios()
      }
    })
    .catch(error => console.error("Error al eliminar:", error))
  }
  return (
    <div className="container">
      <h1>FitTrack</h1>

      {/* SECCIÓN LLENAR INFO */}
      <form onSubmit={manejarEnvio} className="formulario">
        <h2>Agregar Nuevo Entrenamiento</h2>
        <div className="input-group">
          <input 
            type="text" 
            placeholder="Nombre" 
            value={nombre} 
            onChange={(e) => setNombre(e.target.value)} 
            required 
          />
          <select value={musculo} onChange={(e) => setMusculo(e.target.value)}>
            <option value="Piernas">Piernas</option>
            <option value="Pecho">Pecho</option>
            <option value="Espalda">Espalda</option>
            <option value="Brazos">Brazos</option>
            <option value="Hombros">Hombros</option>
            <option value="Abdomen">Abdomen</option>
          </select>
          <input 
            type="number" 
            placeholder="Series" 
            value={series} 
            onChange={(e) => setSeries(e.target.value)} 
            required 
          />
          <input 
            type="number" 
            placeholder="Reps" 
            value={repeticiones} 
            onChange={(e) => setRepeticiones(e.target.value)} 
            required 
          />
        </div>
        <button type="submit" className="btn-guardar">Guardar Ejercicio</button>
      </form>

      <hr />

      {/* SECCIÓN CADA EJERCICIO */}
      <h2>Mis Rutinas Registradas</h2>
      <div className="grid">
        {ejercicios.map(ejercicio => (
          <div key={ejercicio.id} className="card">
            <div className="card-header">
              <div>
              <h3>{ejercicio.nombre}</h3>
              <span className={`badge ${ejercicio.musculo.toLowerCase()}`}>{ejercicio.musculo}</span>
            </div>
              <button 
                onClick={() => eliminarEjercicio(ejercicio.id)} 
                className="btn-eliminar"
                title="Eliminar ejercicio"
              >
                ❌
              </button>
            </div>
            <p> <strong>{ejercicio.series}</strong> series x <strong>{ejercicio.repeticiones}</strong> reps</p>
          </div>
        ))}
      </div>
    </div>
  )
}

export default App