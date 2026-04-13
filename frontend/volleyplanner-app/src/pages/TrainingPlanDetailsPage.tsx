import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import { trainingPlanService } from "../services/trainingPlanService";
import type { TrainingPlanDetails } from "../types/trainingPlan";

function TrainingPlanDetailsPage() {
  const { id } = useParams();
  const [plan, setPlan] = useState<TrainingPlanDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadPlan = async () => {
      if (!id) {
        setError("Hiányzó azonosító.");
        setLoading(false);
        return;
      }

      try {
        const data = await trainingPlanService.getById(Number(id));
        setPlan(data);
      } catch {
        setError("Nem sikerült betölteni az edzéstervet.");
      } finally {
        setLoading(false);
      }
    };

    void loadPlan();
  }, [id]);

  return (
    <AppLayout
      title="Edzésterv részletei"
      subtitle="A kiválasztott mentett terv részletes felépítése."
    >
      <div className="toolbar" style={{ marginBottom: "1.5rem" }}>
        <Link
          to="/plans"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Vissza a mentett tervekhez
        </Link>

        <Link
          to="/planner"
          className="primary-button"
          style={{ textDecoration: "none" }}
        >
          Heti tervező
        </Link>
      </div>

      {loading && <p className="info-text">Betöltés...</p>}
      {error && <p className="error-text">{error}</p>}

      {plan && (
        <>
          <section className="card" style={{ marginBottom: "1.5rem" }}>
            <h2 style={{ marginTop: 0 }}>{plan.title}</h2>

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
                  {plan.sportType}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Terv típusa</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {plan.planType}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Időtartam</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {plan.targetDuration} perc
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Intenzitás</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {plan.targetIntensity}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Nehézség</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {plan.targetLevel}
                </p>
              </div>

              <div className="card" style={{ padding: "1rem" }}>
                <strong>Fő fókusz</strong>
                <p className="info-text" style={{ marginBottom: 0 }}>
                  {plan.primaryFocus}
                </p>
              </div>
            </div>

            <div style={{ marginTop: "1rem" }}>
              <strong>Létrehozva:</strong>{" "}
              <span className="info-text">
                {new Date(plan.createdAt).toLocaleString()}
              </span>
            </div>
          </section>

          <section>
            <h2>Edzéselemek</h2>

            <div className="card-grid">
              {plan.items.map((item) => (
                <article
                  key={`${item.exerciseId}-${item.orderIndex}`}
                  className="card"
                >
                  <h3 className="exercise-card-title">
                    <Link
                      to={`/exercises/${item.exerciseId}`}
                      style={{ textDecoration: "none", color: "inherit" }}
                    >
                      {item.exerciseTitle}
                    </Link>
                  </h3>
                  <p className="exercise-meta">Sorrend: {item.orderIndex}</p>
                  <p className="exercise-meta">Szakasz: {item.sectionName}</p>
                  <p className="exercise-meta">
                    Tervezett idő: {item.plannedDuration} perc
                  </p>
                </article>
              ))}
            </div>
          </section>
        </>
      )}
    </AppLayout>
  );
}

export default TrainingPlanDetailsPage;