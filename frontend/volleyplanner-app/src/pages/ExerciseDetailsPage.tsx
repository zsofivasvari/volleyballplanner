import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { exerciseService } from "../services/exerciseService";
import type { ExerciseDetails } from "../types/exercise";

function ExerciseDetailsPage() {
  const { id } = useParams();
  const [exercise, setExercise] = useState<ExerciseDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadExercise = async () => {
      if (!id) {
        setError("Hiányzó azonosító.");
        setLoading(false);
        return;
      }

      try {
        const data = await exerciseService.getById(Number(id));
        setExercise(data);
      } catch {
        setError("Nem sikerült betölteni a gyakorlat részleteit.");
      } finally {
        setLoading(false);
      }
    };

    void loadExercise();
  }, [id]);

  return (
    <div style={{ padding: "2rem" }}>
      <Link to="/exercises">← Vissza a listához</Link>

      {loading && <p>Betöltés...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      {exercise && (
        <div style={{ marginTop: "1rem" }}>
          <h1>{exercise.title}</h1>
          <p>{exercise.description}</p>
          <p>Sportág: {exercise.sportType}</p>
          <p>Időtartam: {exercise.durationMin} perc</p>
          <p>Nehézség: {exercise.difficulty}</p>
          <p>Intenzitás: {exercise.intensity}</p>
          <p>
            Játékosok: {exercise.minPlayers} - {exercise.maxPlayers}
          </p>
          <p>Fázis: {exercise.phase}</p>

          <h3>Címkék</h3>
          {exercise.tags.length > 0 ? (
            <ul>
              {exercise.tags.map((tag) => (
                <li key={tag}>{tag}</li>
              ))}
            </ul>
          ) : (
            <p>Nincsenek címkék.</p>
          )}
        </div>
      )}
    </div>
  );
}

export default ExerciseDetailsPage;