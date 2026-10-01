import {
  useEffect,
  useState,
} from "react";

import axios from "axios";

import {
  Link,
  useNavigate,
  useParams,
} from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import { trainingSessionService } from "../services/trainingSessionService";
import { authService } from "../services/authService";
import { sportRoleService } from "../services/sportRoleService";

import type {
  UpdateTrainingSessionRequest,
} from "../types/trainingSession";

interface TrainingSessionForm {
  title: string;
  description: string;
  sportType: string;
  startTime: string;
  endTime: string;
  location: string;
  maxParticipants: number;
  targetLevel: string;
}

function toDateTimeLocalValue(
  value: string
) {
  const date = new Date(value);

  const timezoneOffset =
    date.getTimezoneOffset() * 60000;

  return new Date(
    date.getTime() - timezoneOffset
  )
    .toISOString()
    .slice(0, 16);
}

function EditTrainingSessionPage() {
  const { id } = useParams();

  const navigate = useNavigate();

  const sessionId = Number(id);

  const [form, setForm] =
    useState<TrainingSessionForm>({
      title: "",
      description: "",
      sportType: "BeachVolleyball",
      startTime: "",
      endTime: "",
      location: "",
      maxParticipants: 1,
      targetLevel: "",
    });

  const [participantCount, setParticipantCount] =
    useState(0);

  const [loading, setLoading] =
    useState(true);

  const [saving, setSaving] =
    useState(false);

  const [error, setError] =
    useState("");

  useEffect(() => {
    const loadData = async () => {
      if (
        !sessionId ||
        Number.isNaN(sessionId)
      ) {
        setError(
          "Érvénytelen edzésazonosító."
        );

        setLoading(false);

        return;
      }

      try {
        setLoading(true);
        setError("");

        const [
          session,
          currentUser,
          roles,
        ] = await Promise.all([
          trainingSessionService.getById(
            sessionId
          ),

          authService.getMe(),

          sportRoleService.getMySportRoles(),
        ]);

        const canEdit =
          roles.isOrganizerCoach &&
          session.organizerUserId ===
            currentUser.userId;

        if (!canEdit) {
          setError(
            "Ezt az edzést nem szerkesztheted."
          );

          return;
        }

        if (
          new Date(session.startTime) <=
          new Date()
        ) {
          setError(
            "Már megkezdődött vagy lezajlott edzés nem szerkeszthető."
          );

          return;
        }

        setParticipantCount(
          session.participantCount
        );

        setForm({
          title: session.title,
          description:
            session.description ?? "",
          sportType:
            session.sportType,
          startTime:
            toDateTimeLocalValue(
              session.startTime
            ),
          endTime:
            toDateTimeLocalValue(
              session.endTime
            ),
          location:
            session.location,
          maxParticipants:
            session.maxParticipants,
          targetLevel:
            session.targetLevel ?? "",
        });
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni az edzést."
          );
        } else {
          setError(
            "Nem sikerült betölteni az edzést."
          );
        }
      } finally {
        setLoading(false);
      }
    };

    void loadData();
  }, [sessionId]);

  const handleChange = (
    field: keyof TrainingSessionForm,
    value: string
  ) => {
    setError("");

    setForm(previous => ({
      ...previous,

      [field]:
        field === "maxParticipants"
          ? Number(value)
          : value,
    }));
  };

  const handleSubmit = async (
    event: React.FormEvent
  ) => {
    event.preventDefault();

    if (!form.title.trim()) {
      setError(
        "Az edzés címe kötelező."
      );

      return;
    }

    if (!form.location.trim()) {
      setError(
        "A helyszín megadása kötelező."
      );

      return;
    }

    if (
      form.maxParticipants <
      participantCount
    ) {
      setError(
        `A maximális létszám nem lehet kisebb a már jelentkezett játékosok számánál (${participantCount}).`
      );

      return;
    }

    const start =
      new Date(form.startTime);

    const end =
      new Date(form.endTime);

    if (
      start <= new Date()
    ) {
      setError(
        "Az edzés kezdési időpontjának a jövőben kell lennie."
      );

      return;
    }

    if (end <= start) {
      setError(
        "A befejezési időpontnak későbbinek kell lennie a kezdésnél."
      );

      return;
    }

    try {
      setSaving(true);
      setError("");

      const request:
        UpdateTrainingSessionRequest = {
          title:
            form.title.trim(),

          description:
            form.description.trim(),

          sportType:
            form.sportType,

          startTime:
            start.toISOString(),

          endTime:
            end.toISOString(),

          location:
            form.location.trim(),

          maxParticipants:
            form.maxParticipants,

          targetLevel:
            form.targetLevel.trim(),
        };

      await trainingSessionService.update(
        sessionId,
        request
      );

      navigate(
        `/trainings/${sessionId}`
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setError(
          err.response?.data?.message ??
            "Nem sikerült módosítani az edzést."
        );
      } else {
        setError(
          "Nem sikerült módosítani az edzést."
        );
      }
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <AppLayout
        title="Edzés szerkesztése"
      >
        <div className="card">
          <p className="info-text">
            Edzés betöltése...
          </p>
        </div>
      </AppLayout>
    );
  }

  if (error && !form.title) {
    return (
      <AppLayout
        title="Edzés szerkesztése"
      >
        <div className="card">
          <p className="error-text">
            {error}
          </p>

          <Link
            to={`/trainings/${sessionId}`}
            className="secondary-button"
          >
            Vissza
          </Link>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title="Edzés szerkesztése"
      subtitle="Módosítsd a meghirdetett edzés adatait."
    >
      <form
        className="card"
        onSubmit={handleSubmit}
      >
        <div className="form-field">
          <label htmlFor="title">
            Edzés neve
          </label>

          <input
            id="title"
            type="text"
            value={form.title}
            onChange={event =>
              handleChange(
                "title",
                event.target.value
              )
            }
          />
        </div>

        <div className="form-field">
          <label htmlFor="description">
            Leírás
          </label>

          <textarea
            id="description"
            value={form.description}
            onChange={event =>
              handleChange(
                "description",
                event.target.value
              )
            }
          />
        </div>

        <div className="form-field">
          <label htmlFor="sportType">
            Sportág
          </label>

          <select
            id="sportType"
            value={form.sportType}
            onChange={event =>
              handleChange(
                "sportType",
                event.target.value
              )
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

        <div className="form-field">
          <label htmlFor="startTime">
            Kezdés
          </label>

          <input
            id="startTime"
            type="datetime-local"
            value={form.startTime}
            onChange={event =>
              handleChange(
                "startTime",
                event.target.value
              )
            }
          />
        </div>

        <div className="form-field">
          <label htmlFor="endTime">
            Befejezés
          </label>

          <input
            id="endTime"
            type="datetime-local"
            value={form.endTime}
            onChange={event =>
              handleChange(
                "endTime",
                event.target.value
              )
            }
          />
        </div>

        <div className="form-field">
          <label htmlFor="location">
            Helyszín
          </label>

          <input
            id="location"
            type="text"
            value={form.location}
            onChange={event =>
              handleChange(
                "location",
                event.target.value
              )
            }
          />
        </div>

        <div className="form-field">
          <label htmlFor="maxParticipants">
            Maximális létszám
          </label>

          <input
            id="maxParticipants"
            type="number"
            min={Math.max(
              1,
              participantCount
            )}
            value={
              form.maxParticipants
            }
            onChange={event =>
              handleChange(
                "maxParticipants",
                event.target.value
              )
            }
          />

          {participantCount > 0 && (
            <p className="info-text">
              Jelenleg{" "}
              <strong>
                {participantCount}
              </strong>{" "}
              biztos résztvevő van.
            </p>
          )}
        </div>

        <div className="form-field">
          <label htmlFor="targetLevel">
            Szint
          </label>

          <select
            id="targetLevel"
            value={form.targetLevel}
            onChange={event =>
              handleChange(
                "targetLevel",
                event.target.value
              )
            }
          >
            <option value="">
              Válassz szintet
            </option>

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

        {error && (
          <p className="error-text">
            {error}
          </p>
        )}

        <div className="training-edit-actions">
            <button
                type="submit"
                className="primary-button"
                disabled={saving}
            >
                {saving
                ? "Mentés..."
                : "Módosítások mentése"}
            </button>

            <Link
                to={`/trainings/${sessionId}`}
                className="secondary-button"
            >
                Mégse
            </Link>
            </div>
      </form>
    </AppLayout>
  );
}

export default EditTrainingSessionPage;