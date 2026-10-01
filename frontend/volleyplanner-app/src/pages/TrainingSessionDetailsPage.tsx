import {
  useEffect,
  useMemo,
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
import { trainingPlanService } from "../services/trainingPlanService";
import { plannerService } from "../services/plannerService";
import { authService } from "../services/authService";
import { sportRoleService } from "../services/sportRoleService";

import type {
  TrainingSessionDetails,
} from "../types/trainingSession";

import type {
  TrainingParticipant,
} from "../types/trainingBooking";

import type {
  TrainingPlanListItem,
} from "../types/trainingPlan";

function TrainingSessionDetailsPage() {
  const { id } = useParams();

  const sessionId = Number(id);

  const navigate = useNavigate();

  const [
    session,
    setSession,
  ] =
    useState<TrainingSessionDetails | null>(
      null
    );

  const [
    participants,
    setParticipants,
  ] =
    useState<TrainingParticipant[]>([]);

  const [
    trainingPlans,
    setTrainingPlans,
  ] =
    useState<TrainingPlanListItem[]>([]);

  const [
    selectedTrainingPlanId,
    setSelectedTrainingPlanId,
  ] =
    useState<number>(0);

  const [
    currentUserId,
    setCurrentUserId,
  ] =
    useState<number | null>(null);

  const [
    isPlayer,
    setIsPlayer,
  ] =
    useState(false);

  const [
    isOrganizerCoach,
    setIsOrganizerCoach,
  ] =
    useState(false);

  const [
    loading,
    setLoading,
  ] =
    useState(true);

  const [
    bookingLoading,
    setBookingLoading,
  ] =
    useState(false);

  const [
    participantsLoading,
    setParticipantsLoading,
  ] =
    useState(false);

  const [
    trainingPlanLoading,
    setTrainingPlanLoading,
  ] =
    useState(false);

  const [
    calendarLoading,
    setCalendarLoading,
  ] =
    useState(false);

  const [
    deleteLoading,
    setDeleteLoading,
  ] =
  useState(false);

  const [
    error,
    setError,
  ] =
    useState("");

  const [
    successMessage,
    setSuccessMessage,
  ] =
    useState("");

  const isSessionOrganizer =
    session !== null &&
    currentUserId !== null &&
    isOrganizerCoach &&
    session.organizerUserId ===
      currentUserId;

  const isOtherOrganizer =
    isOrganizerCoach &&
    !isSessionOrganizer;

  const confirmedParticipants =
    participants.filter(
      participant =>
        participant.status ===
        "Confirmed"
    );

  const waitlistedParticipants =
    participants.filter(
      participant =>
        participant.status ===
        "Waitlisted"
    );

  const now = new Date();

  const isUpcoming =
    session !== null &&
    new Date(
      session.startTime
    ) > now;

  const isOngoing =
    session !== null &&
    new Date(
      session.startTime
    ) <= now &&
    new Date(
      session.endTime
    ) > now;

  const isPast =
    session !== null &&
    new Date(
      session.endTime
    ) <= now;

  const displayStatus =
    session == null
      ? ""
      : isPast
        ? "Lezajlott"
        : isOngoing
          ? "Zajlik"
          : session.isFull
            ? "Betelt"
            : "Jelentkezhető";

  const sessionDurationMinutes =
    useMemo(() => {
      if (!session) {
        return 0;
      }

      return Math.round(
        (
          new Date(
            session.endTime
          ).getTime() -
          new Date(
            session.startTime
          ).getTime()
        ) /
          60000
      );
    }, [session]);

  const compatibleTrainingPlans =
    useMemo(() => {
      if (!session) {
        return [];
      }

      return trainingPlans.filter(
        plan =>
          plan.sportType ===
            session.sportType &&
          plan.targetDuration ===
            sessionDurationMinutes
      );
    }, [
      trainingPlans,
      session,
      sessionDurationMinutes,
    ]);

  const canAddToCalendar =
    session !== null &&
    isUpcoming &&
    (
      isSessionOrganizer ||
      (
        isPlayer &&
        session.currentUserBookingStatus ===
          "Confirmed"
      )
    );

  const refreshSession =
    async () => {
      const updatedSession =
        await trainingSessionService.getById(
          sessionId
        );

      setSession(
        updatedSession
      );
    };

  const loadParticipants =
    async () => {
      if (
        !sessionId ||
        Number.isNaN(sessionId) ||
        !isSessionOrganizer
      ) {
        return;
      }

      try {
        setParticipantsLoading(
          true
        );

        setError("");

        const data =
          await trainingSessionService.getParticipants(
            sessionId
          );

        setParticipants(data);
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(err)
        ) {
          setError(
            err.response?.data
              ?.message ??
              "Nem sikerült betölteni a jelentkezőket."
          );
        } else {
          setError(
            "Nem sikerült betölteni a jelentkezőket."
          );
        }
      } finally {
        setParticipantsLoading(
          false
        );
      }
    };

  useEffect(() => {
    const loadData =
      async () => {
        if (
          !sessionId ||
          Number.isNaN(
            sessionId
          )
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
            sessionData,
            currentUser,
            sportRoles,
          ] =
            await Promise.all([
              trainingSessionService.getById(
                sessionId
              ),

              authService.getMe(),

              sportRoleService.getMySportRoles(),
            ]);

          setSession(
            sessionData
          );

          setCurrentUserId(
            currentUser.userId
          );

          setIsPlayer(
            sportRoles.isPlayer
          );

          setIsOrganizerCoach(
            sportRoles.isOrganizerCoach
          );

          const userIsOrganizer =
            sportRoles.isOrganizerCoach &&
            sessionData
              .organizerUserId ===
              currentUser.userId;

          if (
            userIsOrganizer
          ) {
            const [
              participantData,
              planData,
            ] =
              await Promise.all([
                trainingSessionService.getParticipants(
                  sessionId
                ),

                trainingPlanService.getAll(),
              ]);

            setParticipants(
              participantData
            );

            setTrainingPlans(
              planData
            );

            if (
              sessionData.trainingPlanId
            ) {
              setSelectedTrainingPlanId(
                sessionData.trainingPlanId
              );
            }
          }
        } catch (err) {
          console.error(err);

          if (
            axios.isAxiosError(
              err
            )
          ) {
            setError(
              err.response?.data
                ?.message ??
                "Nem sikerült betölteni az edzés adatait."
            );
          } else {
            setError(
              "Nem sikerült betölteni az edzés adatait."
            );
          }
        } finally {
          setLoading(false);
        }
      };

    void loadData();
  }, [sessionId]);

  const handleAssignTrainingPlan =
    async () => {
      if (
        selectedTrainingPlanId ===
        0
      ) {
        setError(
          "Válassz ki egy edzéstervet."
        );

        return;
      }

      try {
        setTrainingPlanLoading(
          true
        );

        setError("");
        setSuccessMessage("");

        const updated =
          await trainingSessionService.assignTrainingPlan(
            sessionId,
            selectedTrainingPlanId
          );

        setSession(updated);

        setSuccessMessage(
          "Az edzésterv sikeresen hozzárendelve az edzéshez."
        );
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(
            err
          )
        ) {
          setError(
            err.response?.data
              ?.message ??
              "Nem sikerült hozzárendelni az edzéstervet."
          );
        } else {
          setError(
            "Nem sikerült hozzárendelni az edzéstervet."
          );
        }
      } finally {
        setTrainingPlanLoading(
          false
        );
      }
    };

  const handleRemoveTrainingPlan =
    async () => {
      const confirmed =
        window.confirm(
          "Biztosan leválasztod az edzéstervet erről az edzésről?"
        );

      if (!confirmed) {
        return;
      }

      try {
        setTrainingPlanLoading(
          true
        );

        setError("");
        setSuccessMessage("");

        await trainingSessionService.removeTrainingPlan(
          sessionId
        );

        setSelectedTrainingPlanId(
          0
        );

        await refreshSession();

        setSuccessMessage(
          "Az edzésterv sikeresen leválasztva."
        );
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(
            err
          )
        ) {
          setError(
            err.response?.data
              ?.message ??
              "Nem sikerült leválasztani az edzéstervet."
          );
        } else {
          setError(
            "Nem sikerült leválasztani az edzéstervet."
          );
        }
      } finally {
        setTrainingPlanLoading(
          false
        );
      }
    };

  const handleDeleteTrainingSession =
    async () => {
      if (!session) {
        return;
      }

      const confirmed =
        window.confirm(
          `Biztosan törölni szeretnéd a(z) "${session.title}" edzést?\n\nA jelentkezések és a kapcsolódó naptáresemények is törlődnek.`
        );

      if (!confirmed) {
        return;
      }

      try {
        setDeleteLoading(true);
        setError("");
        setSuccessMessage("");

        await trainingSessionService.delete(
          sessionId
        );

        navigate(
          "/trainings/mine"
        );
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült törölni az edzést."
          );
        } else {
          setError(
            "Nem sikerült törölni az edzést."
          );
        }
      } finally {
        setDeleteLoading(false);
      }
    };

  const handleAddToCalendar =
    async () => {
      try {
        setCalendarLoading(
          true
        );

        setError("");
        setSuccessMessage("");

        await plannerService
          .createFromTrainingSession(
            sessionId
          );

        await refreshSession();

        setSuccessMessage(
          "Az edzés sikeresen bekerült a terveződbe."
        );
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(
            err
          )
        ) {
          setError(
            err.response?.data
              ?.message ??
              "Nem sikerült hozzáadni az edzést a tervezőhöz."
          );
        } else {
          setError(
            "Nem sikerült hozzáadni az edzést a tervezőhöz."
          );
        }
      } finally {
        setCalendarLoading(
          false
        );
      }
    };

  const handleBooking =
    async () => {
      if (
        !session ||
        !isPlayer
      ) {
        return;
      }

      if (
        new Date(
          session.startTime
        ) <= new Date()
      ) {
        setError(
          "A már megkezdődött vagy lezajlott edzésre nem lehet jelentkezni."
        );

        return;
      }

      try {
        setBookingLoading(
          true
        );

        setError("");
        setSuccessMessage("");

        const booking =
          await trainingSessionService.book(
            sessionId
          );

        await refreshSession();

        if (
          booking.status ===
          "Waitlisted"
        ) {
          setSuccessMessage(
            `Az edzés betelt, ezért felkerültél a várólistára. Jelenlegi helyezésed: ${booking.waitlistPosition ?? "-"}.`
          );
        } else {
          setSuccessMessage(
            "Sikeresen jelentkeztél az edzésre."
          );
        }
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(
            err
          )
        ) {
          setError(
            err.response?.data
              ?.message ??
              "Nem sikerült jelentkezni az edzésre."
          );
        } else {
          setError(
            "Nem sikerült jelentkezni az edzésre."
          );
        }
      } finally {
        setBookingLoading(
          false
        );
      }
    };

  const handleCancelBooking =
    async () => {
      if (!session) {
        return;
      }

      if (
        new Date(
          session.startTime
        ) <= new Date()
      ) {
        setError(
          "A már megkezdődött vagy lezajlott edzésről nem lehet lejelentkezni."
        );

        return;
      }

      const previousStatus =
        session
          .currentUserBookingStatus;

      try {
        setBookingLoading(
          true
        );

        setError("");
        setSuccessMessage("");

        await trainingSessionService
          .cancelBooking(
            sessionId
          );

        await refreshSession();

        if (
          previousStatus ===
          "Waitlisted"
        ) {
          setSuccessMessage(
            "A várólistás jelentkezésedet sikeresen lemondtad."
          );
        } else {
          setSuccessMessage(
            "A jelentkezésedet sikeresen lemondtad."
          );
        }
      } catch (err) {
        console.error(err);

        if (
          axios.isAxiosError(
            err
          )
        ) {
          setError(
            err.response?.data
              ?.message ??
              "Nem sikerült lemondani a jelentkezést."
          );
        } else {
          setError(
            "Nem sikerült lemondani a jelentkezést."
          );
        }
      } finally {
        setBookingLoading(
          false
        );
      }
    };

  const handleRefreshStatus =
    async () => {
      try {
        setBookingLoading(
          true
        );

        setError("");
        setSuccessMessage("");

        await refreshSession();
      } catch {
        setError(
          "Nem sikerült frissíteni a jelentkezési állapotot."
        );
      } finally {
        setBookingLoading(
          false
        );
      }
    };

  const formatDateTime = (
    value: string
  ) =>
    new Date(
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

  if (loading) {
    return (
      <AppLayout title="Edzés">
        <div className="card">
          <p className="info-text">
            Edzés betöltése...
          </p>
        </div>
      </AppLayout>
    );
  }

  if (!session) {
    return (
      <AppLayout title="Edzés">
        <div className="card">
          <p className="error-text">
            {error ||
              "Az edzés nem található."}
          </p>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title={session.title}
      subtitle={`${session.sportType} • ${session.location}`}
    >
      <div className="training-details-layout">
        <section className="card training-details-main">
          <div className="training-details-header">
            <span className="training-session-sport">
              {
                session.sportType
              }
            </span>

            <span className="training-session-status">
              {displayStatus}
            </span>
          </div>

          <h2>
            Edzés részletei
          </h2>

          <p className="training-description">
            {session.description ||
              "Az edzéshez nincs külön leírás."}
          </p>

          <div className="training-detail-grid">
            <div>
              <span>Kezdés</span>
              <strong>
                {formatDateTime(
                  session.startTime
                )}
              </strong>
            </div>

            <div>
              <span>Befejezés</span>
              <strong>
                {formatDateTime(
                  session.endTime
                )}
              </strong>
            </div>

            <div>
              <span>Helyszín</span>
              <strong>
                {
                  session.location
                }
              </strong>
            </div>

            <div>
              <span>Szint</span>
              <strong>
                {session.targetLevel ||
                  "Nincs megadva"}
              </strong>
            </div>

            <div>
              <span>
                Jelentkezők
              </span>
              <strong>
                {
                  session.participantCount
                }{" "}
                /{" "}
                {
                  session.maxParticipants
                }{" "}
                fő
              </strong>
            </div>

            <div>
              <span>
                Várólista
              </span>
              <strong>
                {
                  session.waitlistCount
                }{" "}
                fő
              </strong>
            </div>

            <div>
              <span>Szervező</span>
              <strong>
                {
                  session.organizerName
                }
              </strong>
            </div>
          </div>

          <div className="training-plan-reference">
            <span>
              Kapcsolódó edzésterv
            </span>

            {session.trainingPlanId &&
            session.trainingPlanTitle ? (
              <>
                <strong>
                  {
                    session.trainingPlanTitle
                  }
                </strong>

                <div className="training-session-actions">
                  <Link
                    to={`/plans/${session.trainingPlanId}`}
                    className="secondary-button"
                  >
                    Edzésterv megnyitása
                  </Link>

                  {isSessionOrganizer &&
                    isUpcoming && (
                    <button
                      type="button"
                      className="danger-button"
                      onClick={
                        handleRemoveTrainingPlan
                      }
                      disabled={
                        trainingPlanLoading
                      }
                    >
                      Leválasztás
                    </button>
                  )}
                </div>
              </>
            ) : (
              <strong>
                Nincs edzésterv
                hozzárendelve.
              </strong>
            )}
          </div>

          {isSessionOrganizer &&
            isUpcoming && (
            <div className="card">
              <h2>
                Edzésterv
                hozzárendelése
              </h2>

              <p className="info-text">
                Csak azonos sportágú,
                {` ${sessionDurationMinutes} `}
                perces terveket
                választhatsz.
              </p>

              {compatibleTrainingPlans.length >
              0 ? (
                <>
                  <div className="form-field">
                    <label
                      htmlFor="trainingPlan"
                    >
                      Edzésterv
                    </label>

                    <select
                      id="trainingPlan"
                      value={
                        selectedTrainingPlanId
                      }
                      onChange={e =>
                        setSelectedTrainingPlanId(
                          Number(
                            e.target
                              .value
                          )
                        )
                      }
                    >
                      <option
                        value={0}
                      >
                        Válassz
                        edzéstervet
                      </option>

                      {compatibleTrainingPlans.map(
                        plan => (
                          <option
                            key={
                              plan.id
                            }
                            value={
                              plan.id
                            }
                          >
                            {
                              plan.title
                            }{" "}
                            –{" "}
                            {
                              plan.targetDuration
                            }{" "}
                            perc
                          </option>
                        )
                      )}
                    </select>
                  </div>

                  <button
                    type="button"
                    className="primary-button"
                    onClick={
                      handleAssignTrainingPlan
                    }
                    disabled={
                      trainingPlanLoading ||
                      selectedTrainingPlanId ===
                        0
                    }
                  >
                    {trainingPlanLoading
                      ? "Mentés..."
                      : session.trainingPlanId
                        ? "Edzésterv cseréje"
                        : "Edzésterv hozzárendelése"}
                  </button>
                </>
              ) : (
                <p className="info-text">
                  Nincs olyan mentett
                  edzésterved, amely
                  sportágban és
                  időtartamban megfelel
                  ennek az edzésnek.
                </p>
              )}
            </div>
          )}

          {isSessionOrganizer && (
            <div className="training-participants-section">
              <div className="training-participants-header">
                <div>
                  <h2>
                    Jelentkezők
                  </h2>

                  <p className="info-text">
                    {
                      confirmedParticipants.length
                    }{" "}
                    /{" "}
                    {
                      session.maxParticipants
                    }{" "}
                    fő
                  </p>
                </div>

                <button
                  type="button"
                  className="secondary-button"
                  onClick={
                    loadParticipants
                  }
                  disabled={
                    participantsLoading
                  }
                >
                  {participantsLoading
                    ? "Frissítés..."
                    : "Frissítés"}
                </button>
              </div>

              {confirmedParticipants.length ===
              0 ? (
                <p className="info-text">
                  Még senki nem
                  jelentkezett.
                </p>
              ) : (
                <div className="training-participant-list">
                  {confirmedParticipants.map(
                    (
                      participant,
                      index
                    ) => (
                      <div
                        key={
                          participant.userId
                        }
                        className="training-participant-item"
                      >
                        <div className="training-participant-number">
                          {index + 1}
                        </div>

                        <div>
                          <strong>
                            {
                              participant.userName
                            }
                          </strong>

                          <p>
                            Jelentkezés:{" "}
                            {formatDateTime(
                              participant.registeredAt
                            )}
                          </p>
                        </div>
                      </div>
                    )
                  )}
                </div>
              )}

              <div className="training-participants-section">
                <h2>
                  Várólista
                </h2>

                {waitlistedParticipants.length ===
                0 ? (
                  <p className="info-text">
                    A várólista üres.
                  </p>
                ) : (
                  <div className="training-participant-list">
                    {waitlistedParticipants.map(
                      participant => (
                        <div
                          key={
                            participant.userId
                          }
                          className="training-participant-item"
                        >
                          <div className="training-participant-number">
                            {
                              participant.waitlistPosition
                            }
                          </div>

                          <div>
                            <strong>
                              {
                                participant.userName
                              }
                            </strong>
                          </div>
                        </div>
                      )
                    )}
                  </div>
                )}
              </div>
            </div>
          )}
        </section>

        <aside className="card training-booking-panel">
          {isSessionOrganizer ? (
            <>
              <h2>
                Szervezői nézet
              </h2>

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
                }
              </p>

              <p>
                <strong>
                  Várólista:
                </strong>{" "}
                {
                  session.waitlistCount
                }
              </p>
              {isUpcoming && (
                <div className="training-organizer-actions">
                  <Link
                    to={`/trainings/${session.id}/edit`}
                    className="primary-button"
                  >
                    Szerkesztés
                  </Link>

                  <button
                    type="button"
                    className="danger-button"
                    onClick={
                      handleDeleteTrainingSession
                    }
                    disabled={deleteLoading}
                  >
                    {deleteLoading
                      ? "Törlés..."
                      : "Edzés törlése"}
                  </button>
                </div>
              )}
            </>
          ) : isPlayer ? (
            <>
              <h2>
                Jelentkezés
              </h2>

              <p>
                <strong>
                  Foglalt helyek:
                </strong>{" "}
                {
                  session.participantCount
                }{" "}
                /{" "}
                {
                  session.maxParticipants
                }
              </p>

              {isOngoing ? (
                <p className="info-text">
                  Az edzés jelenleg
                  zajlik. Jelentkezni vagy
                  lejelentkezni már nem
                  lehet.
                </p>
              ) : isPast ? (
                <p className="info-text">
                  Ez az edzés már
                  lezajlott.
                </p>
              ) : session
                  .currentUserBookingStatus ===
                "Confirmed" ? (
                <>
                  <p className="success-text">
                    Biztos helyed van az
                    edzésen.
                  </p>

                  <button
                    type="button"
                    className="danger-button"
                    onClick={
                      handleCancelBooking
                    }
                    disabled={
                      bookingLoading
                    }
                  >
                    Jelentkezés
                    lemondása
                  </button>
                </>
              ) : session
                  .currentUserBookingStatus ===
                "Waitlisted" ? (
                <>
                  <p className="info-text">
                    A várólistán vagy.
                  </p>

                  <p>
                    <strong>
                      Pozíció:
                    </strong>{" "}
                    {
                      session.waitlistPosition
                    }
                    .
                  </p>

                  <button
                    type="button"
                    className="secondary-button"
                    onClick={
                      handleRefreshStatus
                    }
                    disabled={
                      bookingLoading
                    }
                  >
                    Állapot
                    frissítése
                  </button>

                  <button
                    type="button"
                    className="danger-button"
                    onClick={
                      handleCancelBooking
                    }
                    disabled={
                      bookingLoading
                    }
                  >
                    Várólista
                    lemondása
                  </button>
                </>
              ) : (
                <>
                  <p className="info-text">
                    {session.isFull
                      ? "Az edzés betelt, de feliratkozhatsz a várólistára."
                      : "Jelentkezz az edzésre az alábbi gombbal."}
                  </p>

                  <button
                    type="button"
                    className="primary-button"
                    onClick={
                      handleBooking
                    }
                    disabled={
                      bookingLoading
                    }
                  >
                    {session.isFull
                      ? "Jelentkezem a várólistára"
                      : "Jelentkezem"}
                  </button>
                </>
              )}
            </>
          ) : isOtherOrganizer ? (
            <>
              <h2>
                Edzés információ
              </h2>

              <p className="info-text">
                Ezt az edzést egy másik
                szervező hozta létre.
              </p>
            </>
          ) : null}

          <hr />

          <h2>
            Tervező
          </h2>

          {session
            .isInCurrentUserCalendar ? (
            <>
              <p className="success-text">
                Ez az edzés már szerepel
                a terveződben.
              </p>

              <Link
                to="/planner"
                className="primary-button training-back-link"
              >
                Tervező megnyitása
              </Link>
            </>
          ) : canAddToCalendar ? (
            <>
              <p className="info-text">
                Hozzáadhatod ezt az
                edzést a heti
                terveződhöz.
              </p>

              <button
                type="button"
                className="primary-button"
                onClick={
                  handleAddToCalendar
                }
                disabled={
                  calendarLoading
                }
              >
                {calendarLoading
                  ? "Hozzáadás..."
                  : "Hozzáadás a tervezőhöz"}
              </button>
            </>
          ) : isPlayer &&
            session
              .currentUserBookingStatus ===
              "Waitlisted" ? (
            <p className="info-text">
              Várólistásként még nem
              adhatod hozzá az edzést a
              terveződhöz. Ha biztos
              helyre kerülsz, a funkció
              elérhetővé válik.
            </p>
          ) : (
            <p className="info-text">
              Ez az edzés jelenleg nem
              adható hozzá a
              terveződhöz.
            </p>
          )}

          {error && (
            <p className="error-text">
              {error}
            </p>
          )}

          {successMessage && (
            <p className="success-text">
              {successMessage}
            </p>
          )}

          <Link
            to="/trainings"
            className="secondary-button training-back-link"
          >
            Vissza az edzésekhez
          </Link>
        </aside>
      </div>
    </AppLayout>
  );
}

export default TrainingSessionDetailsPage;