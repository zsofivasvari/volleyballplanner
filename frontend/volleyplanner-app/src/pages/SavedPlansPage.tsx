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
        setError("Nem sikerült betölteni a mentett edzésterveket.");
      } finally {
        setLoading(false);
      }
    };

    void loadPlans();
  }, []);

  const isGymPlan = (plan: TrainingPlanListItem) => {
    const sportType = String(plan.sportType ?? "").toLowerCase();
    const title = String(plan.title ?? "").toLowerCase();

    return (
      sportType.includes("gym") ||
      sportType.includes("kondi") ||
      title.includes("gym") ||
      title.includes("kondi") ||
      sportType === "3"
    );
  };

  const getPlanImage = (plan: TrainingPlanListItem) => {
    return isGymPlan(plan) ? "/images/gym.png?v=2" : "/images/bv.png?v=2";
  };

  const getSportLabel = (plan: TrainingPlanListItem) => {
    return isGymPlan(plan) ? "Konditermi edzés" : "Strandröplabda edzés";
  };

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
        <section className="card">
          <p className="info-text">
            Még nincs mentett edzésterved. Generálj egy új tervet, majd mentsd
            el.
          </p>
        </section>
      )}

      {!loading && !error && plans.length > 0 && (
        <section className="saved-plan-list">
          {plans.map((plan) => (
            <article className="saved-plan-card card" key={plan.id}>
              <div className="saved-plan-content">
                <div>
                  <span className="saved-plan-badge">
                    {getSportLabel(plan)}
                  </span>

                  <h2 className="saved-plan-title">{plan.title}</h2>

                  <div className="saved-plan-meta">
                    <span>Sportág: {plan.sportType}</span>
                    <span>Időtartam: {plan.targetDuration} perc</span>
                    <span>Fő fókusz: {plan.primaryFocus}</span>
                    <span>
                      Létrehozva:{" "}
                      {new Date(plan.createdAt).toLocaleString("hu-HU")}
                    </span>
                  </div>

                  <div className="saved-plan-actions">
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
                </div>

                <img
                  className="saved-plan-image"
                  src={getPlanImage(plan)}
                  alt={getSportLabel(plan)}
                />
              </div>
            </article>
          ))}
        </section>
      )}
    </AppLayout>
  );
}

export default SavedPlansPage;