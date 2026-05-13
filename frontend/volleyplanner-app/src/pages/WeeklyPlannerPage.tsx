import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { AxiosError } from "axios";
import AppLayout from "../components/layout/AppLayout";
import { plannerService } from "../services/plannerService";
import { trainingPlanService } from "../services/trainingPlanService";
import type { CalendarEvent } from "../types/planner";
import type { TrainingPlanListItem } from "../types/trainingPlan";

const CALENDAR_START_HOUR = 6;
const CALENDAR_END_HOUR = 22;
const SLOT_STEP_MINUTES = 30;

function WeeklyPlannerPage() {
  const [searchParams] = useSearchParams();

  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [plans, setPlans] = useState<TrainingPlanListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [saveMessage, setSaveMessage] = useState("");
  const [saveError, setSaveError] = useState("");

  const [currentWeekStart, setCurrentWeekStart] = useState(() => {
    const today = new Date();
    const day = today.getDay();
    const diff = day === 0 ? -6 : 1 - day;
    const monday = new Date(today);
    monday.setDate(today.getDate() + diff);
    monday.setHours(0, 0, 0, 0);
    return monday;
  });

  const todayDate = useMemo(() => {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  }, []);

  const [formData, setFormData] = useState({
    trainingPlanId: 0,
    title: "",
    date: todayDate,
    startTime: "",
    endTime: "",
  });

  const selectedPlan = useMemo(
    () => plans.find((p) => p.id === formData.trainingPlanId),
    [plans, formData.trainingPlanId]
  );

  const getTotalMinutes = (time: string) => {
    const [hours, minutes] = time.split(":").map(Number);
    return hours * 60 + minutes;
  };

  const calculateEndTime = (startTime: string, durationMinutes: number) => {
    if (!startTime || durationMinutes <= 0) return "";

    const startTotalMinutes = getTotalMinutes(startTime);
    const endTotalMinutes = startTotalMinutes + durationMinutes;

    if (endTotalMinutes > CALENDAR_END_HOUR * 60) return "";

    const endHours = String(Math.floor(endTotalMinutes / 60)).padStart(2, "0");
    const endMinutes = String(endTotalMinutes % 60).padStart(2, "0");

    return `${endHours}:${endMinutes}`;
  };

  const combineDateAndTime = (date: string, time: string) => {
    return `${date}T${time}`;
  };

  const timeOptions = useMemo(() => {
    const options: string[] = [];

    for (
      let minutes = CALENDAR_START_HOUR * 60;
      minutes <= CALENDAR_END_HOUR * 60;
      minutes += SLOT_STEP_MINUTES
    ) {
      const hours = String(Math.floor(minutes / 60)).padStart(2, "0");
      const mins = String(minutes % 60).padStart(2, "0");
      options.push(`${hours}:${mins}`);
    }

    return options;
  }, []);

  const startTimeOptions = useMemo(() => {
    if (!selectedPlan) {
      return timeOptions.filter((time) => time < "22:00");
    }

    return timeOptions.filter((time) => {
      if (time >= "22:00") return false;
      return calculateEndTime(time, selectedPlan.targetDuration) !== "";
    });
  }, [timeOptions, selectedPlan]);

  const endTimeOptions = useMemo(() => {
    if (!formData.startTime) {
      return timeOptions.filter((time) => time > "06:00");
    }

    if (selectedPlan) {
      const autoEnd = calculateEndTime(
        formData.startTime,
        selectedPlan.targetDuration
      );

      return autoEnd ? [autoEnd] : [];
    }

    return timeOptions.filter((time) => time > formData.startTime);
  }, [formData.startTime, timeOptions, selectedPlan]);

  const loadData = async () => {
    setLoading(true);
    setError("");

    const requestedPlanId = Number(searchParams.get("planId"));

    try {
      const plansData = await trainingPlanService.getAll();
      setPlans(plansData);

      if (plansData.length > 0) {
        let nextSelectedPlan: TrainingPlanListItem | undefined;

        if (!Number.isNaN(requestedPlanId) && requestedPlanId > 0) {
          nextSelectedPlan = plansData.find((p) => p.id === requestedPlanId);
        }

        if (!nextSelectedPlan) {
          nextSelectedPlan =
            plansData.find((p) => p.id === formData.trainingPlanId) ??
            plansData[0];
        }

        setFormData((prev) => ({
          ...prev,
          trainingPlanId: nextSelectedPlan!.id,
          title: nextSelectedPlan!.title,
          endTime:
            prev.startTime && nextSelectedPlan
              ? calculateEndTime(prev.startTime, nextSelectedPlan.targetDuration)
              : prev.endTime,
        }));
      } else {
        setFormData((prev) => ({
          ...prev,
          trainingPlanId: 0,
          title: "",
          startTime: "",
          endTime: "",
        }));
      }
    } catch {
      setError("Nem sikerült betölteni a mentett edzésterveket.");
      setPlans([]);
    }

    try {
      const eventsData = await plannerService.getAll();
      setEvents(eventsData);
    } catch {
      setError((prev) =>
        prev
          ? `${prev} Az eseményeket sem sikerült betölteni.`
          : "Nem sikerült betölteni a heti eseményeket."
      );
      setEvents([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadData();
  }, [searchParams]);

  useEffect(() => {
    if (!selectedPlan || !formData.startTime) return;

    const newEndTime = calculateEndTime(
      formData.startTime,
      selectedPlan.targetDuration
    );

    if (!newEndTime) {
      setFormData((prev) => ({
        ...prev,
        startTime: "",
        endTime: "",
      }));
      setSaveError(
        "Ehhez az edzéstervhez ez a kezdési idő már nem fér bele 22:00 óráig."
      );
      return;
    }

    setFormData((prev) => ({
      ...prev,
      endTime: newEndTime,
    }));
  }, [selectedPlan, formData.startTime]);

  const weekDays = useMemo(() => {
    return Array.from({ length: 7 }, (_, index) => {
      const date = new Date(currentWeekStart);
      date.setDate(currentWeekStart.getDate() + index);
      return date;
    });
  }, [currentWeekStart]);

  const weekStart = weekDays[0];
  const weekEnd = weekDays[6];

  const hours = useMemo(
    () =>
      Array.from(
        { length: CALENDAR_END_HOUR - CALENDAR_START_HOUR + 1 },
        (_, i) => CALENDAR_START_HOUR + i
      ),
    []
  );

  const handlePlanChange = (planId: number) => {
    const plan = plans.find((p) => p.id === planId);
    setSaveError("");

    setFormData((prev) => {
      const nextEndTime =
        prev.startTime && plan
          ? calculateEndTime(prev.startTime, plan.targetDuration)
          : prev.endTime;

      return {
        ...prev,
        trainingPlanId: planId,
        title: plan?.title || "",
        startTime: prev.startTime && plan && !nextEndTime ? "" : prev.startTime,
        endTime: nextEndTime,
      };
    });
  };

  const handleStartTimeChange = (value: string) => {
    setSaveError("");

    if (!selectedPlan) {
      setFormData((prev) => ({
        ...prev,
        startTime: value,
        endTime: "",
      }));
      return;
    }

    const newEndTime = calculateEndTime(value, selectedPlan.targetDuration);

    if (!newEndTime) {
      setSaveError(
        "Ez a kezdési idő már nem fér bele a terv időtartamával 22:00 óráig."
      );
      return;
    }

    setFormData((prev) => ({
      ...prev,
      startTime: value,
      endTime: newEndTime,
    }));
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setSaveMessage("");
    setSaveError("");

    if (formData.trainingPlanId === 0) {
      setSaveError("Válassz ki egy mentett edzéstervet.");
      return;
    }

    if (!formData.date || !formData.startTime || !formData.endTime) {
      setSaveError("Add meg a dátumot, a kezdési és a befejezési időpontot.");
      return;
    }

    const startMinutes = getTotalMinutes(formData.startTime);
    const endMinutes = getTotalMinutes(formData.endTime);

    if (
      startMinutes < CALENDAR_START_HOUR * 60 ||
      startMinutes >= CALENDAR_END_HOUR * 60
    ) {
      setSaveError("A kezdési idő csak 06:00 és 21:30 között lehet.");
      return;
    }

    if (
      endMinutes <= CALENDAR_START_HOUR * 60 ||
      endMinutes > CALENDAR_END_HOUR * 60
    ) {
      setSaveError("A befejezési idő csak 06:30 és 22:00 között lehet.");
      return;
    }

    if (selectedPlan) {
      const expectedEnd = calculateEndTime(
        formData.startTime,
        selectedPlan.targetDuration
      );

      if (!expectedEnd || expectedEnd !== formData.endTime) {
        setSaveError(
          "A befejezési időnek illeszkednie kell a kiválasztott terv időtartamához."
        );
        return;
      }
    }

    try {
      await plannerService.create({
        trainingPlanId: formData.trainingPlanId,
        title: formData.title,
        startTime: combineDateAndTime(formData.date, formData.startTime),
        endTime: combineDateAndTime(formData.date, formData.endTime),
      });

      setSaveMessage("Az edzés sikeresen hozzáadva a heti tervhez.");

      await loadData();

      const plan = plans.find((p) => p.id === formData.trainingPlanId);

      setFormData((prev) => ({
        ...prev,
        title: plan?.title || "",
        startTime: "",
        endTime: "",
      }));
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setSaveError(err.response?.data?.message || "Nem sikerült menteni.");
      } else {
        setSaveError("Nem sikerült menteni.");
      }
    }
  };

  const handleDeleteEvent = async (eventId: number) => {
    const confirmed = window.confirm(
      "Biztosan törölni szeretnéd ezt az eseményt?"
    );

    if (!confirmed) return;

    setSaveMessage("");
    setSaveError("");

    try {
      await plannerService.remove(eventId);
      setEvents((prev) => prev.filter((event) => event.id !== eventId));
      setSaveMessage("Az esemény sikeresen törölve.");
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setSaveError(
          err.response?.data?.message || "Nem sikerült törölni az eseményt."
        );
      } else {
        setSaveError("Nem sikerült törölni az eseményt.");
      }
    }
  };

  const goToPreviousWeek = () => {
    const previous = new Date(currentWeekStart);
    previous.setDate(previous.getDate() - 7);
    setCurrentWeekStart(previous);
  };

  const goToNextWeek = () => {
    const next = new Date(currentWeekStart);
    next.setDate(next.getDate() + 7);
    setCurrentWeekStart(next);
  };

  const goToCurrentWeek = () => {
    const today = new Date();
    const day = today.getDay();
    const diff = day === 0 ? -6 : 1 - day;
    const monday = new Date(today);
    monday.setDate(today.getDate() + diff);
    monday.setHours(0, 0, 0, 0);
    setCurrentWeekStart(monday);
  };

  const getEventsForDay = (day: Date) => {
    return events
      .filter((event) => {
        const eventDate = new Date(event.startTime);
        return (
          eventDate.getFullYear() === day.getFullYear() &&
          eventDate.getMonth() === day.getMonth() &&
          eventDate.getDate() === day.getDate()
        );
      })
      .sort(
        (a, b) =>
          new Date(a.startTime).getTime() - new Date(b.startTime).getTime()
      );
  };

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString("hu-HU", {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <AppLayout
      title="Heti tervező"
      subtitle="Szervezd a mentett edzésterveket heti bontásban."
    >
      <section className="card" style={{ marginBottom: "1.5rem" }}>
        <div className="page-header" style={{ marginBottom: "1rem" }}>
          <div>
            <h2 style={{ margin: 0 }}>Új edzés hozzáadása</h2>
            <p className="page-subtitle">
              Válassz egy mentett tervet, majd add meg a dátumát és az
              időpontját.
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="filters-grid" style={{ marginBottom: "1rem" }}>
            <div className="form-field">
              <label htmlFor="trainingPlanId">Edzésterv</label>
              <select
                id="trainingPlanId"
                value={formData.trainingPlanId}
                onChange={(e) => handlePlanChange(Number(e.target.value))}
                disabled={plans.length === 0}
              >
                {plans.length === 0 ? (
                  <option value={0}>Nincs elérhető mentett terv</option>
                ) : (
                  plans.map((plan) => (
                    <option key={plan.id} value={plan.id}>
                      {plan.title}
                    </option>
                  ))
                )}
              </select>
            </div>

            <div className="form-field">
              <label htmlFor="title">Cím</label>
              <input
                id="title"
                type="text"
                value={formData.title}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, title: e.target.value }))
                }
              />
            </div>

            <div className="form-field">
              <label htmlFor="date">Dátum</label>
              <input
                id="date"
                type="date"
                value={formData.date}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, date: e.target.value }))
                }
              />
            </div>

            <div className="form-field">
              <label htmlFor="startTime">Kezdés</label>
              <select
                id="startTime"
                value={formData.startTime}
                onChange={(e) => handleStartTimeChange(e.target.value)}
              >
                <option value="">Válassz kezdési időt</option>
                {startTimeOptions.map((time) => (
                  <option key={time} value={time}>
                    {time}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-field">
              <label htmlFor="endTime">Befejezés</label>
              <select
                id="endTime"
                value={formData.endTime}
                disabled={!!selectedPlan}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    endTime: e.target.value,
                  }))
                }
              >
                <option value="">Automatikus befejezés</option>
                {endTimeOptions.map((time) => (
                  <option key={time} value={time}>
                    {time}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="toolbar">
            <button className="primary-button" type="submit">
              Hozzáadás a heti tervhez
            </button>
          </div>
        </form>

        {saveMessage && (
          <p className="success-text" style={{ marginTop: "1rem" }}>
            {saveMessage}
          </p>
        )}

        {saveError && (
          <p className="error-text" style={{ marginTop: "1rem" }}>
            {saveError}
          </p>
        )}
      </section>

      <section className="card">
        <div className="planner-header">
          <div>
            <h2 style={{ margin: 0 }}>Heti nézet</h2>
            <p className="page-subtitle">
              {weekStart.toLocaleDateString("hu-HU")} –{" "}
              {weekEnd.toLocaleDateString("hu-HU")}
            </p>
          </div>

          <div className="planner-week-buttons">
            <button
              type="button"
              className="secondary-button"
              onClick={goToPreviousWeek}
            >
              Előző hét
            </button>
            <button
              type="button"
              className="secondary-button"
              onClick={goToCurrentWeek}
            >
              Aktuális hét
            </button>
            <button
              type="button"
              className="secondary-button"
              onClick={goToNextWeek}
            >
              Következő hét
            </button>
          </div>
        </div>

        {loading && <p className="info-text">Betöltés...</p>}
        {error && <p className="error-text">{error}</p>}

        {!loading && (
          <>
            <div className="desktop-week-calendar">
              <div className="week-calendar-wrapper">
                <div className="week-calendar">
                  <div className="time-column">
                    <div className="calendar-header-cell"></div>
                    {hours.map((hour) => (
                      <div key={hour} className="time-cell">
                        {hour}:00
                      </div>
                    ))}
                  </div>

                  {weekDays.map((day) => {
                    const dayEvents = getEventsForDay(day);

                    return (
                      <div key={day.toISOString()} className="day-column">
                        <div className="calendar-header-cell day-header-cell">
                          <strong>
                            {day.toLocaleDateString("hu-HU", {
                              weekday: "short",
                            })}
                          </strong>
                          <div className="info-text">
                            {day.toLocaleDateString("hu-HU")}
                          </div>
                        </div>

                        <div className="day-slots" style={{ height: "960px" }}>
                          {hours.map((hour) => (
                            <div key={hour} className="hour-slot" />
                          ))}

                          {dayEvents.map((event) => {
                            const start = new Date(event.startTime);
                            const end = new Date(event.endTime);

                            const startMinutes =
                              (start.getHours() - CALENDAR_START_HOUR) * 60 +
                              start.getMinutes();

                            const durationMinutes =
                              end.getHours() * 60 +
                              end.getMinutes() -
                              (start.getHours() * 60 + start.getMinutes());

                            const top = startMinutes;
                            const height = Math.max(durationMinutes, 45);

                            return (
                              <div
                                key={event.id}
                                className="calendar-event-block"
                                style={{
                                  top: `${top}px`,
                                  height: `${height}px`,
                                }}
                              >
                                <button
                                  type="button"
                                  className="calendar-event-delete"
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    void handleDeleteEvent(event.id);
                                  }}
                                  title="Esemény törlése"
                                >
                                  ×
                                </button>

                                <Link
                                  to={`/plans/${event.trainingPlanId}`}
                                  className="calendar-event-title"
                                  onClick={(e) => e.stopPropagation()}
                                >
                                  {event.title}
                                </Link>

                                <div className="calendar-event-time">
                                  {formatTime(start)} - {formatTime(end)}
                                </div>

                                <div className="calendar-event-meta">
                                  {event.sportType}
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>

            <div className="mobile-week-list">
              {weekDays.map((day) => {
                const dayEvents = getEventsForDay(day);

                return (
                  <section key={day.toISOString()} className="mobile-day-card">
                    <div className="mobile-day-header">
                      <div>
                        <strong>
                          {day.toLocaleDateString("hu-HU", {
                            weekday: "long",
                          })}
                        </strong>
                        <span>{day.toLocaleDateString("hu-HU")}</span>
                      </div>
                    </div>

                    {dayEvents.length === 0 ? (
                      <p className="info-text">Nincs edzés erre a napra.</p>
                    ) : (
                      <div className="mobile-event-list">
                        {dayEvents.map((event) => {
                          const start = new Date(event.startTime);
                          const end = new Date(event.endTime);

                          return (
                            <article
                              key={event.id}
                              className="mobile-event-card"
                            >
                              <div>
                                <Link
                                  to={`/plans/${event.trainingPlanId}`}
                                  className="mobile-event-title"
                                >
                                  {event.title}
                                </Link>

                                <p className="mobile-event-time">
                                  {formatTime(start)} - {formatTime(end)}
                                </p>

                                <p className="mobile-event-meta">
                                  {event.sportType}
                                </p>
                              </div>

                              <button
                                type="button"
                                className="danger-button"
                                onClick={() => void handleDeleteEvent(event.id)}
                              >
                                Törlés
                              </button>
                            </article>
                          );
                        })}
                      </div>
                    )}
                  </section>
                );
              })}
            </div>
          </>
        )}
      </section>
    </AppLayout>
  );
}

export default WeeklyPlannerPage;