import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import AppLayout from "../components/layout/AppLayout";
import { exerciseService } from "../services/exerciseService";
import { isAdmin } from "../utils/auth";
import type { ExerciseListItem, ExerciseQuery } from "../types/exercise";

type SportTab = "BeachVolleyball" | "Gym";

interface FilterOption {
  value: string;
  label: string;
  tone?: "blue" | "yellow" | "orange";
}

const beachFocusOptions: FilterOption[] = [
  { value: "Nyitás", label: "Nyitás", tone: "yellow" },
  { value: "Nyitásfogadás", label: "Nyitásfogadás", tone: "blue" },
  { value: "Feladás", label: "Feladás", tone: "blue" },
  { value: "Támadás", label: "Támadás", tone: "orange" },
  { value: "Blokk/Védekezés", label: "Blokk / Védekezés", tone: "blue" },
  { value: "Állóképesség", label: "Állóképesség", tone: "yellow" },
];

const gymFocusOptions: FilterOption[] = [
  { value: "Alsótest", label: "Alsótest", tone: "yellow" },
  { value: "Törzs", label: "Törzs", tone: "blue" },
  { value: "Felsőtest", label: "Felsőtest", tone: "orange" },
  { value: "Fullbody", label: "Fullbody", tone: "blue" },
  { value: "Állóképesség", label: "Állóképesség", tone: "yellow" },
];

const difficultyOptions: FilterOption[] = [
  { value: "Beginner", label: "Beginner", tone: "blue" },
  { value: "Intermediate", label: "Intermediate", tone: "yellow" },
  { value: "Advanced", label: "Advanced", tone: "orange" },
];

const intensityOptions: FilterOption[] = [
  { value: "Low", label: "Low", tone: "blue" },
  { value: "Medium", label: "Medium", tone: "yellow" },
  { value: "High", label: "High", tone: "orange" },
];

const phaseOptions: FilterOption[] = [
  { value: "Warmup", label: "Warmup", tone: "blue" },
  { value: "Main", label: "Main", tone: "yellow" },
];

const gymFocusImageMap: Record<string, string> = {
  Alsótest: "/images/alsotest.png",
  Törzs: "/images/torzs.png",
  Felsőtest: "/images/felsotest.png",
  Fullbody: "/images/teljestest.png",
  Állóképesség: "/images/allokepesseg.png",
};

const gymFocusFallbackImageMap: Record<string, string> = {
  Alsótest: "/images/alsotest.png",
  Törzs: "/images/torzs.png",
  Felsőtest: "/images/felssotesst.png",
  Fullbody: "/images/teljestest.png",
  Állóképesség: "/images/allokepesseg.png",
};

const defaultFilters: ExerciseQuery = {
  sportTypes: ["BeachVolleyball"],
  difficulties: [],
  intensities: [],
  phases: [],
  focusTags: [],
};

function getFiltersFromSearchParams(
  searchParams: URLSearchParams
): ExerciseQuery {
  const sportTypes = searchParams.getAll("sportTypes");

  return {
    sportTypes: sportTypes.length > 0 ? sportTypes : ["BeachVolleyball"],
    difficulties: searchParams.getAll("difficulties"),
    intensities: searchParams.getAll("intensities"),
    phases: searchParams
      .getAll("phases")
      .filter((phase) => phase !== "Cooldown"),
    focusTags: searchParams.getAll("focusTags"),
  };
}

function getActiveSportFromFilters(filters: ExerciseQuery): SportTab {
  return filters.sportTypes[0] === "Gym" ? "Gym" : "BeachVolleyball";
}

function getPageFromSearchParams(searchParams: URLSearchParams) {
  const page = Number(searchParams.get("page"));
  return Number.isNaN(page) || page < 1 ? 1 : page;
}

function buildSearchParams(filters: ExerciseQuery, page: number) {
  const params = new URLSearchParams();

  filters.sportTypes.forEach((value) => params.append("sportTypes", value));
  filters.difficulties.forEach((value) => params.append("difficulties", value));
  filters.intensities.forEach((value) => params.append("intensities", value));

  filters.phases
    .filter((value) => value !== "Cooldown")
    .forEach((value) => params.append("phases", value));

  filters.focusTags.forEach((value) => params.append("focusTags", value));

  if (page > 1) {
    params.set("page", String(page));
  }

  return params;
}

interface FocusIllustrationProps {
  tag: string;
  alt?: string;
  className?: string;
}

