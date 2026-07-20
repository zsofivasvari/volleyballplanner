import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { AxiosError } from "axios";
import AppLayout from "../components/layout/AppLayout";
import { generationService } from "../services/generationService";
import { trainingPlanService } from "../services/trainingPlanService";
import type {
  GenerateTrainingPlanRequest,
  GeneratedTrainingPlanResponse,
} from "../types/generation";

type SupportedSportType = "BeachVolleyball" | "Gym";

const beachFocusOptions = [
  "Nyitás",
  "Nyitásfogadás",
  "Feladás",
  "Támadás",
  "Blokk/Védekezés",
  "Állóképesség",
];

const gymFocusOptions = [
  "Alsótest",
  "Törzs",
  "Felsőtest",
  "Fullbody",
  "Állóképesség",
];

const durationPresets = [60, 90, 120];

const getFocusOptionsForSport = (sportType: SupportedSportType) => {
  return sportType === "Gym" ? gymFocusOptions : beachFocusOptions;
};

function GenerateTrainingPlanPage() {
  const [formData, setFormData] = useState<GenerateTrainingPlanRequest>({
    sportTypes: ["BeachVolleyball"],
    durationMin: 60,
    playerCount: 4,
    intensities: ["Medium"],
    difficulties: ["Beginner"],
    focusAreas: ["Nyitásfogadás"],
  });

  const [generatedPlan, setGeneratedPlan] =
    useState<GeneratedTrainingPlanResponse | null>(null);

  const [customPlanTitle, setCustomPlanTitle] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [saveMessage, setSaveMessage] = useState("");
  const [saveError, setSaveError] = useState("");
  const [saving, setSaving] = useState(false);

  const selectedSport: SupportedSportType =
    formData.sportTypes[0] === "Gym" ? "Gym" : "BeachVolleyball";

  const isGym = selectedSport === "Gym";

  const availableFocusOptions = useMemo(() => {
    return getFocusOptionsForSport(selectedSport);
  }, [selectedSport]);

  const formattedSport = isGym ? "Gym" : "Beach Volleyball";

  const activeProfileParts = [
    formattedSport,
    `${formData.durationMin} perc`,
    !isGym && formData.playerCount ? `${formData.playerCount} fő` : null,
    formData.difficulties.join(", "),
    formData.intensities.join(", "),
  ].filter(Boolean);

  const activeProfileText = activeProfileParts.join(" · ");

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;

    if (name === "durationMin") {
      setFormData((prev) => ({
        ...prev,
        durationMin: Math.max(10, Number(value) || 10),
      }));
    }

    if (name === "playerCount") {
      setFormData((prev) => ({
        ...prev,
        playerCount: Math.max(1, Number(value) || 1),
      }));
    }
  };

  const changeDuration = (delta: number) => {
    setFormData((prev) => ({
      ...prev,
      durationMin: Math.max(10, prev.durationMin + delta),
    }));
  };

  const setDurationPreset = (duration: number) => {
    setFormData((prev) => ({
      ...prev,
      durationMin: duration,
    }));
  };

  const changePlayerCount = (delta: number) => {
    setFormData((prev) => ({
      ...prev,
      playerCount: Math.max(1, (prev.playerCount ?? 1) + delta),
    }));
  };

  const handleSportChange = (sportType: SupportedSportType) => {
    const availableFocuses = getFocusOptionsForSport(sportType);

    setGeneratedPlan(null);
    setCustomPlanTitle("");
    setError("");
    setSaveError("");
    setSaveMessage("");

    setFormData((prev) => ({
      ...prev,
      sportTypes: [sportType],
      playerCount: sportType === "Gym" ? null : prev.playerCount ?? 4,
      focusAreas: [availableFocuses[0]],
    }));
  };

  const toggleChoiceValue = (
    field: "difficulties" | "intensities" | "focusAreas",
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

  const removeFocusArea = (focus: string) => {
    setFormData((prev) => ({
      ...prev,
      focusAreas: prev.focusAreas.filter((item) => item !== focus),
    }));
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();

    setError("");
    setSaveMessage("");
    setSaveError("");
    setGeneratedPlan(null);
    setCustomPlanTitle("");
    setLoading(true);

    if (formData.sportTypes.length !== 1) {
      setError("Egyszerre pontosan egy sportág választható.");
      setLoading(false);
      return;
    }

    if (formData.durationMin < 10) {
      setError("Az edzés időtartama legalább 10 perc legyen.");
      setLoading(false);
      return;
    }

    if (!isGym && (!formData.playerCount || formData.playerCount < 1)) {
      setError("Strandröplabda tervnél a játékosok száma legalább 1 fő legyen.");
      setLoading(false);
      return;
    }

    if (formData.difficulties.length === 0) {
      setError("Legalább egy nehézség kiválasztása kötelező.");
      setLoading(false);
      return;
    }

    if (formData.intensities.length === 0) {
      setError("Legalább egy intenzitás kiválasztása kötelező.");
      setLoading(false);
      return;
    }

    if (formData.focusAreas.length === 0) {
      setError("Legalább egy fókuszterület kiválasztása kötelező.");
      setLoading(false);
      return;
    }

    const invalidFocus = formData.focusAreas.some(
      (focus) => !availableFocusOptions.includes(focus)
    );

    if (invalidFocus) {
      setError("A kiválasztott sportághoz érvénytelen fókuszterület tartozik.");
      setLoading(false);
      return;
    }

    const request: GenerateTrainingPlanRequest = {
      ...formData,
      playerCount: isGym ? null : formData.playerCount,
    };

    try {
      const result = await generationService.generate(request);
      setGeneratedPlan(result);

      setCustomPlanTitle(
        `${result.sportType} terv - ${result.focusAreas.join(", ")}`
      );
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
    if (!generatedPlan) {
      return;
    }

    setSaveMessage("");
    setSaveError("");
    setSaving(true);

    const finalTitle = customPlanTitle.trim();

    if (!finalTitle) {
      setSaveError("Adj meg egy nevet az edzéstervnek.");
      setSaving(false);
      return;
    }

    try {
      await trainingPlanService.create({
  title: finalTitle,
  sportType: generatedPlan.sportType,
  planType: "Single",
  targetDuration: generatedPlan.targetDuration,
  targetIntensity: generatedPlan.intensity,
  targetLevel: generatedPlan.difficulty,
  primaryFocus: generatedPlan.focusAreas.join(", "),
  playerCount:
    generatedPlan.sportType === "BeachVolleyball"
      ? formData.playerCount ?? null
      : null,
  items: generatedPlan.items.map((item, index) => ({
    exerciseId: item.exerciseId,
    orderIndex: index + 1,
    sectionName: item.sectionName,
    plannedDuration: item.plannedDuration,
  })),
});

      setSaveMessage("Az edzésterv sikeresen elmentve.");
    } catch {
      setSaveError("Nem sikerült elmenteni az edzéstervet.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <AppLayout
      title="Edzésterv generálása"
      subtitle="Állítsd be a fő paramétereket, és a rendszer összeállít egy személyre szabott edzésterv-javaslatot."
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

      <section className="card generation-studio-card">
        <div className="generation-studio-heading">
          <div>
            <p className="generation-studio-kicker">Training Plan Builder</p>
            <h2>Építs tudatos, célzott edzéstervet</h2>
            <p>
              Válassz sportágat, állítsd be a terhelési profilt, majd add meg,
              mely fókuszterületek köré épüljön a terv.
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="generation-sport-grid">
            <button
              type="button"
              className={`generation-sport-card ${
                selectedSport === "BeachVolleyball" ? "active" : ""
              }`}
              onClick={() => handleSportChange("BeachVolleyball")}
            >
              <span className="generation-sport-orb generation-sport-orb-beach" />
              <strong>Beach Volleyball</strong>
            </button>

            <button
              type="button"
              className={`generation-sport-card ${
                selectedSport === "Gym" ? "active" : ""
              }`}
              onClick={() => handleSportChange("Gym")}
            >
              <span className="generation-sport-orb generation-sport-orb-gym" />
              <strong>Gym</strong>
            </button>
          </div>

          <div className={`generation-config-grid ${isGym ? "generation-config-grid-gym" : ""}`}>
            <section className="generation-panel generation-core-controls">
              <div className="generation-panel-heading">
                <span>Alapparaméterek</span>
              </div>

              <div
                className={`generation-stepper-grid ${
                  isGym ? "generation-stepper-grid-single" : ""
                }`}
              >
                <div className="generation-stepper-card">
                  <label htmlFor="durationMin">Időtartam</label>

                  <div className="generation-stepper">
                    <button
                      type="button"
                      className="generation-stepper-button"
                      onClick={() => changeDuration(-5)}
                      aria-label="Időtartam csökkentése"
                    >
                      −
                    </button>

                    <div className="generation-stepper-value">
                      <input
                        id="durationMin"
                        name="durationMin"
                        type="number"
                        min={10}
                        value={formData.durationMin}
                        onChange={handleInputChange}
                      />
                      <span>perc</span>
                    </div>

                    <button
                      type="button"
                      className="generation-stepper-button"
                      onClick={() => changeDuration(5)}
                      aria-label="Időtartam növelése"
                    >
                      +
                    </button>
                  </div>

                  <div className="generation-duration-presets">
                    {durationPresets.map((duration) => (
                      <button
                        key={duration}
                        type="button"
                        className={`generation-duration-preset ${
                          formData.durationMin === duration ? "active" : ""
                        }`}
                        onClick={() => setDurationPreset(duration)}
                      >
                        {duration} perc
                      </button>
                    ))}
                  </div>
                </div>

                {!isGym && (
                  <div className="generation-stepper-card">
                    <label htmlFor="playerCount">Játékosok száma</label>

                    <div className="generation-stepper">
                      <button
                        type="button"
                        className="generation-stepper-button"
                        onClick={() => changePlayerCount(-1)}
                        aria-label="Játékosszám csökkentése"
                      >
                        −
                      </button>

                      <div className="generation-stepper-value">
                        <input
                          id="playerCount"
                          name="playerCount"
                          type="number"
                          min={1}
                          value={formData.playerCount ?? 1}
                          onChange={handleInputChange}
                        />
                        <span>fő</span>
                      </div>

                      <button
                        type="button"
                        className="generation-stepper-button"
                        onClick={() => changePlayerCount(1)}
                        aria-label="Játékosszám növelése"
                      >
                        +
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </section>

            <section className="generation-panel">
              <div className="generation-panel-heading">
                <span>Terhelési profil</span>
              </div>

              <div className="generation-choice-block">
                <p>Nehézség</p>

                <div className="generation-pill-group">
                  {["Beginner", "Intermediate", "Advanced"].map((value) => (
                    <button
                      key={value}
                      type="button"
                      className={`generation-pill ${
                        formData.difficulties.includes(value) ? "active" : ""
                      }`}
                      onClick={() => toggleChoiceValue("difficulties", value)}
                    >
                      {value}
                    </button>
                  ))}
                </div>
              </div>

              <div className="generation-choice-block">
                <p>Intenzitás</p>

                <div className="generation-pill-group">
                  {["Low", "Medium", "High"].map((value) => (
                    <button
                      key={value}
                      type="button"
                      className={`generation-pill ${
                        formData.intensities.includes(value) ? "active" : ""
                      }`}
                      onClick={() => toggleChoiceValue("intensities", value)}
                    >
                      {value}
                    </button>
                  ))}
                </div>
              </div>
            </section>
          </div>

          <section className="generation-focus-panel">
            <div className="generation-focus-header">
              <div>
                <span>Fókuszterületek</span>
              </div>
            </div>

            <div className="generation-focus-grid">
              {availableFocusOptions.map((focus) => (
                <button
                  key={focus}
                  type="button"
                  className={`generation-focus-card ${
                    formData.focusAreas.includes(focus) ? "active" : ""
                  }`}
                  onClick={() => toggleChoiceValue("focusAreas", focus)}
                >
                  <strong>{focus}</strong>
                </button>
              ))}
            </div>
          </section>

          <section className="generation-profile-summary">
            <div className="generation-profile-head">
              <strong>Aktuális generálási profil</strong>
              <span>{formattedSport}</span>
            </div>

            <p>{activeProfileText}</p>

            {formData.focusAreas.length > 0 ? (
              <div className="generation-selected-focuses">
                {formData.focusAreas.map((focus) => (
                  <button
                    key={focus}
                    type="button"
                    onClick={() => removeFocusArea(focus)}
                  >
                    <span>{focus}</span>
                    <b>×</b>
                  </button>
                ))}
              </div>
            ) : (
              <p className="generation-empty-focus">
                Még nincs fókuszterület kiválasztva.
              </p>
            )}
          </section>

          <div className="generation-actions">
            <button className="primary-button" type="submit" disabled={loading}>
              {loading ? "Generálás..." : "Edzésterv generálása"}
            </button>
          </div>
        </form>

        {error && (
          <p className="error-text generation-feedback-text">
            {error}
          </p>
        )}
      </section>

      {generatedPlan && (
        <section className="card generated-plan-showcase">
          <div className="generated-plan-header">
            <div>
              <p className="generation-studio-kicker">Generated Plan</p>
              <h2>Elkészült az edzésterved</h2>
              <p>
                A terv a kiválasztott paraméterek és fókuszterületek alapján
                állt össze.
              </p>
            </div>
          </div>

          <div className="generated-plan-stat-grid">
            <div className="generated-plan-stat">
              <span>Sportág</span>
              <strong>{generatedPlan.sportType}</strong>
            </div>

            <div className="generated-plan-stat">
              <span>Időtartam</span>
              <strong>{generatedPlan.targetDuration} perc</strong>
            </div>

            <div className="generated-plan-stat">
              <span>Nehézség</span>
              <strong>{generatedPlan.difficulty}</strong>
            </div>

            <div className="generated-plan-stat">
              <span>Intenzitás</span>
              <strong>{generatedPlan.intensity}</strong>
            </div>
          </div>

          <div className="generated-plan-focus-summary">
            <strong>Fókuszterületek</strong>

            <div>
              {generatedPlan.focusAreas.map((focus) => (
                <span key={focus}>{focus}</span>
              ))}
            </div>
          </div>

          <div className="generated-items-list">
            {generatedPlan.items.map((item, index) => (
              <article
                key={`${item.exerciseId}-${index}`}
                className="generated-item-card"
              >
                <div className="generated-item-index">{index + 1}</div>

                <div>
                  <h3>
                    <Link to={`/exercises/${item.exerciseId}`}>
                      {item.exerciseTitle}
                    </Link>
                  </h3>

                  <p>
                    {item.sectionName} · {item.plannedDuration} perc
                  </p>
                </div>
              </article>
            ))}
          </div>

          <div className="generated-save-panel">
            <div className="form-field">
              <label htmlFor="customPlanTitle">Mentett terv neve</label>
              <input
                id="customPlanTitle"
                type="text"
                value={customPlanTitle}
                onChange={(e) => setCustomPlanTitle(e.target.value)}
                placeholder="Például: Alsótest és törzs kondi edzés"
              />
            </div>

            <div className="toolbar">
              <button
                className="primary-button"
                onClick={handleSavePlan}
                disabled={saving}
              >
                {saving ? "Mentés..." : "Edzésterv mentése"}
              </button>
            </div>
          </div>

          {saveMessage && (
            <p className="success-text generation-feedback-text">
              {saveMessage}
            </p>
          )}

          {saveError && (
            <p className="error-text generation-feedback-text">
              {saveError}
            </p>
          )}
        </section>
      )}
    </AppLayout>
  );
}

export default GenerateTrainingPlanPage;