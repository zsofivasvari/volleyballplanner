import {
  useEffect,
  useMemo,
  useState,
} from "react";

import axios from "axios";

import {
  Link,
} from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import {
  tournamentService,
} from "../services/tournamentService";

import type {
  TournamentListItem,
} from "../types/tournament";

type TournamentFilter =
  | "all"
  | "registration"
  | "running"
  | "completed";

function TournamentsPage() {
  const [
    tournaments,
    setTournaments,
  ] = useState<TournamentListItem[]>([]);

  const [
    filter,
    setFilter,
  ] = useState<TournamentFilter>("all");

  const [
    loading,
    setLoading,
  ] = useState(true);

  const [
    error,
    setError,
  ] = useState("");

  useEffect(() => {
    const loadTournaments = async () => {
      try {
        setLoading(true);
        setError("");

        const data =
          await tournamentService.getAll();

        setTournaments(data);
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni a versenyeket."
          );
        } else {
          setError(
            "Nem sikerült betölteni a versenyeket."
          );
        }
      } finally {
        setLoading(false);
      }
    };

    void loadTournaments();
  }, []);

  const visibleTournaments =
    useMemo(() => {
      if (filter === "all") {
        return tournaments;
      }

      return tournaments.filter(
        tournament => {
          if (filter === "registration") {
            return (
              tournament.status ===
              "RegistrationOpen"
            );
          }

          if (filter === "running") {
            return (
              tournament.status ===
                "PoolStage" ||
              tournament.status ===
                "KnockoutStage"
            );
          }

          if (filter === "completed") {
            return (
              tournament.status ===
              "Completed"
            );
          }

          return true;
        }
      );
    }, [
      tournaments,
      filter,
    ]);

  const formatDate = (
    value: string
  ) => {
    return new Date(
      value
    ).toLocaleDateString(
      "hu-HU",
      {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
      }
    );
  };

  const getStatusLabel = (
    status: string
  ) => {
    switch (status) {
      case "RegistrationOpen":
        return "Nevezés nyitva";

      case "RegistrationClosed":
        return "Nevezés lezárva";

      case "PoolStage":
        return "Csoportkör";

      case "KnockoutStage":
        return "Kieséses szakasz";

      case "Completed":
        return "Befejezett";

      case "Cancelled":
        return "Törölve";

      default:
        return status;
    }
  };

  return (
    <AppLayout
      title="Versenyek"
      subtitle="Böngéssz a strandröplabda-versenyek között."
    >
      <div className="card training-filter-bar">
        <div className="training-filter-group">
          <label htmlFor="tournamentFilter">
            Állapot
          </label>

          <select
            id="tournamentFilter"
            value={filter}
            onChange={event =>
              setFilter(
                event.target
                  .value as TournamentFilter
              )
            }
          >
            <option value="all">
              Összes verseny
            </option>

            <option value="registration">
              Nevezés nyitva
            </option>

            <option value="running">
              Folyamatban
            </option>

            <option value="completed">
              Befejezett
            </option>
          </select>
        </div>
      </div>

      {loading && (
        <div className="card">
          <p className="info-text">
            Versenyek betöltése...
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
        tournaments.length === 0 && (
          <div className="card">
            <p className="info-text">
              Jelenleg nincs meghirdetett
              verseny.
            </p>
          </div>
        )}

      {!loading &&
        !error &&
        tournaments.length > 0 &&
        visibleTournaments.length === 0 && (
          <div className="card">
            <p className="info-text">
              A kiválasztott szűrésnek
              nincs megfelelő verseny.
            </p>
          </div>
        )}

      {!loading &&
        !error &&
        visibleTournaments.length > 0 && (
          <div className="training-session-grid">
            {visibleTournaments.map(
              tournament => (
                <article
                  key={tournament.id}
                  className="card training-session-card"
                >
                  <div className="training-session-card-header">
                    <div>
                      <span className="training-session-sport">
                        Strandröplabda
                      </span>

                      <h2>
                        {tournament.name}
                      </h2>
                    </div>

                    <span className="training-session-status">
                      {getStatusLabel(
                        tournament.status
                      )}
                    </span>
                  </div>

                  <div className="training-session-meta">
                    <p>
                      <strong>
                        Időpont:
                      </strong>{" "}
                      {formatDate(
                        tournament.startDate
                      )}
                    </p>

                    <p>
                      <strong>
                        Helyszín:
                      </strong>{" "}
                      {tournament.location}
                    </p>

                    <p>
                      <strong>
                        Csapatok:
                      </strong>{" "}
                      {tournament.registeredTeams}
                      {" / "}
                      {tournament.maxTeams}
                    </p>

                    <p>
                      <strong>
                        Nevezési határidő:
                      </strong>{" "}
                      {formatDate(
                        tournament.registrationDeadline
                      )}
                    </p>

                    <p>
                      <strong>
                        Szervező:
                      </strong>{" "}
                      {tournament.organizerName}
                    </p>
                  </div>

                  <div className="training-session-actions">
                    <Link
                      to={`/tournaments/${tournament.id}`}
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

export default TournamentsPage;