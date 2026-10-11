import {
  useState,
} from "react";

import type {
  FormEvent,
} from "react";

import axios from "axios";

import {
  Link,
  useNavigate,
} from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import {
  tournamentService,
} from "../services/tournamentService";

function CreateTournamentPage() {
  const navigate = useNavigate();

  const [
    name,
    setName,
  ] = useState("");

  const [
    description,
    setDescription,
  ] = useState("");

  const [
    location,
    setLocation,
  ] = useState("");

  const [
    startDate,
    setStartDate,
  ] = useState("");

  const [
    endDate,
    setEndDate,
  ] = useState("");

  const [
    registrationDeadline,
    setRegistrationDeadline,
  ] = useState("");

  const [
    maxTeams,
    setMaxTeams,
  ] = useState(16);

  const [
    submitting,
    setSubmitting,
  ] = useState(false);

  const [
    error,
    setError,
  ] = useState("");

  const handleSubmit = async (
    event: FormEvent
  ) => {
    event.preventDefault();

    try {
      setSubmitting(true);
      setError("");

      if (!name.trim()) {
        setError(
          "A verseny neve kötelező."
        );

        return;
      }

      if (!location.trim()) {
        setError(
          "A helyszín megadása kötelező."
        );

        return;
      }

      if (!startDate) {
        setError(
          "A verseny kezdési időpontja kötelező."
        );

        return;
      }

      if (!registrationDeadline) {
        setError(
          "A nevezési határidő kötelező."
        );

        return;
      }

      const start =
        new Date(startDate);

      const registration =
        new Date(
          registrationDeadline
        );

      const end =
        endDate
          ? new Date(endDate)
          : null;

      if (
        start.getTime() <=
        Date.now()
      ) {
        setError(
          "A verseny kezdési időpontjának a jövőben kell lennie."
        );

        return;
      }

      if (
        registration.getTime() <=
        Date.now()
      ) {
        setError(
          "A nevezési határidőnek a jövőben kell lennie."
        );

        return;
      }

      if (
        registration.getTime() >=
        start.getTime()
      ) {
        setError(
          "A nevezési határidőnek a verseny kezdete előtt kell lennie."
        );

        return;
      }

      if (
        end &&
        end.getTime() <
          start.getTime()
      ) {
        setError(
          "A verseny befejezése nem lehet korábban a kezdésnél."
        );

        return;
      }

      const created =
        await tournamentService.create({
          name: name.trim(),
          description:
            description.trim(),
          location:
            location.trim(),

          startDate:
            start.toISOString(),

          endDate:
            end
              ? end.toISOString()
              : null,

          registrationDeadline:
            registration.toISOString(),

          maxTeams,
        });

      navigate(
        `/tournaments/${created.id}`
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setError(
          err.response?.data?.message ??
            "Nem sikerült létrehozni a versenyt."
        );
      } else {
        setError(
          "Nem sikerült létrehozni a versenyt."
        );
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <AppLayout
      title="Új verseny"
      subtitle="Hozz létre új strandröplabda-versenyt."
    >
      <div className="tournament-details-actions">
        <Link
          to="/tournaments"
          className="secondary-button tournament-back-link"
        >
          ← Vissza
        </Link>
      </div>

      <form
        className="card tournament-create-form"
        onSubmit={handleSubmit}
      >
        <div className="tournament-section-heading">
          <span>
            VERSENY LÉTREHOZÁSA
          </span>

          <h2>
            Versenyadatok
          </h2>
        </div>

        <div className="tournament-create-grid">
          <div className="profile-field">
            <label htmlFor="tournamentName">
              Verseny neve *
            </label>

            <input
              id="tournamentName"
              type="text"
              value={name}
              onChange={event =>
                setName(
                  event.target.value
                )
              }
              placeholder="pl. Budapest Beach Cup"
              disabled={submitting}
            />
          </div>

          <div className="profile-field">
            <label htmlFor="tournamentLocation">
              Helyszín *
            </label>

            <input
              id="tournamentLocation"
              type="text"
              value={location}
              onChange={event =>
                setLocation(
                  event.target.value
                )
              }
              placeholder="pl. Budapest"
              disabled={submitting}
            />
          </div>

          <div className="profile-field">
            <label htmlFor="tournamentStart">
              Kezdés *
            </label>

            <input
              id="tournamentStart"
              type="datetime-local"
              value={startDate}
              onChange={event =>
                setStartDate(
                  event.target.value
                )
              }
              disabled={submitting}
            />
          </div>

          <div className="profile-field">
            <label htmlFor="tournamentEnd">
              Befejezés
            </label>

            <input
              id="tournamentEnd"
              type="datetime-local"
              value={endDate}
              onChange={event =>
                setEndDate(
                  event.target.value
                )
              }
              disabled={submitting}
            />
          </div>

          <div className="profile-field">
            <label htmlFor="registrationDeadline">
              Nevezési határidő *
            </label>

            <input
              id="registrationDeadline"
              type="datetime-local"
              value={
                registrationDeadline
              }
              onChange={event =>
                setRegistrationDeadline(
                  event.target.value
                )
              }
              disabled={submitting}
            />
          </div>

          <div className="profile-field">
            <label htmlFor="maxTeams">
              Maximális csapatszám *
            </label>

            <select
              id="maxTeams"
              value={maxTeams}
              onChange={event =>
                setMaxTeams(
                  Number(
                    event.target.value
                  )
                )
              }
              disabled={submitting}
            >
              <option value={12}>
                12 csapat
              </option>

              <option value={13}>
                13 csapat
              </option>

              <option value={14}>
                14 csapat
              </option>

              <option value={15}>
                15 csapat
              </option>

              <option value={16}>
                16 csapat
              </option>
            </select>
          </div>

          <div className="profile-field tournament-create-description">
            <label htmlFor="tournamentDescription">
              Leírás
            </label>

            <textarea
              id="tournamentDescription"
              value={description}
              onChange={event =>
                setDescription(
                  event.target.value
                )
              }
              placeholder="A verseny rövid leírása..."
              rows={5}
              disabled={submitting}
            />
          </div>
        </div>

        <div className="tournament-create-format">
          <span>
            Versenyformátum
          </span>

          <strong>
            Modified Pool Play
          </strong>

          <p>
            4 csoport, majd Legjobb 12-es
            kieséses szakasz.
          </p>
        </div>

        {error && (
          <p className="error-text">
            {error}
          </p>
        )}

        <div className="tournament-create-actions">
          <Link
            to="/tournaments"
            className="secondary-button"
          >
            Mégse
          </Link>

          <button
            type="submit"
            className="primary-button"
            disabled={submitting}
          >
            {submitting
              ? "Létrehozás..."
              : "Verseny létrehozása"}
          </button>
        </div>
      </form>
    </AppLayout>
  );
}

export default CreateTournamentPage;