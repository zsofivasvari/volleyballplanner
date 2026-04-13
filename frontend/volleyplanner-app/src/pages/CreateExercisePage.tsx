import { useEffect, useMemo, useState } from "react";
import { AxiosError } from "axios";
import { Link, useNavigate } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import { exerciseService } from "../services/exerciseService";
import { tagService } from "../services/tagService";
import { isAdmin } from "../utils/auth";
import type { Tag } from "../types/tag";

function CreateExercisePage() {
  const navigate = useNavigate();

  const [tags, setTags] = useState<Tag[]>([]);
  const [loadingTags, setLoadingTags] = useState(true);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const [formData, setFormData] = useState({
    title: "",
    description: "",
    sportType: "BeachVolleyball",
    durationMin: 10,
    difficulty: "Beginner",
    intensity: "Low",
    minPlayers: 1,
    maxPlayers: 2,
    phase: "Warmup",
    tagIds: [] as number[],
  });

  const admin = isAdmin();

  useEffect(() => {
    if (!admin) {
      return;
    }

    const loadTags = async () => {
      try {
        const data = await tagService.getAll();
        setTags(data);
      } catch {
        setError("Nem sikerült betölteni a tageket.");
      } finally {
        setLoadingTags(false);
      }
    };

    void loadTags();
  }, [admin]);

  const focusTags = useMemo(
    () => tags.filter((tag) => tag.type === "Focus"),
    [tags]
  );

  const otherTags = useMemo(
    () => tags.filter((tag) => tag.type !== "Focus"),
    [tags]
  );

  const handleInputChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const { name, value } = e.target;

    if (["durationMin", "minPlayers", "maxPlayers"].includes(name)) {
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

  const handleChoiceChange = (
    field: "sportType" | "difficulty" | "intensity" | "phase",
    value: string
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  const handleTagChange = (tagId: number) => {
    setFormData((prev) => {
      const alreadySelected = prev.tagIds.includes(tagId);

      return {
        ...prev,
        tagIds: alreadySelected
          ? prev.tagIds.filter((id) => id !== tagId)
          : [...prev.tagIds, tagId],
      };
    });
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");

    if (!admin) {
      setError("Ehhez a művelethez admin jogosultság szükséges.");
      return;
    }

    if (formData.minPlayers > formData.maxPlayers) {
      setError("A minimum játékosszám nem lehet nagyobb, mint a maximum.");
      return;
    }

    if (focusTags.length > 0) {
      const selectedFocusCount = focusTags.filter((tag) =>
        formData.tagIds.includes(tag.id)
      ).length;

      if (selectedFocusCount === 0) {
        setError("Válassz ki legalább egy fókuszterületet.");
        return;
      }
    }

    try {
      await exerciseService.create(formData);
      setSuccessMessage("A gyakorlat sikeresen létrejött.");
      setTimeout(() => navigate("/exercises"), 1000);
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        if (err.response?.status === 403) {
          setError("Ehhez a művelethez admin jogosultság szükséges.");
        } else {
          setError(err.response?.data?.message || "Sikertelen mentés.");
        }
      } else {
        setError("Sikertelen mentés.");
      }
    }
  };

  if (!admin) {
    return (
      <AppLayout
        title="Nincs hozzáférés"
        subtitle="Ehhez az oldalhoz nincs jogosultságod."
      >
        <section className="card">
          <p className="error-text">
            Új gyakorlatot csak admin felhasználó hozhat létre.
          </p>

          <div className="toolbar" style={{ marginTop: "1rem" }}>
            <Link
              to="/exercises"
              className="secondary-button"
              style={{ textDecoration: "none" }}
            >
              Vissza a gyakorlatokhoz
            </Link>
          </div>
        </section>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title="Új gyakorlat létrehozása"
      subtitle="Adj hozzá új strandröplabda vagy kondi gyakorlatot az adatbázishoz."
    >
      <div className="toolbar" style={{ marginBottom: "1.5rem" }}>
        <Link
          to="/exercises"
          className="secondary-button"
          style={{ textDecoration: "none" }}
        >
          Vissza a gyakorlatokhoz
        </Link>
      </div>

      <section className="card">
        <form onSubmit={handleSubmit}>
          <div className="filters-grid" style={{ marginBottom: "1rem" }}>
            <div className="form-field">
              <label htmlFor="title">Cím</label>
              <input
                id="title"
                name="title"
                type="text"
                value={formData.title}
                onChange={handleInputChange}
                required
              />
            </div>

            <div className="form-field">
              <label htmlFor="durationMin">Időtartam (perc)</label>
              <input
                id="durationMin"
                name="durationMin"
                type="number"
                min={1}
                value={formData.durationMin}
                onChange={handleInputChange}
                required
              />
            </div>

            <div className="form-field" style={{ gridColumn: "1 / -1" }}>
              <label htmlFor="description">Leírás</label>
              <textarea
                id="description"
                name="description"
                rows={4}
                value={formData.description}
                onChange={handleInputChange}
                required
              />
            </div>

            <div className="form-field">
              <label>Sportág</label>
              <div className="choice-group">
                {["BeachVolleyball", "Gym"].map((value) => (
                  <button
                    key={value}
                    type="button"
                    className={`choice-chip ${
                      formData.sportType === value ? "active" : ""
                    }`}
                    onClick={() => handleChoiceChange("sportType", value)}
                  >
                    {value}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-field">
              <label>Nehézség</label>
              <div className="choice-group">
                {["Beginner", "Intermediate", "Advanced"].map((value) => (
                  <button
                    key={value}
                    type="button"
                    className={`choice-chip ${
                      formData.difficulty === value ? "active" : ""
                    }`}
                    onClick={() => handleChoiceChange("difficulty", value)}
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
                      formData.intensity === value ? "active" : ""
                    }`}
                    onClick={() => handleChoiceChange("intensity", value)}
                  >
                    {value}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-field">
              <label>Fázis</label>
              <div className="choice-group">
                {["Warmup", "Main"].map((value) => (
                  <button
                    key={value}
                    type="button"
                    className={`choice-chip ${
                      formData.phase === value ? "active" : ""
                    }`}
                    onClick={() => handleChoiceChange("phase", value)}
                  >
                    {value}
                  </button>
                ))}
              </div>
            </div>

            <div className="form-field">
              <label htmlFor="minPlayers">Minimum játékos</label>
              <input
                id="minPlayers"
                name="minPlayers"
                type="number"
                min={1}
                value={formData.minPlayers}
                onChange={handleInputChange}
                required
              />
            </div>

            <div className="form-field">
              <label htmlFor="maxPlayers">Maximum játékos</label>
              <input
                id="maxPlayers"
                name="maxPlayers"
                type="number"
                min={1}
                value={formData.maxPlayers}
                onChange={handleInputChange}
                required
              />
            </div>

            <div className="form-field" style={{ gridColumn: "1 / -1" }}>
              <label>Fókuszterületek</label>

              {loadingTags && <p className="info-text">Tagek betöltése...</p>}

              {!loadingTags && focusTags.length === 0 && (
                <p className="info-text">
                  Nincs elérhető fókusz tag. Hozd létre a Focus típusú tageket az
                  adatbázisban.
                </p>
              )}

              {!loadingTags && focusTags.length > 0 && (
                <div className="choice-group">
                  {focusTags.map((tag) => (
                    <button
                      key={tag.id}
                      type="button"
                      className={`choice-chip ${
                        formData.tagIds.includes(tag.id) ? "active" : ""
                      }`}
                      onClick={() => handleTagChange(tag.id)}
                    >
                      {tag.name}
                    </button>
                  ))}
                </div>
              )}
            </div>

            <div className="form-field" style={{ gridColumn: "1 / -1" }}>
              <label>Egyéb tagek</label>

              {loadingTags && <p className="info-text">Tagek betöltése...</p>}

              {!loadingTags && otherTags.length === 0 && (
                <p className="info-text">Nincs egyéb elérhető tag.</p>
              )}

              {!loadingTags && otherTags.length > 0 && (
                <div className="choice-group">
                  {otherTags.map((tag) => (
                    <button
                      key={tag.id}
                      type="button"
                      className={`choice-chip ${
                        formData.tagIds.includes(tag.id) ? "active" : ""
                      }`}
                      onClick={() => handleTagChange(tag.id)}
                    >
                      {tag.name} ({tag.type})
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>

          {error && (
            <p className="error-text" style={{ marginBottom: "1rem" }}>
              {error}
            </p>
          )}

          {successMessage && (
            <p className="success-text" style={{ marginBottom: "1rem" }}>
              {successMessage}
            </p>
          )}

          <div className="toolbar">
            <button className="primary-button" type="submit">
              Mentés
            </button>
          </div>
        </form>
      </section>
    </AppLayout>
  );
}

export default CreateExercisePage;