function FocusIllustration({ tag, alt, className }: FocusIllustrationProps) {
  const primarySrc = gymFocusImageMap[tag];
  const fallbackSrc = gymFocusFallbackImageMap[tag];

  if (!primarySrc && !fallbackSrc) {
    return null;
  }

  return (
    <img
      src={primarySrc ?? fallbackSrc}
      alt={alt ?? tag}
      className={className}
      onError={(event) => {
        const img = event.currentTarget;

        if (img.dataset.fallbackApplied === "true") {
          return;
        }

        if (fallbackSrc && fallbackSrc !== primarySrc) {
          img.dataset.fallbackApplied = "true";
          img.src = fallbackSrc;
        }
      }}
    />
  );
}

function ExercisesPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [exercises, setExercises] = useState<ExerciseListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [activeSport, setActiveSport] = useState<SportTab>("BeachVolleyball");
  const [filters, setFilters] = useState<ExerciseQuery>(defaultFilters);

  const admin = isAdmin();

  const currentFocusOptions = useMemo(() => {
    return activeSport === "BeachVolleyball"
      ? beachFocusOptions
      : gymFocusOptions;
  }, [activeSport]);

  const currentListUrl = useMemo(() => {
    const query = searchParams.toString();
    return query ? `/exercises?${query}` : "/exercises";
  }, [searchParams]);

  const detailReturnQuery = useMemo(() => {
    return encodeURIComponent(currentListUrl);
  }, [currentListUrl]);

  const loadExercises = async (
    currentFilters: ExerciseQuery,
    currentPage: number = 1
  ) => {
    setLoading(true);
    setError("");

    try {
      const data = await exerciseService.getAll({
        sportTypes: currentFilters.sportTypes,
        difficulties: currentFilters.difficulties,
        intensities: currentFilters.intensities,
        phases: currentFilters.phases.filter((phase) => phase !== "Cooldown"),
        focusTags: currentFilters.focusTags,
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
    const nextFilters = getFiltersFromSearchParams(searchParams);
    const nextPage = getPageFromSearchParams(searchParams);
    const nextActiveSport = getActiveSportFromFilters(nextFilters);

    setFilters(nextFilters);
    setActiveSport(nextActiveSport);

    void loadExercises(nextFilters, nextPage);
  }, [searchParams]);

  const updateUrlAndLoad = (nextFilters: ExerciseQuery, nextPage: number) => {
    const params = buildSearchParams(nextFilters, nextPage);
    setSearchParams(params);
  };

  const toggleFilterValue = (
    field: "difficulties" | "intensities" | "phases" | "focusTags",
    value: string
  ) => {
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

  const handleSportChange = async (sport: SportTab) => {
    setActiveSport(sport);

    const nextFilters: ExerciseQuery = {
      ...filters,
      sportTypes: [sport],
      focusTags: [],
      phases: filters.phases.filter((phase) => phase !== "Cooldown"),
    };

    setFilters(nextFilters);
    updateUrlAndLoad(nextFilters, 1);
  };

  const handleApplyFilters = async () => {
    updateUrlAndLoad(
      {
        ...filters,
        phases: filters.phases.filter((phase) => phase !== "Cooldown"),
      },
      1
    );
  };

  const handleResetFilters = async () => {
    const resetFilters: ExerciseQuery = {
      sportTypes: [activeSport],
      difficulties: [],
      intensities: [],
      phases: [],
      focusTags: [],
    };

    setFilters(resetFilters);
    updateUrlAndLoad(resetFilters, 1);
  };

  const handleRemoveActiveFilter = async (
    field: "difficulties" | "intensities" | "phases" | "focusTags",
    value: string
  ) => {
    const updatedFilters: ExerciseQuery = {
      ...filters,
      [field]: filters[field].filter((item) => item !== value),
    };

    setFilters(updatedFilters);
    updateUrlAndLoad(updatedFilters, 1);
  };

  const handlePreviousPage = async () => {
    if (page > 1) {
      updateUrlAndLoad(filters, page - 1);
    }
  };

  const handleNextPage = async () => {
    if (page < totalPages) {
      updateUrlAndLoad(filters, page + 1);
    }
  };

  const activeFilterEntries = useMemo(() => {
    return [
      ...filters.difficulties.map((value) => ({
        field: "difficulties" as const,
        value,
      })),
      ...filters.intensities.map((value) => ({
        field: "intensities" as const,
        value,
      })),
      ...filters.phases
        .filter((value) => value !== "Cooldown")
        .map((value) => ({
          field: "phases" as const,
          value,
        })),
      ...filters.focusTags.map((value) => ({
        field: "focusTags" as const,
        value,
      })),
    ];
  }, [filters]);

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

      <section className="card training-filter-studio">
        <div className="training-filter-heading">
          <div>
            <h2>Találd meg gyorsan a megfelelő gyakorlatot</h2>
          </div>
        </div>

        <div className="sport-mode-switch">
          <button
            type="button"
            className={`sport-mode-card ${
              activeSport === "BeachVolleyball" ? "active" : ""
            }`}
            onClick={() => void handleSportChange("BeachVolleyball")}
          >
            <span className="sport-mode-accent sport-mode-accent-beach" />
            <strong>Beach Volleyball</strong>
          </button>

          <button
            type="button"
            className={`sport-mode-card ${
              activeSport === "Gym" ? "active" : ""
            }`}
            onClick={() => void handleSportChange("Gym")}
          >
            <span className="sport-mode-accent sport-mode-accent-gym" />
            <strong>Gym</strong>
          </button>
        </div>

        <div className="filter-studio-grid">
          <div className="filter-studio-panel filter-studio-panel-wide">
            <div className="filter-section-title">
              <span>Fókuszterület</span>
            </div>

            <div className="focus-option-grid">
              {currentFocusOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  className={`filter-option-card filter-option-card-${option.tone} ${
                    filters.focusTags.includes(option.value) ? "active" : ""
                  } ${activeSport === "Gym" ? "gym-focus-filter-card" : ""}`}
                  onClick={() => toggleFilterValue("focusTags", option.value)}
                >
                  {activeSport === "Gym" && (
                    <FocusIllustration
                      tag={option.value}
                      alt={option.label}
                      className="gym-focus-filter-image"
                    />
                  )}

                  <strong>{option.label}</strong>
                </button>
              ))}
            </div>
          </div>

          <div className="filter-studio-panel">
            <div className="filter-section-title">
              <span>Nehézség</span>
            </div>

            <div className="compact-option-stack">
              {difficultyOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  className={`compact-filter-option ${
                    filters.difficulties.includes(option.value) ? "active" : ""
                  }`}
                  onClick={() =>
                    toggleFilterValue("difficulties", option.value)
                  }
                >
                  <strong>{option.label}</strong>
                </button>
              ))}
            </div>
          </div>

          <div className="filter-studio-panel">
            <div className="filter-section-title">
              <span>Intenzitás</span>
            </div>

            <div className="compact-option-stack">
              {intensityOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  className={`compact-filter-option intensity-option ${
                    filters.intensities.includes(option.value) ? "active" : ""
                  }`}
                  onClick={() =>
                    toggleFilterValue("intensities", option.value)
                  }
                >
                  <div>
                    <strong>{option.label}</strong>
                  </div>

                  <i
                    className={`intensity-meter intensity-meter-${option.value.toLowerCase()}`}
                  />
                </button>
              ))}
            </div>
          </div>

          <div className="filter-studio-panel">
            <div className="filter-section-title">
              <span>Fázis</span>
            </div>

            <div className="compact-option-stack">
              {phaseOptions.map((option) => (
                <button
                  key={option.value}
                  type="button"
                  className={`compact-filter-option ${
                    filters.phases.includes(option.value) ? "active" : ""
                  }`}
                  onClick={() => toggleFilterValue("phases", option.value)}
                >
                  <strong>{option.label}</strong>
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="active-filter-summary">
          <div>
            <strong>Aktív szűrőprofil</strong>
            <span>
              {activeSport === "BeachVolleyball"
                ? "Beach Volleyball"
                : "Gym"}
            </span>
          </div>

          {activeFilterEntries.length > 0 ? (
            <div className="active-filter-chip-list">
              {activeFilterEntries.map((entry) => (
                <button
                  key={`${entry.field}-${entry.value}`}
                  type="button"
                  className="active-filter-chip"
                  onClick={() =>
                    void handleRemoveActiveFilter(entry.field, entry.value)
                  }
                >
                  <span>{entry.value}</span>
                  <b>×</b>
                </button>
              ))}
            </div>
          ) : (
            <p className="active-filter-empty">
              Még nincs további szűkítés kiválasztva.
            </p>
          )}
        </div>

        <div className="toolbar training-filter-actions">
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

                {exercise.sportType !== "Gym" && (
                  <p className="exercise-meta">
                    Játékosok: {exercise.minPlayers} - {exercise.maxPlayers}
                  </p>
                )}

                <p className="exercise-meta">Fázis: {exercise.phase}</p>

                <div style={{ marginTop: "1rem" }}>
                  <Link
                    to={`/exercises/${exercise.id}?returnTo=${detailReturnQuery}`}
                    state={{ from: currentListUrl }}
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