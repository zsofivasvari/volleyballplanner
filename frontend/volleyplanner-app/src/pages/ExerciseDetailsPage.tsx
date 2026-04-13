import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
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
    <AppLayout
      title="Gyakorlat részletei"
      subtitle="A kiválasztott gyakorlat teljes leírása és jellemzői."
    >
      <div className="toolbar" style={{ marginBottom: "1.5rem" }}>
        <Link
          to="/exercises"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Vissza a gyakorlatokhoz
        </Link>

        <Link
          to="/generate"
          className="primary-button"
          style={{ textDecoration: "none" }}
        >
          Edzésterv generálás
        </Link>
      </div>

      {loading && <p className="info-text">Betöltés...</p>}
      {error && <p className="error-text">{error}</p>}

      {exercise && (
        <>
          <section className="card" style={{ marginBottom: "1.5rem" }}>
            <h2 style={{ marginTop: 0 }}>{exercise.title}</h2>

            <div
              style={{
                display: "grid",
                gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
                gap: "1rem",
                marginTop: "1rem",
              }}
            >
              <div className="card" style={{ padding: "1rem" }}>
                <strong>Sportág</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {exercise.sportType}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Időtartam</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {exercise.durationMin} perc
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Nehézség</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {exercise.difficulty}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Intenzitás</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {exercise.intensity}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Játékosok</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {exercise.minPlayers} - {exercise.maxPlayers}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Fázis</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {exercise.phase}
                </p>
              </div>
            </div>
          </section>

          <section className="card" style={{ marginBottom: "1.5rem" }}>
            <h3 style={{ marginTop: 0 }}>Leírás</h3>
            <p className="info-text" style={{ whiteSpace: "pre-line" }}>
              {exercise.description}
            </p>
          </section>

          <section className="card">
            <h3 style={{ marginTop: 0 }}>Tagek</h3>

            {exercise.tags.length === 0 ? (
              <p className="info-text">Ehhez a gyakorlathoz nincs tag megadva.</p>
            ) : (
              <div className="choice-group">
                {exercise.tags.map((tag) => (
                  <span key={tag} className="choice-chip active">
                    {tag}
                  </span>
                ))}
              </div>
            )}
          </section>
        </>
      )}
    </AppLayout>
  );
}

export default ExerciseDetailsPage;