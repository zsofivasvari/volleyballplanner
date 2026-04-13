import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import { trainingPlanService } from "../services/trainingPlanService";
import type { TrainingPlanListItem } from "../types/trainingPlan";

function SavedPlansPage() {
  const [plans, setPlans] = useState<TrainingPlanListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadPlans = async () => {
      try {
        const data = await trainingPlanService.getAll();
        setPlans(data);
      } catch {
        setError("Nem sikerült betölteni a mentett terveket.");
      } finally {
        setLoading(false);
      }
    };

    void loadPlans();
  }, []);

  return (
    <AppLayout
      title="Mentett edzéstervek"
      subtitle="Itt találod a korábban generált és elmentett edzésterveidet."
    >
      <div className="toolbar" style={{ marginBottom: "1.5rem" }}>
        <Link
          to="/generate"
          className="primary-button"
          style={{ textDecoration: "none" }}
        >
          Új terv generálása
        </Link>

        <Link
          to="/planner"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Heti tervező
        </Link>
      </div>

      {loading && <p className="info-text">Betöltés...</p>}
      {error && <p className="error-text">{error}</p>}

      {!loading && !error && plans.length === 0 && (
        <div className="card">
          <p className="info-text" style={{ margin: 0 }}>
            Még nincs mentett edzésterved.
          </p>
        </div>
      )}

      {!loading && !error && plans.length > 0 && (
        <section className="card-grid">
          {plans.map((plan) => (
            <article key={plan.id} className="card">
              <h3 className="exercise-card-title">{plan.title}</h3>

              <p className="exercise-meta">Sportág: {plan.sportType}</p>
              <p className="exercise-meta">
                Időtartam: {plan.targetDuration} perc
              </p>
              <p className="exercise-meta">Fő fókusz: {plan.primaryFocus}</p>
              <p className="exercise-meta">
                Létrehozva: {new Date(plan.createdAt).toLocaleString("hu-HU")}
              </p>

              <div className="toolbar" style={{ marginTop: "1rem" }}>
                <Link
                  to={`/plans/${plan.id}`}
                  className="primary-button"
                  style={{ textDecoration: "none" }}
                >
                  Részletek
                </Link>

                <Link
                  to={`/planner?planId=${plan.id}`}
                  className="secondary-button"
                  style={{ textDecoration: "none" }}
                >
                  Hozzáadás a heti tervezőhöz
                </Link>
              </div>
            </article>
          ))}
        </section>
      )}
    </AppLayout>
  );
}

export default SavedPlansPage;