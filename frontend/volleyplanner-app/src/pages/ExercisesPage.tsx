import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { exerciseService } from "../services/exerciseService";
import type { ExerciseListItem, ExerciseQuery } from "../types/exercise";

function ExercisesPage() {
  const [exercises, setExercises] = useState<ExerciseListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [filters, setFilters] = useState<ExerciseQuery>({
    sportType: "",
    difficulty: "",
    intensity: "",
    phase: "",
  });

  const loadExercises = async (currentFilters?: ExerciseQuery) => {
    setLoading(true);
    setError("");

    try {
      const cleanedFilters: ExerciseQuery = {
        sportType: currentFilters?.sportType || undefined,
        difficulty: currentFilters?.difficulty || undefined,
        intensity: currentFilters?.intensity || undefined,
        phase: currentFilters?.phase || undefined,
      };

      const data = await exerciseService.getAll(cleanedFilters);
      setExercises(data);
    } catch {
      setError("Nem sikerült betölteni a gyakorlatokat.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadExercises(filters);
  }, []);

  const handleFilterChange = (
    e: React.ChangeEvent<HTMLSelectElement>
  ) => {
    const { name, value } = e.target;

    setFilters((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleApplyFilters = async () => {
    await loadExercises(filters);
  };

  const handleResetFilters = async () => {
    const resetFilters = {
      sportType: "",
      difficulty: "",
      intensity: "",
      phase: "",
    };

    setFilters(resetFilters);
    await loadExercises(resetFilters);
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userEmail");
    window.location.href = "/";
  };

  return (
    <div style={{ padding: "2rem" }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "2rem",
        }}
      >
        <h1>Gyakorlatok</h1>

        <div style={{ display: "flex", gap: "1rem" }}>
          <Link to="/exercises/new">Új gyakorlat</Link>
          <button onClick={handleLogout}>Kijelentkezés</button>
        </div>
      </div>

      <div
        style={{
          border: "1px solid #ccc",
          borderRadius: "8px",
          padding: "1rem",
          marginBottom: "2rem",
        }}
      >
        <h2>Szűrők</h2>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
            gap: "1rem",
            marginBottom: "1rem",
          }}
        >
          <div>
            <label htmlFor="sportType">Sportág</label>
            <select
              id="sportType"
              name="sportType"
              value={filters.sportType}
              onChange={handleFilterChange}
              style={{ display: "block", width: "100%", padding: "0.5rem" }}
            >
              <option value="">Összes</option>
              <option value="BeachVolleyball">BeachVolleyball</option>
              <option value="Gym">Gym</option>
            </select>
          </div>

          <div>
            <label htmlFor="difficulty">Nehézség</label>
            <select
              id="difficulty"
              name="difficulty"
              value={filters.difficulty}
              onChange={handleFilterChange}
              style={{ display: "block", width: "100%", padding: "0.5rem" }}
            >
              <option value="">Összes</option>
              <option value="Beginner">Beginner</option>
              <option value="Intermediate">Intermediate</option>
              <option value="Advanced">Advanced</option>
            </select>
          </div>

          <div>
            <label htmlFor="intensity">Intenzitás</label>
            <select
              id="intensity"
              name="intensity"
              value={filters.intensity}
              onChange={handleFilterChange}
              style={{ display: "block", width: "100%", padding: "0.5rem" }}
            >
              <option value="">Összes</option>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </div>

          <div>
            <label htmlFor="phase">Fázis</label>
            <select
              id="phase"
              name="phase"
              value={filters.phase}
              onChange={handleFilterChange}
              style={{ display: "block", width: "100%", padding: "0.5rem" }}
            >
              <option value="">Összes</option>
              <option value="Warmup">Warmup</option>
              <option value="Main">Main</option>
              <option value="Cooldown">Cooldown</option>
            </select>
          </div>
        </div>

        <div style={{ display: "flex", gap: "1rem" }}>
          <button onClick={handleApplyFilters}>Szűrés alkalmazása</button>
          <button onClick={handleResetFilters}>Szűrők törlése</button>
        </div>
      </div>

      {loading && <p>Betöltés...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      {!loading && !error && exercises.length === 0 && (
        <p>Nincs a szűrésnek megfelelő gyakorlat.</p>
      )}

      {!loading && !error && exercises.length > 0 && (
        <div style={{ display: "grid", gap: "1rem" }}>
          {exercises.map((exercise) => (
            <div
              key={exercise.id}
              style={{
                border: "1px solid #ccc",
                padding: "1rem",
                borderRadius: "8px",
              }}
            >
              <h3>{exercise.title}</h3>
              <p>Sportág: {exercise.sportType}</p>
              <p>Időtartam: {exercise.durationMin} perc</p>
              <p>Nehézség: {exercise.difficulty}</p>
              <p>Intenzitás: {exercise.intensity}</p>
              <p>
                Játékosok: {exercise.minPlayers} - {exercise.maxPlayers}
              </p>
              <p>Fázis: {exercise.phase}</p>

              <Link to={`/exercises/${exercise.id}`}>Részletek</Link>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default ExercisesPage;