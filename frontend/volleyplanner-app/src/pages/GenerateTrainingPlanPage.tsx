import { useState } from "react";
import { Link } from "react-router-dom";
import { AxiosError } from "axios";
import AppLayout from "../components/layout/AppLayout";
import { generationService } from "../services/generationService";
import { trainingPlanService } from "../services/trainingPlanService";
import type {
  GenerateTrainingPlanRequest,
  GeneratedTrainingPlanResponse,
} from "../types/generation";

function GenerateTrainingPlanPage() {
  const [formData, setFormData] = useState<GenerateTrainingPlanRequest>({
    sportTypes: ["BeachVolleyball"],
    durationMin: 60,
    playerCount: 4,
    intensities: ["Medium"],
    difficulties: ["Beginner"],
    primaryFocus: "Nyitásfogadás",
  });

  const [generatedPlan, setGeneratedPlan] =
    useState<GeneratedTrainingPlanResponse | null>(null);

  const [planTitle, setPlanTitle] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [saveMessage, setSaveMessage] = useState("");
  const [saveError, setSaveError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;

    if (name === "durationMin" || name === "playerCount") {
      setFormData((prev) => ({
        ...prev,
        [name]: Number(value),
      }));
      return;
    }

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const toggleChoiceValue = (
    field: "sportTypes" | "difficulties" | "intensities",
    value: string
  ) => {
    setFormData((prev) => {
      const currentValues = prev[field];
      const exists = currentValues.includes(value);

      return {
        ...prev,
        [field]: exists
          ? currentValues.filter((item) => item !== value)
          : [...currentValues, value],
      };
    });
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();

    setError("");
    setSaveMessage("");
    setSaveError("");
    setGeneratedPlan(null);
    setPlanTitle("");
    setLoading(true);

    try {
      const result = await generationService.generate(formData);
      setGeneratedPlan(result);
      setPlanTitle(`${result.sportType} terv - ${result.primaryFocus}`);
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setError(err.response?.data?.message || "Sikertelen generálás.");
      } else {
        setError("Sikertelen generálás.");
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSavePlan = async () => {
    if (!generatedPlan) return;

    const trimmedTitle = planTitle.trim();

    if (!trimmedTitle) {
      setSaveError("Adj meg nevet a mentett edzéstervhez.");
      setSaveMessage("");
      return;
    }

    setSaveMessage("");
    setSaveError("");
    setSaving(true);

    try {
      const payload = {
        title: trimmedTitle,
        sportType: generatedPlan.sportType,
        planType: "Single",
        targetDuration: generatedPlan.targetDuration,
        targetIntensity: generatedPlan.intensity,
        targetLevel: generatedPlan.difficulty,
        primaryFocus: generatedPlan.primaryFocus,
        items: generatedPlan.items.map((item, index) => ({
          exerciseId: item.exerciseId,
          orderIndex: index + 1,
          sectionName: item.sectionName,
          plannedDuration: item.plannedDuration,
        })),
      };

      await trainingPlanService.create(payload);
      setSaveMessage("Az edzésterv sikeresen elmentve.");
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setSaveError(
          err.response?.data?.message ||
            "Nem sikerült elmenteni az edzéstervet."
        );
      } else {
        setSaveError("Nem sikerült elmenteni az edzéstervet.");
      }
    } finally {
      setSaving(false);
    }
  };

  return (
    <AppLayout
      title="Edzésterv generálása"
      subtitle="Állítsd be a fő paramétereket, és a rendszer összeállít egy edzésterv-javaslatot."
    >
      <div className="toolbar" style={{ marginBottom: "1.5rem" }}>
        <Link
          to="/exercises"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Gyakorlatok
        </Link>

        <Link
          to="/plans"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Mentett tervek
        </Link>

        <Link
          to="/planner"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Heti tervező
        </Link>
      </div>

      <section className="card filter-card" style={{ marginBottom: "1.5rem" }}>
        <h2 style={{ marginTop: 0 }}>Generálási beállítások</h2>

        <form onSubmit={handleSubmit}>
          <div className="filter-sections" style={{ marginBottom: "1rem" }}>
            <div className="form-field">
              <label>Sportág</label>
              <div className="choice-group">
                {["BeachVolleyball", "Gym"].map((value) => (
                  <button
                    key={value}
                    type="button"
                    className={`choice-chip ${
                      formData.sportTypes.includes(value) ? "active" : ""
                    }`}
                    onClick={() => toggleChoiceValue("sportTypes", value)}
                  >
                    {value}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-field">
              <label htmlFor="durationMin">Időtartam (perc)</label>
              <input
                id="durationMin"
                name="durationMin"
                type="number"
                min={10}
                value={formData.durationMin}
                onChange={handleInputChange}
              />
            </div>

            <div className="form-field">
              <label htmlFor="playerCount">Játékosok száma</label>
              <input
                id="playerCount"
                name="playerCount"
                type="number"
                min={1}
                value={formData.playerCount}
                onChange={handleInputChange}
              />
            </div>

            <div className="form-field">
              <label>Nehézség</label>
              <div className="choice-group">
                {["Beginner", "Intermediate", "Advanced"].map((value) => (
                  <button
                    key={value}
                    type="button"
                    className={`choice-chip ${
                      formData.difficulties.includes(value) ? "active" : ""
                    }`}
                    onClick={() => toggleChoiceValue("difficulties", value)}
                  >
                    {value}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-field">
              <label>Intenzitás</label>
              <div className="choice-group">
                {["Low", "Medium", "High"].map((value) => (
                  <button
                    key={value}
                    type="button"
                    className={`choice-chip ${
                      formData.intensities.includes(value) ? "active" : ""
                    }`}
                    onClick={() => toggleChoiceValue("intensities", value)}
                  >
                    {value}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-field">
              <label htmlFor="primaryFocus">Fő fókusz</label>
              <select
                id="primaryFocus"
                name="primaryFocus"
                value={formData.primaryFocus}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    primaryFocus: e.target.value,
                  }))
                }
              >
                <option value="Nyitás">Nyitás</option>
                <option value="Nyitásfogadás">Nyitásfogadás</option>
                <option value="Feladás">Feladás</option>
                <option value="Támadás">Támadás</option>
                <option value="Blokk/Védekezés">Blokk/Védekezés</option>
              </select>
            </div>
          </div>

          <div className="toolbar">
            <button className="primary-button" type="submit" disabled={loading}>
              {loading ? "Generálás..." : "Edzésterv generálása"}
            </button>
          </div>
        </form>

        {error && (
          <p className="error-text" style={{ marginTop: "1rem" }}>
            {error}
          </p>
        )}
      </section>

      {generatedPlan && (
        <section className="card">
          <h2 style={{ marginTop: 0 }}>Generált edzésterv</h2>

          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
              gap: "1rem",
              marginBottom: "1.5rem",
            }}
          >
            <div className="card" style={{ padding: "1rem" }}>
              <strong>Sportág</strong>
              <p className="info-text" style={{ marginBottom: 0 }}>
                {generatedPlan.sportType}
              </p>
            </div>

            <div className="card" style={{ padding: "1rem" }}>
              <strong>Időtartam</strong>
              <p className="info-text" style={{ marginBottom: 0 }}>
                {generatedPlan.targetDuration} perc
              </p>
            </div>

            <div className="card" style={{ padding: "1rem" }}>
              <strong>Nehézségek</strong>
              <p className="info-text" style={{ marginBottom: 0 }}>
                {generatedPlan.difficulty}
              </p>
            </div>

            <div className="card" style={{ padding: "1rem" }}>
              <strong>Intenzitások</strong>
              <p className="info-text" style={{ marginBottom: 0 }}>
                {generatedPlan.intensity}
              </p>
            </div>
          </div>

          <div style={{ marginBottom: "1rem" }}>
            <strong>Fő fókusz:</strong>{" "}
            <span className="info-text">{generatedPlan.primaryFocus}</span>
          </div>

          <div className="form-field" style={{ marginBottom: "1.5rem" }}>
            <label htmlFor="planTitle">Mentett edzésterv neve</label>
            <input
              id="planTitle"
              type="text"
              value={planTitle}
              onChange={(e) => setPlanTitle(e.target.value)}
              placeholder="Adj nevet az edzéstervnek"
            />
          </div>

          <h3>Edzéselemek</h3>

          <div className="card-grid">
            {generatedPlan.items.map((item, index) => (
              <article key={`${item.exerciseId}-${index}`} className="card">
                <h3 className="exercise-card-title">
                  <Link
                    to={`/exercises/${item.exerciseId}`}
                    style={{ textDecoration: "none", color: "inherit" }}
                  >
                    {item.exerciseTitle}
                  </Link>
                </h3>
                <p className="exercise-meta">Szakasz: {item.sectionName}</p>
                <p className="exercise-meta">
                  Tervezett idő: {item.plannedDuration} perc
                </p>
              </article>
            ))}
          </div>

          <div className="toolbar" style={{ marginTop: "1.5rem" }}>
            <button
              className="primary-button"
              onClick={handleSavePlan}
              disabled={saving}
            >
              {saving ? "Mentés..." : "Edzésterv mentése"}
            </button>
          </div>

          {saveMessage && (
            <p className="success-text" style={{ marginTop: "1rem" }}>
              {saveMessage}
            </p>
          )}

          {saveError && (
            <p className="error-text" style={{ marginTop: "1rem" }}>
              {saveError}
            </p>
          )}
        </section>
      )}
    </AppLayout>
  );
}

export default GenerateTrainingPlanPage;