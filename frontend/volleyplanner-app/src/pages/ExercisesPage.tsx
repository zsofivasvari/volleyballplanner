import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import { exerciseService } from "../services/exerciseService";
import { isAdmin } from "../utils/auth";
import type { ExerciseListItem, ExerciseQuery } from "../types/exercise";

function ExercisesPage() {
  const [exercises, setExercises] = useState<ExerciseListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [page, setPage] = useState(1);
  const [pageSize] = useState(8);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [filters, setFilters] = useState<ExerciseQuery>({
    sportTypes: [],
    difficulties: [],
    intensities: [],
    phases: [],
  });

  const admin = isAdmin();

  const loadExercises = async (
    currentFilters?: ExerciseQuery,
    currentPage: number = 1
  ) => {
    setLoading(true);
    setError("");

    try {
      const data = await exerciseService.getAll({
        sportTypes: currentFilters?.sportTypes ?? [],
        difficulties: currentFilters?.difficulties ?? [],
        intensities: currentFilters?.intensities ?? [],
        phases: currentFilters?.phases ?? [],
        page: currentPage,
        pageSize,
      });

      setExercises(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setPage(data.page);
    } catch {
      setError("Nem sikerült betölteni a gyakorlatokat.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadExercises(filters, 1);
  }, []);

  const toggleFilterValue = (field: keyof ExerciseQuery, value: string) => {
    setFilters((prev) => {
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

  const handleApplyFilters = async () => {
    await loadExercises(filters, 1);
  };

  const handleResetFilters = async () => {
    const resetFilters: ExerciseQuery = {
      sportTypes: [],
      difficulties: [],
      intensities: [],
      phases: [],
    };

    setFilters(resetFilters);
    await loadExercises(resetFilters, 1);
  };

  const handlePreviousPage = async () => {
    if (page > 1) {
      await loadExercises(filters, page - 1);
    }
  };

  const handleNextPage = async () => {
    if (page < totalPages) {
      await loadExercises(filters, page + 1);
    }
  };

  return (
    <AppLayout
      title="Gyakorlatok"
      subtitle="Böngészd, szűrd és kezeld a strandröplabda és kondi gyakorlatokat."
    >
      <div className="toolbar" style={{ marginBottom: "1.5rem" }}>
        <Link
          to="/generate"
          className="primary-button"
          style={{ textDecoration: "none" }}
        >
          Edzésterv generálás
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

        {admin && (
          <Link
            to="/exercises/new"
            className="secondary-button"
            style={{ textDecoration: "none" }}
          >
            Új gyakorlat
          </Link>
        )}
      </div>

      <section
        className="card filter-card"
        style={{ marginBottom: "1.5rem" }}
      >
        <h2 style={{ marginTop: 0 }}>Szűrők</h2>

        <div className="filter-sections">
          <div className="form-field">
            <label>Sportág</label>
            <div className="choice-group">
              {["BeachVolleyball", "Gym"].map((value) => (
                <button
                  key={value}
                  type="button"
                  className={`choice-chip ${
                    filters.sportTypes.includes(value) ? "active" : ""
                  }`}
                  onClick={() => toggleFilterValue("sportTypes", value)}
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
                    filters.difficulties.includes(value) ? "active" : ""
                  }`}
                  onClick={() => toggleFilterValue("difficulties", value)}
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
                    filters.intensities.includes(value) ? "active" : ""
                  }`}
                  onClick={() => toggleFilterValue("intensities", value)}
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
                    filters.phases.includes(value) ? "active" : ""
                  }`}
                  onClick={() => toggleFilterValue("phases", value)}
                >
                  {value}
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="toolbar" style={{ marginTop: "1rem" }}>
          <button className="primary-button" onClick={handleApplyFilters}>
            Szűrés alkalmazása
          </button>
          <button className="secondary-button" onClick={handleResetFilters}>
            Szűrők törlése
          </button>
        </div>
      </section>

      {loading && <p className="info-text">Betöltés...</p>}
      {error && <p className="error-text">{error}</p>}

      {!loading && !error && exercises.length === 0 && (
        <p className="info-text">Nincs a szűrésnek megfelelő gyakorlat.</p>
      )}

      {!loading && !error && exercises.length > 0 && (
        <>
          <p className="info-text" style={{ marginBottom: "1rem" }}>
            Összes találat: {totalCount}
          </p>

          <section className="exercise-grid-large">
            {exercises.map((exercise) => (
              <article className="card exercise-card-large" key={exercise.id}>
                <h3 className="exercise-card-title">{exercise.title}</h3>
                <p className="exercise-meta">Sportág: {exercise.sportType}</p>
                <p className="exercise-meta">
                  Időtartam: {exercise.durationMin} perc
                </p>
                <p className="exercise-meta">
                  Nehézség: {exercise.difficulty}
                </p>
                <p className="exercise-meta">
                  Intenzitás: {exercise.intensity}
                </p>
                <p className="exercise-meta">
                  Játékosok: {exercise.minPlayers} - {exercise.maxPlayers}
                </p>
                <p className="exercise-meta">Fázis: {exercise.phase}</p>

                <div style={{ marginTop: "1rem" }}>
                  <Link
                    to={`/exercises/${exercise.id}`}
                    className="primary-button"
                    style={{ textDecoration: "none" }}
                  >
                    Részletek
                  </Link>
                </div>
              </article>
            ))}
          </section>

          <div
            className="toolbar"
            style={{
              marginTop: "1.5rem",
              justifyContent: "center",
              alignItems: "center",
            }}
          >
            <button
              className="secondary-button"
              onClick={handlePreviousPage}
              disabled={page <= 1}
            >
              Előző
            </button>

            <span className="info-text">
              Oldal: {page} / {totalPages}
            </span>

            <button
              className="secondary-button"
              onClick={handleNextPage}
              disabled={page >= totalPages}
            >
              Következő
            </button>
          </div>
        </>
      )}
    </AppLayout>
  );
}

export default ExercisesPage;