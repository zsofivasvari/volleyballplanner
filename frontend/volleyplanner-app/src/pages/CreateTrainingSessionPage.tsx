import {
  useEffect,
  useMemo,
  useState,
} from "react";

import axios from "axios";

import {
  useNavigate,
} from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import {
  trainingSessionService,
} from "../services/trainingSessionService";

import {
  trainingPlanService,
} from "../services/trainingPlanService";

import type {
  CreateTrainingSessionRequest,
} from "../types/trainingSession";

import type {
  TrainingPlanListItem,
} from "../types/trainingPlan";

function CreateTrainingSessionPage() {
  const navigate = useNavigate();

  const [
    formData,
    setFormData,
  ] =
    useState<CreateTrainingSessionRequest>({
      trainingPlanId: null,
      title: "",
      description: "",
      sportType: "BeachVolleyball",
      startTime: "",
      endTime: "",
      location: "",
      maxParticipants: 8,
      targetLevel: "Intermediate",
    });

  const [
    trainingPlans,
    setTrainingPlans,
  ] =
    useState<TrainingPlanListItem[]>([]);

  const [
    loadingPlans,
    setLoadingPlans,
  ] =
    useState(true);

  const [
    loading,
    setLoading,
  ] =
    useState(false);

  const [
    error,
    setError,
  ] =
    useState("");

  useEffect(() => {
    const loadTrainingPlans =
      async () => {
        try {
          setLoadingPlans(true);

          const plans =
            await trainingPlanService.getAll();

          setTrainingPlans(plans);
        } catch (err) {
          console.error(err);

          setTrainingPlans([]);
        } finally {
          setLoadingPlans(false);
        }
      };

    void loadTrainingPlans();
  }, []);

  const sessionDuration =
    useMemo(() => {
      if (
        !formData.startTime ||
        !formData.endTime
      ) {
        return 0;
      }

      const start =
        new Date(formData.startTime);

      const end =
        new Date(formData.endTime);

      const duration =
        Math.round(
          (
            end.getTime() -
            start.getTime()
          ) /
            60000
        );

      return duration > 0
        ? duration
        : 0;
    }, [
      formData.startTime,
      formData.endTime,
    ]);

  const compatiblePlans =
    useMemo(() => {
      return trainingPlans.filter(
        plan =>
          plan.sportType ===
            formData.sportType &&
          (
            sessionDuration === 0 ||
            plan.targetDuration ===
              sessionDuration
          )
      );
    }, [
      trainingPlans,
      formData.sportType,
      sessionDuration,
    ]);

  const selectedPlan =
    useMemo(() => {
      if (
        !formData.trainingPlanId
      ) {
        return null;
      }

      return (
        trainingPlans.find(
          plan =>
            plan.id ===
            formData.trainingPlanId
        ) ?? null
      );
    }, [
      trainingPlans,
      formData.trainingPlanId,
    ]);

  const handleChange = (
    e:
      | React.ChangeEvent<HTMLInputElement>
      | React.ChangeEvent<HTMLTextAreaElement>
      | React.ChangeEvent<HTMLSelectElement>
  ) => {
    const {
      name,
      value,
    } = e.target;

    setError("");

    setFormData(prev => {
      const updated = {
        ...prev,

        [name]:
          name ===
          "maxParticipants"
            ? Number(value)
            : value,
      };

      // Sportágváltáskor ne maradjon
      // inkompatibilis edzésterv kiválasztva.
      if (
        name === "sportType"
      ) {
        updated.trainingPlanId =
          null;
      }

      return updated;
    });
  };

  const handlePlanChange = (
    value: string
  ) => {
    const planId =
      value === ""
        ? null
        : Number(value);

    setError("");

    setFormData(prev => ({
      ...prev,
      trainingPlanId: planId,
    }));
  };

  const validateForm = () => {
    if (
      !formData.title.trim()
    ) {
      return "Add meg az edzés nevét.";
    }

    if (
      !formData.location.trim()
    ) {
      return "Add meg az edzés helyszínét.";
    }

    if (
      !formData.startTime ||
      !formData.endTime
    ) {
      return "Add meg az edzés kezdési és befejezési időpontját.";
    }

    const start =
      new Date(formData.startTime);

    const end =
      new Date(formData.endTime);

    if (
      end <= start
    ) {
      return "A befejezési időpontnak későbbinek kell lennie a kezdésnél.";
    }

    if (
      start <= new Date()
    ) {
      return "Az edzés kezdési időpontjának a jövőben kell lennie.";
    }

    if (
      formData.maxParticipants < 1
    ) {
      return "A maximális létszám legalább 1 fő legyen.";
    }

    if (
      selectedPlan &&
      selectedPlan.targetDuration !==
        sessionDuration
    ) {
      return `A kiválasztott edzésterv ${selectedPlan.targetDuration} perces, az esemény pedig ${sessionDuration} perces.`;
    }

    return null;
  };

  const handleSubmit = async (
    e: React.FormEvent<HTMLFormElement>
  ) => {
    e.preventDefault();

    const validationError =
      validateForm();

    if (validationError) {
      setError(
        validationError
      );

      return;
    }

    try {
      setLoading(true);
      setError("");

      const created =
        await trainingSessionService.create(
          formData
        );

      navigate(
        `/trainings/${created.id}`
      );
    } catch (err) {
      console.error(err);

      if (
        axios.isAxiosError(err)
      ) {
        setError(
          err.response?.data?.message ??
            "Nem sikerült létrehozni az edzést."
        );
      } else {
        setError(
          "Nem sikerült létrehozni az edzést."
        );
      }
    } finally {
      setLoading(false);
    }
  };

  const formatPreviewDate = (
    value: string
  ) => {
    if (!value) {
      return "Nincs megadva";
    }

    return new Date(
      value
    ).toLocaleString(
      "hu-HU",
      {
        year: "numeric",
        month: "long",
        day: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      }
    );
  };

  const sportLabel =
    formData.sportType ===
    "BeachVolleyball"
      ? "Strandröplabda"
      : "Kondiedzés";

  const levelLabel =
    formData.targetLevel ===
    "Beginner"
      ? "Kezdő"
      : formData.targetLevel ===
          "Advanced"
        ? "Haladó"
        : "Középhaladó";

  return (
    <AppLayout
      title="Új edzés"
      subtitle="Hozz létre egy új edzést, állítsd be a részleteket és opcionálisan kapcsolj hozzá edzéstervet."
    >
      <form
        className="training-create-studio"
        onSubmit={handleSubmit}
      >
        <div className="training-create-main">
          <section className="card training-create-section">
            <div className="training-create-section-heading">
              <span className="training-create-step">
                1
              </span>

              <div>
                <h2>
                  Alapadatok
                </h2>

                <p>
                  Add meg, milyen
                  edzést szeretnél
                  meghirdetni.
                </p>
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="title">
                Edzés neve
              </label>

              <input
                id="title"
                name="title"
                type="text"
                value={
                  formData.title
                }
                onChange={
                  handleChange
                }
                placeholder="Pl. Sideout technikai edzés"
                required
              />

              <small className="training-field-hint">
                Rövid, könnyen
                felismerhető nevet adj
                az edzésnek.
              </small>
            </div>

            <div className="training-create-grid">
              <div className="form-group">
                <label htmlFor="sportType">
                  Sportág
                </label>

                <select
                  id="sportType"
                  name="sportType"
                  value={
                    formData.sportType
                  }
                  onChange={
                    handleChange
                  }
                >
                  <option value="BeachVolleyball">
                    Strandröplabda
                  </option>

                  <option value="IndoorVolleyball">
                    Kondiedzés
                  </option>
                </select>
              </div>

              <div className="form-group">
                <label htmlFor="targetLevel">
                  Tudásszint
                </label>

                <select
                  id="targetLevel"
                  name="targetLevel"
                  value={
                    formData.targetLevel
                  }
                  onChange={
                    handleChange
                  }
                >
                  <option value="Beginner">
                    Kezdő
                  </option>

                  <option value="Intermediate">
                    Középhaladó
                  </option>

                  <option value="Advanced">
                    Haladó
                  </option>
                </select>
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="description">
                Leírás
              </label>

              <textarea
                id="description"
                name="description"
                value={
                  formData.description
                }
                onChange={
                  handleChange
                }
                placeholder="Pl. Nyitásfogadás, sideout és támadásfejlesztés középhaladó játékosoknak."
                rows={5}
              />

              <small className="training-field-hint">
                Írd le röviden az
                edzés célját és
                tartalmát.
              </small>
            </div>
          </section>

          <section className="card training-create-section">
            <div className="training-create-section-heading">
              <span className="training-create-step">
                2
              </span>

              <div>
                <h2>
                  Időpont és helyszín
                </h2>

                <p>
                  Add meg, mikor és hol
                  lesz az edzés.
                </p>
              </div>
            </div>

            <div className="training-create-grid">
              <div className="form-group">
                <label htmlFor="startTime">
                  Kezdés
                </label>

                <input
                  id="startTime"
                  name="startTime"
                  type="datetime-local"
                  value={
                    formData.startTime
                  }
                  onChange={
                    handleChange
                  }
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="endTime">
                  Befejezés
                </label>

                <input
                  id="endTime"
                  name="endTime"
                  type="datetime-local"
                  value={
                    formData.endTime
                  }
                  onChange={
                    handleChange
                  }
                  required
                />
              </div>
            </div>

            {sessionDuration > 0 && (
              <div className="training-duration-info">
                <span>
                  Tervezett időtartam
                </span>

                <strong>
                  {sessionDuration} perc
                </strong>
              </div>
            )}

            <div className="form-group">
              <label htmlFor="location">
                Helyszín
              </label>

              <input
                id="location"
                name="location"
                type="text"
                value={
                  formData.location
                }
                onChange={
                  handleChange
                }
                placeholder="Pl. BME Sporttelep, Budapest"
                required
              />
            </div>
          </section>

          <section className="card training-create-section">
            <div className="training-create-section-heading">
              <span className="training-create-step">
                3
              </span>

              <div>
                <h2>
                  Résztvevők
                </h2>

                <p>
                  Állítsd be az edzés
                  maximális létszámát.
                </p>
              </div>
            </div>

            <div className="training-participant-limit">
              <button
                type="button"
                className="training-number-button"
                onClick={() =>
                  setFormData(prev => ({
                    ...prev,
                    maxParticipants:
                      Math.max(
                        1,
                        prev.maxParticipants -
                          1
                      ),
                  }))
                }
              >
                −
              </button>

              <div className="training-participant-value">
                <strong>
                  {
                    formData.maxParticipants
                  }
                </strong>

                <span>
                  fő
                </span>
              </div>

              <button
                type="button"
                className="training-number-button"
                onClick={() =>
                  setFormData(prev => ({
                    ...prev,
                    maxParticipants:
                      prev.maxParticipants +
                      1,
                  }))
                }
              >
                +
              </button>
            </div>

            <p className="training-field-hint">
              Ha az edzés betelik, a
              további jelentkezők
              automatikusan várólistára
              kerülnek.
            </p>
          </section>

          <section className="card training-create-section">
            <div className="training-create-section-heading">
              <span className="training-create-step">
                4
              </span>

              <div>
                <h2>
                  Edzésterv
                </h2>

                <p>
                  Opcionálisan kapcsolj
                  egy saját mentett vagy
                  generált tervet az
                  eseményhez.
                </p>
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="trainingPlanId">
                Kapcsolódó edzésterv
              </label>

              <select
                id="trainingPlanId"
                value={
                  formData.trainingPlanId ??
                  ""
                }
                onChange={e =>
                  handlePlanChange(
                    e.target.value
                  )
                }
                disabled={
                  loadingPlans
                }
              >
                <option value="">
                  Nincs edzésterv
                  hozzárendelve
                </option>

                {compatiblePlans.map(
                  plan => (
                    <option
                      key={
                        plan.id
                      }
                      value={
                        plan.id
                      }
                    >
                      {plan.title} –{" "}
                      {
                        plan.targetDuration
                      }{" "}
                      perc
                    </option>
                  )
                )}
              </select>

              {loadingPlans ? (
                <small className="training-field-hint">
                  Edzéstervek
                  betöltése...
                </small>
              ) : sessionDuration ===
                0 ? (
                <small className="training-field-hint">
                  Az időpont megadása
                  után csak megfelelő
                  hosszúságú
                  edzésterveket
                  jelenítünk meg.
                </small>
              ) : compatiblePlans.length ===
                0 ? (
                <small className="training-field-warning">
                  Nincs saját,
                  {` ${sessionDuration} `}
                  perces,
                  {` ${sportLabel.toLowerCase()} `}
                  edzésterved.
                </small>
              ) : (
                <small className="training-field-hint">
                  {
                    compatiblePlans.length
                  }{" "}
                  kompatibilis
                  edzésterv érhető el.
                </small>
              )}
            </div>

            {selectedPlan && (
              <div className="training-selected-plan">
                <div>
                  <span>
                    Kiválasztott terv
                  </span>

                  <strong>
                    {
                      selectedPlan.title
                    }
                  </strong>
                </div>

                <div>
                  <span>
                    Időtartam
                  </span>

                  <strong>
                    {
                      selectedPlan.targetDuration
                    }{" "}
                    perc
                  </strong>
                </div>

                <div>
                  <span>
                    Fókusz
                  </span>

                  <strong>
                    {selectedPlan.primaryFocus ||
                      "Nincs megadva"}
                  </strong>
                </div>
              </div>
            )}
          </section>
        </div>

        <aside className="card training-create-preview">
          <div className="training-create-preview-header">
            <p>
              EDZÉS ELŐNÉZETE
            </p>

            <h2>
              {formData.title ||
                "Új edzés"}
            </h2>

            <span>
              {sportLabel}
            </span>
          </div>

          <div className="training-preview-list">
            <div>
              <span>
                Kezdés
              </span>

              <strong>
                {formatPreviewDate(
                  formData.startTime
                )}
              </strong>
            </div>

            <div>
              <span>
                Befejezés
              </span>

              <strong>
                {formatPreviewDate(
                  formData.endTime
                )}
              </strong>
            </div>

            <div>
              <span>
                Időtartam
              </span>

              <strong>
                {sessionDuration > 0
                  ? `${sessionDuration} perc`
                  : "Nincs megadva"}
              </strong>
            </div>

            <div>
              <span>
                Helyszín
              </span>

              <strong>
                {formData.location ||
                  "Nincs megadva"}
              </strong>
            </div>

            <div>
              <span>
                Szint
              </span>

              <strong>
                {levelLabel}
              </strong>
            </div>

            <div>
              <span>
                Maximális létszám
              </span>

              <strong>
                {
                  formData.maxParticipants
                }{" "}
                fő
              </strong>
            </div>

            <div>
              <span>
                Edzésterv
              </span>

              <strong>
                {selectedPlan?.title ??
                  "Nincs hozzárendelve"}
              </strong>
            </div>
          </div>

          {error && (
            <div className="training-create-error">
              {error}
            </div>
          )}

          <div className="training-create-actions">
            <button
              type="button"
              className="secondary-button"
              onClick={() =>
                navigate(
                  "/trainings"
                )
              }
              disabled={loading}
            >
              Mégse
            </button>

            <button
              type="submit"
              className="primary-button"
              disabled={loading}
            >
              {loading
                ? "Létrehozás..."
                : "Edzés meghirdetése"}
            </button>
          </div>
        </aside>
      </form>
    </AppLayout>
  );
}

export default CreateTrainingSessionPage;