import { useEffect, useMemo, useState } from "react";
import axios from "axios";
import { Link } from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import { trainingSessionService } from "../services/trainingSessionService";
import { sportRoleService } from "../services/sportRoleService";

import type { TrainingSessionListItem } from "../types/trainingSession";

type TrainingFilter =
  | "all"
  | "available"
  | "full"
  | "ongoing"
  | "past";

type TrainingSort =
  | "startAsc"
  | "startDesc";

function TrainingSessionsPage() {
  const [sessions, setSessions] =
    useState<TrainingSessionListItem[]>([]);

  const [isOrganizerCoach, setIsOrganizerCoach] =
    useState(false);

  const [filter, setFilter] =
    useState<TrainingFilter>("all");

  const [sort, setSort] =
    useState<TrainingSort>("startAsc");

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState("");

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError("");

        const [
          sessionData,
          sportRoles,
        ] = await Promise.all([
          trainingSessionService.getAll(),
          sportRoleService.getMySportRoles(),
        ]);

        setSessions(sessionData);

        setIsOrganizerCoach(
          sportRoles.isOrganizerCoach
        );
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni az edzéseket."
          );
        } else {
          setError(
            "Nem sikerült betölteni az edzéseket."
          );
        }
      } finally {
        setLoading(false);
      }
    };

    void loadData();
  }, []);

  const getSessionState = (
    session: TrainingSessionListItem
  ) => {
    const now = new Date();

    const start =
      new Date(session.startTime);

    const end =
      new Date(session.endTime);

    if (end <= now) {
      return "past";
    }

    if (
      start <= now &&
      end > now
    ) {
      return "ongoing";
    }

    if (session.isFull) {
      return "full";
    }

    return "available";
  };

  const visibleSessions =
    useMemo(() => {
      const filtered =
        sessions.filter(session => {
          if (filter === "all") {
            return true;
          }

          return (
            getSessionState(session) ===
            filter
          );
        });

      return [...filtered].sort(
        (a, b) => {
          const aTime =
            new Date(
              a.startTime
            ).getTime();

          const bTime =
            new Date(
              b.startTime
            ).getTime();

          return sort === "startAsc"
            ? aTime - bTime
            : bTime - aTime;
        }
      );
    }, [sessions, filter, sort]);

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

  const getStatusLabel = (
    session: TrainingSessionListItem
  ) => {
    const state =
      getSessionState(session);

    if (state === "past") {
      return "Lezajlott";
    }

    if (state === "ongoing") {
      return "Zajlik";
    }

    if (state === "full") {
      return "Betelt";
    }

    return "Jelentkezhető";
  };

  return (
    <AppLayout
      title="Edzések"
      subtitle="Böngéssz a meghirdetett röplabda- és strandröplabda-edzések között."
    >
      {isOrganizerCoach && (
        <div className="training-page-actions">
          <Link
            to="/trainings/mine"
            className="secondary-button"
          >
            Saját edzéseim
          </Link>

          <Link
            to="/trainings/new"
            className="primary-button"
          >
            Új edzés létrehozása
          </Link>
        </div>
      )}

      <div className="card training-filter-bar">
        <div className="training-filter-group">
          <label htmlFor="trainingFilter">
            Állapot
          </label>

          <select
            id="trainingFilter"
            value={filter}
            onChange={e =>
              setFilter(
                e.target
                  .value as TrainingFilter
              )
            }
          >
            <option value="all">
              Összes edzés
            </option>

            <option value="available">
              Jelentkezhető
            </option>

            <option value="full">
              Betelt
            </option>

            <option value="ongoing">
              Zajló
            </option>

            <option value="past">
              Lezajlott
            </option>
          </select>
        </div>

        <div className="training-filter-group">
          <label htmlFor="trainingSort">
            Rendezés
          </label>

          <select
            id="trainingSort"
            value={sort}
            onChange={e =>
              setSort(
                e.target
                  .value as TrainingSort
              )
            }
          >
            <option value="startAsc">
              Időpont – legkorábbi elöl
            </option>

            <option value="startDesc">
              Időpont – legkésőbbi elöl
            </option>
          </select>
        </div>
      </div>

      {loading && (
        <div className="card">
          <p className="info-text">
            Edzések betöltése...
          </p>
        </div>
      )}

      {error && (
        <div className="card">
          <p className="error-text">
            {error}
          </p>
        </div>
      )}

      {!loading &&
        !error &&
        sessions.length === 0 && (
          <div className="card">
            <p className="info-text">
              Jelenleg nincs meghirdetett
              edzés.
            </p>
          </div>
        )}

      {!loading &&
        !error &&
        sessions.length > 0 &&
        visibleSessions.length === 0 && (
          <div className="card">
            <p className="info-text">
              A kiválasztott szűrésnek
              nincs megfelelő edzés.
            </p>
          </div>
        )}

      {!loading &&
        !error &&
        visibleSessions.length > 0 && (
          <div className="training-session-grid">
            {visibleSessions.map(
              session => (
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
                      {getStatusLabel(
                        session
                      )}
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
                      {
                        session.location
                      }
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

                    {session.waitlistCount >
                      0 && (
                      <p>
                        <strong>
                          Várólista:
                        </strong>{" "}
                        {
                          session.waitlistCount
                        }{" "}
                        fő
                      </p>
                    )}

                    <p>
                      <strong>
                        Szervező:
                      </strong>{" "}
                      {
                        session.organizerName
                      }
                    </p>
                  </div>

                  <div className="training-session-actions">
                    <Link
                      to={`/trainings/${session.id}`}
                      className="primary-button training-session-link"
                    >
                      Részletek
                    </Link>
                  </div>
                </article>
              )
            )}
          </div>
        )}
    </AppLayout>
  );
}

export default TrainingSessionsPage;