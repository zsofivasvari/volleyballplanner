import { useEffect, useState } from "react";
import axios from "axios";
import {
  Link,
  useNavigate,
} from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import { trainingSessionService } from "../services/trainingSessionService";
import { sportRoleService } from "../services/sportRoleService";

import type { TrainingSessionListItem } from "../types/trainingSession";

function MyTrainingSessionsPage() {
  const navigate = useNavigate();

  const [sessions, setSessions] =
    useState<TrainingSessionListItem[]>([]);

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState("");

  const [
    isOrganizerCoach,
    setIsOrganizerCoach,
  ] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError("");

        const sportRoles =
          await sportRoleService.getMySportRoles();

        setIsOrganizerCoach(
          sportRoles.isOrganizerCoach
        );

        if (
          !sportRoles.isOrganizerCoach
        ) {
          setError(
            "A Saját edzéseim oldal csak Edző / Szervező szerepkörrel érhető el."
          );

          return;
        }

        const sessionData =
          await trainingSessionService.getMine();

        setSessions(sessionData);
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(err)
        ) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni a saját edzéseket."
          );
        } else {
          setError(
            "Nem sikerült betölteni a saját edzéseket."
          );
        }
      } finally {
        setLoading(false);
      }
    };

    void loadData();
  }, []);

  const formatDateTime = (
    value: string
  ) => {
    return new Date(value).toLocaleString(
      "hu-HU",
      {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      }
    );
  };

  const isPastSession = (
    endTime: string
  ) => {
    return new Date(endTime) < new Date();
  };

  if (loading) {
    return (
      <AppLayout
        title="Saját edzéseim"
        subtitle="Az általad létrehozott edzések kezelése."
      >
        <div className="card">
          <p className="info-text">
            Edzések betöltése...
          </p>
        </div>
      </AppLayout>
    );
  }

  if (!isOrganizerCoach) {
    return (
      <AppLayout
        title="Saját edzéseim"
        subtitle="Az általad létrehozott edzések kezelése."
      >
        <div className="card">
          <p className="error-text">
            {error}
          </p>

          <button
            type="button"
            className="primary-button"
            onClick={() =>
              navigate("/profile")
            }
          >
            Profil megnyitása
          </button>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title="Saját edzéseim"
      subtitle="Az általad létrehozott edzések kezelése."
    >
      <div className="training-page-actions">
        <Link
          to="/trainings"
          className="secondary-button"
        >
          Összes edzés
        </Link>

        <Link
          to="/trainings/new"
          className="primary-button"
        >
          Új edzés létrehozása
        </Link>
      </div>

      {error && (
        <div className="card">
          <p className="error-text">
            {error}
          </p>
        </div>
      )}

      {!error &&
        sessions.length === 0 && (
          <div className="card">
            <p className="info-text">
              Még nem hoztál létre
              edzést.
            </p>

            <Link
              to="/trainings/new"
              className="primary-button"
            >
              Első edzés létrehozása
            </Link>
          </div>
        )}

      {!error &&
        sessions.length > 0 && (
          <div className="training-session-grid">
            {sessions.map(
              (session) => {
                const past =
                  isPastSession(
                    session.endTime
                  );

                return (
                  <article
                    key={session.id}
                    className="card training-session-card"
                  >
                    <div className="training-session-card-header">
                      <div>
                        <span className="training-session-sport">
                          {
                            session.sportType
                          }
                        </span>

                        <h2>
                          {session.title}
                        </h2>
                      </div>

                      <span className="training-session-status">
                        {past
                          ? "Lezajlott"
                          : session.isFull
                            ? "Betelt"
                            : session.status}
                      </span>
                    </div>

                    <div className="training-session-meta">
                      <p>
                        <strong>
                          Időpont:
                        </strong>{" "}
                        {formatDateTime(
                          session.startTime
                        )}
                      </p>

                      <p>
                        <strong>
                          Helyszín:
                        </strong>{" "}
                        {session.location}
                      </p>

                      <p>
                        <strong>
                          Szint:
                        </strong>{" "}
                        {session.targetLevel ||
                          "Nincs megadva"}
                      </p>

                      <p>
                        <strong>
                          Jelentkezők:
                        </strong>{" "}
                        {
                          session.participantCount
                        }{" "}
                        /{" "}
                        {
                          session.maxParticipants
                        }{" "}
                        fő
                      </p>

                      <p>
                        <strong>
                          Várólista:
                        </strong>{" "}
                        {
                          session.waitlistCount
                        }{" "}
                        fő
                      </p>
                    </div>

                    <div className="training-session-actions">
                      <Link
                        to={`/trainings/${session.id}`}
                        className="primary-button training-session-link"
                      >
                        Részletek és
                        jelentkezők
                      </Link>
                    </div>
                  </article>
                );
              }
            )}
          </div>
        )}
    </AppLayout>
  );
}

export default MyTrainingSessionsPage;