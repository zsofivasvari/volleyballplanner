import { useEffect, useState } from "react";
import { AxiosError } from "axios";
import { Link, useNavigate } from "react-router-dom";
import { exerciseService } from "../services/exerciseService";
import { tagService } from "../services/tagService";
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

  useEffect(() => {
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
  }, []);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
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

    try {
      await exerciseService.create(formData);
      setSuccessMessage("A gyakorlat sikeresen létrejött.");
      setTimeout(() => navigate("/exercises"), 1000);
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setError(err.response?.data?.message || "Sikertelen mentés.");
      } else {
        setError("Sikertelen mentés.");
      }
    }
  };

  return (
    <div style={{ padding: "2rem", maxWidth: "800px", margin: "0 auto" }}>
      <Link to="/exercises">← Vissza a gyakorlatokhoz</Link>

      <h1 style={{ marginTop: "1rem" }}>Új gyakorlat létrehozása</h1>

      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="title">Cím</label>
          <input
            id="title"
            name="title"
            type="text"
            value={formData.title}
            onChange={handleChange}
            required
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          />
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="description">Leírás</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            required
            rows={4}
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          />
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="sportType">Sportág</label>
          <select
            id="sportType"
            name="sportType"
            value={formData.sportType}
            onChange={handleChange}
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          >
            <option value="BeachVolleyball">BeachVolleyball</option>
            <option value="Gym">Gym</option>
          </select>
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="durationMin">Időtartam (perc)</label>
          <input
            id="durationMin"
            name="durationMin"
            type="number"
            value={formData.durationMin}
            onChange={handleChange}
            min={1}
            required
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          />
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="difficulty">Nehézség</label>
          <select
            id="difficulty"
            name="difficulty"
            value={formData.difficulty}
            onChange={handleChange}
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          >
            <option value="Beginner">Beginner</option>
            <option value="Intermediate">Intermediate</option>
            <option value="Advanced">Advanced</option>
          </select>
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="intensity">Intenzitás</label>
          <select
            id="intensity"
            name="intensity"
            value={formData.intensity}
            onChange={handleChange}
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          >
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <label htmlFor="phase">Fázis</label>
          <select
            id="phase"
            name="phase"
            value={formData.phase}
            onChange={handleChange}
            style={{ display: "block", width: "100%", padding: "0.5rem" }}
          >
            <option value="Warmup">Warmup</option>
            <option value="Main">Main</option>
            <option value="Cooldown">Cooldown</option>
          </select>
        </div>

        <div style={{ marginBottom: "1rem", display: "flex", gap: "1rem" }}>
          <div style={{ flex: 1 }}>
            <label htmlFor="minPlayers">Minimum játékos</label>
            <input
              id="minPlayers"
              name="minPlayers"
              type="number"
              value={formData.minPlayers}
              onChange={handleChange}
              min={1}
              required
              style={{ display: "block", width: "100%", padding: "0.5rem" }}
            />
          </div>

          <div style={{ flex: 1 }}>
            <label htmlFor="maxPlayers">Maximum játékos</label>
            <input
              id="maxPlayers"
              name="maxPlayers"
              type="number"
              value={formData.maxPlayers}
              onChange={handleChange}
              min={1}
              required
              style={{ display: "block", width: "100%", padding: "0.5rem" }}
            />
          </div>
        </div>

        <div style={{ marginBottom: "1rem" }}>
          <h3>Tagek</h3>

          {loadingTags && <p>Tagek betöltése...</p>}

          {!loadingTags && tags.length === 0 && <p>Nincs elérhető tag.</p>}

          {!loadingTags && tags.length > 0 && (
            <div style={{ display: "grid", gap: "0.5rem" }}>
              {tags.map((tag) => (
                <label key={tag.id} style={{ display: "flex", gap: "0.5rem" }}>
                  <input
                    type="checkbox"
                    checked={formData.tagIds.includes(tag.id)}
                    onChange={() => handleTagChange(tag.id)}
                  />
                  {tag.name} ({tag.type})
                </label>
              ))}
            </div>
          )}
        </div>

        {error && <p style={{ color: "red" }}>{error}</p>}
        {successMessage && <p style={{ color: "green" }}>{successMessage}</p>}

        <button type="submit">Mentés</button>
      </form>
    </div>
  );
}

export default CreateExercisePage;