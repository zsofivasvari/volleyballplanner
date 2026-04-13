import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { AxiosError } from "axios";
import AppLayout from "../components/layout/AppLayout";
import { plannerService } from "../services/plannerService";
import { trainingPlanService } from "../services/trainingPlanService";
import type {
  CalendarEvent,
  CreateCalendarEventRequest,
} from "../types/planner";
import type { TrainingPlanListItem } from "../types/trainingPlan";

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

  const [formData, setFormData] = useState<CreateCalendarEventRequest>({
    trainingPlanId: 0,
    title: "",
    startTime: "",
    endTime: "",
  });

  const calculateEndTime = (startTime: string, durationMinutes: number) => {
    if (!startTime || durationMinutes <= 0) {
      return "";
    }

    const start = new Date(startTime);
    const end = new Date(start.getTime() + durationMinutes * 60000);

    const year = end.getFullYear();
    const month = String(end.getMonth() + 1).padStart(2, "0");
    const day = String(end.getDate()).padStart(2, "0");
    const hours = String(end.getHours()).padStart(2, "0");
    const minutes = String(end.getMinutes()).padStart(2, "0");

    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const loadData = async () => {
    setLoading(true);
    setError("");

    const requestedPlanId = Number(searchParams.get("planId"));

    try {
      const plansData = await trainingPlanService.getAll();
      setPlans(plansData);

      if (plansData.length > 0) {
        let selectedPlan: TrainingPlanListItem | undefined;

        if (!Number.isNaN(requestedPlanId) && requestedPlanId > 0) {
          selectedPlan = plansData.find((p) => p.id === requestedPlanId);
        }

        if (!selectedPlan) {
          selectedPlan =
            plansData.find((p) => p.id === formData.trainingPlanId) ??
            plansData[0];
        }

        if (selectedPlan) {
          setFormData((prev) => ({
            ...prev,
            trainingPlanId: selectedPlan.id,
            title: selectedPlan.title,
            endTime:
              prev.startTime && selectedPlan
                ? calculateEndTime(prev.startTime, selectedPlan.targetDuration)
                : prev.endTime,
          }));
        }
      } else {
        setFormData((prev) => ({
          ...prev,
          trainingPlanId: 0,
          title: "",
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

  const weekDays = useMemo(() => {
    return Array.from({ length: 7 }, (_, index) => {
      const date = new Date(currentWeekStart);
      date.setDate(currentWeekStart.getDate() + index);
      return date;
    });
  }, [currentWeekStart]);

  const weekStart = weekDays[0];
  const weekEnd = weekDays[6];

  const hours = useMemo(() => Array.from({ length: 13 }, (_, i) => 8 + i), []);

  const handlePlanChange = (planId: number) => {
    const selectedPlan = plans.find((p) => p.id === planId);

    setFormData((prev) => ({
      ...prev,
      trainingPlanId: planId,
      title: selectedPlan?.title || "",
      endTime:
        prev.startTime && selectedPlan
          ? calculateEndTime(prev.startTime, selectedPlan.targetDuration)
          : prev.endTime,
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

    if (!formData.startTime || !formData.endTime) {
      setSaveError("Add meg a kezdési és befejezési időpontot.");
      return;
    }

    if (new Date(formData.endTime) <= new Date(formData.startTime)) {
      setSaveError(
        "A befejezési idő nem lehet korábbi vagy azonos a kezdési idővel."
      );
      return;
    }

    try {
      await plannerService.create(formData);
      setSaveMessage("Az edzés sikeresen hozzáadva a heti tervhez.");

      await loadData();

      const selectedPlan = plans.find((p) => p.id === formData.trainingPlanId);

      setFormData((prev) => ({
        ...prev,
        title: selectedPlan?.title || "",
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

    if (!confirmed) {
      return;
    }

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
              Válassz egy mentett tervet, majd add meg az időpontját.
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

              {plans.length === 0 && (
                <p className="info-text">
                  Még nincs mentett edzésterved. Előbb generálj és ments egy
                  tervet.
                </p>
              )}
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
              <label htmlFor="startTime">Kezdés</label>
              <input
                id="startTime"
                type="datetime-local"
                value={formData.startTime}
                onChange={(e) => {
                  const newStartTime = e.target.value;
                  const selectedPlan = plans.find(
                    (p) => p.id === formData.trainingPlanId
                  );

                  setFormData((prev) => ({
                    ...prev,
                    startTime: newStartTime,
                    endTime:
                      selectedPlan && newStartTime
                        ? calculateEndTime(
                            newStartTime,
                            selectedPlan.targetDuration
                          )
                        : prev.endTime,
                  }));
                }}
              />
            </div>

            <div className="form-field">
              <label htmlFor="endTime">Befejezés</label>
              <input
                id="endTime"
                type="datetime-local"
                value={formData.endTime}
                onChange={(e) =>
                  setFormData((prev) => ({
                    ...prev,
                    endTime: e.target.value,
                  }))
                }
              />
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
        <div
          className="toolbar"
          style={{
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: "1rem",
          }}
        >
          <div>
            <h2 style={{ margin: 0 }}>Heti nézet</h2>
            <p className="page-subtitle">
              {weekStart.toLocaleDateString("hu-HU")} –{" "}
              {weekEnd.toLocaleDateString("hu-HU")}
            </p>
          </div>

          <div className="toolbar">
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
                        {day.toLocaleDateString("hu-HU", { weekday: "short" })}
                      </strong>
                      <div className="info-text">
                        {day.toLocaleDateString("hu-HU")}
                      </div>
                    </div>

                    <div className="day-slots">
                      {hours.map((hour) => (
                        <div key={hour} className="hour-slot" />
                      ))}

                      {dayEvents.map((event) => {
                        const start = new Date(event.startTime);
                        const end = new Date(event.endTime);

                        const startMinutes =
                          (start.getHours() - 8) * 60 + start.getMinutes();
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
                              {start.toLocaleTimeString("hu-HU", {
                                hour: "2-digit",
                                minute: "2-digit",
                              })}
                              {" - "}
                              {end.toLocaleTimeString("hu-HU", {
                                hour: "2-digit",
                                minute: "2-digit",
                              })}
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
        )}
      </section>
    </AppLayout>
  );
}

export default WeeklyPlannerPage;