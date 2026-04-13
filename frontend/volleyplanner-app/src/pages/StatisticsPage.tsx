import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/layout/AppLayout";
import { statisticsService } from "../services/statisticsService";
import type { StatisticsSummary } from "../types/statistics";

function StatisticsPage() {
  const [stats, setStats] = useState<StatisticsSummary | null>(null);
  const [range, setRange] = useState("30days");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadStats = async () => {
      try {
        const data = await statisticsService.getMyStatistics(range);
        setStats(data);
      } catch {
        setError("Nem sikerült betölteni a statisztikákat.");
      } finally {
        setLoading(false);
      }
    };

    setLoading(true);
    setError("");
    void loadStats();
  }, [range]);

  const totalHours = useMemo(() => {
    if (!stats) return "0.0";
    return (stats.totalDurationMinutes / 60).toFixed(1);
  }, [stats]);

  const maxDailyCount = useMemo(() => {
    if (!stats || stats.weeklyDailyStats.length === 0) return 1;
    return Math.max(...stats.weeklyDailyStats.map((item) => item.count), 1);
  }, [stats]);

  const topFocus = useMemo(() => {
    if (!stats || stats.focusUsageStats.length === 0) {
      return null;
    }

    return stats.focusUsageStats[0];
  }, [stats]);

  return (
    <AppLayout
      title="Statisztikák"
      subtitle="Kövesd nyomon az edzéseid mennyiségét és fókuszterületeit."
    >
      <section className="card" style={{ marginBottom: "1.5rem" }}>
        <div className="form-field" style={{ maxWidth: "280px" }}>
          <label htmlFor="stats-range">Időszak</label>
          <select
            id="stats-range"
            value={range}
            onChange={(e) => setRange(e.target.value)}
          >
            <option value="7days">Elmúlt 7 nap</option>
            <option value="30days">Elmúlt 30 nap</option>
            <option value="month">Aktuális hónap</option>
            <option value="all">Összes</option>
          </select>
        </div>
      </section>

      {loading && <p className="info-text">Betöltés...</p>}
      {error && <p className="error-text">{error}</p>}

      {!loading && stats && (
        <>
          <section
            className="card-grid"
            style={{
              gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))",
              marginBottom: "1.5rem",
            }}
          >
            <article className="card">
              <h3 className="exercise-card-title">Heti edzések</h3>
              <p className="exercise-meta">{stats.weeklyCount} db</p>
            </article>

            <article className="card">
              <h3 className="exercise-card-title">Havi edzések</h3>
              <p className="exercise-meta">{stats.monthlyCount} db</p>
            </article>

            <article className="card">
              <h3 className="exercise-card-title">Összes esemény</h3>
              <p className="exercise-meta">{stats.totalEvents} db</p>
            </article>

            <article className="card">
              <h3 className="exercise-card-title">Összes idő</h3>
              <p className="exercise-meta">{totalHours} óra</p>
            </article>

            <article className="card">
              <h3 className="exercise-card-title">Legfőbb fókusz</h3>
              <p className="exercise-meta">
                {topFocus ? `${topFocus.focusName} (${topFocus.count})` : "Nincs adat"}
              </p>
            </article>
          </section>

          <section className="card" style={{ marginBottom: "1.5rem" }}>
            <h2 style={{ marginTop: 0 }}>Heti edzéseloszlás</h2>

            <div
              style={{
                display: "flex",
                alignItems: "flex-end",
                gap: "1rem",
                minHeight: "220px",
                paddingTop: "1rem",
              }}
            >
              {stats.weeklyDailyStats.map((item) => {
                const height = `${(item.count / maxDailyCount) * 160}px`;

                return (
                  <div
                    key={item.dayLabel}
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      alignItems: "center",
                      flex: 1,
                      gap: "0.5rem",
                    }}
                  >
                    <span className="info-text">{item.count} edzés</span>

                    <div
                      style={{
                        width: "100%",
                        maxWidth: "48px",
                        height,
                        minHeight: item.count > 0 ? "12px" : "4px",
                        borderRadius: "12px 12px 0 0",
                        background:
                          "linear-gradient(135deg, #6ee7ff 0%, #34c6f3 45%, #148fd0 100%)",
                      }}
                    />

                    <strong>{item.dayLabel}</strong>
                    <span className="info-text">{item.durationMinutes} perc</span>
                  </div>
                );
              })}
            </div>
          </section>

          <section className="card" style={{ marginBottom: "1.5rem" }}>
            <h2 style={{ marginTop: 0 }}>Leggyakrabban használt edzéstervek</h2>

            {stats.topPlans.length === 0 ? (
              <p className="info-text">Még nincs elég adat.</p>
            ) : (
              <div className="card-grid">
                {stats.topPlans.map((plan) => (
                  <article key={plan.trainingPlanId} className="card">
                    <h3 className="exercise-card-title">{plan.title}</h3>
                    <p className="exercise-meta">
                      Felhasználás: {plan.count} alkalom
                    </p>
                  </article>
                ))}
              </div>
            )}
          </section>

          <section className="card">
            <h2 style={{ marginTop: 0 }}>Leggyakoribb fókuszterületek</h2>

            {stats.focusUsageStats.length === 0 ? (
              <p className="info-text">Még nincs elég fókuszadat.</p>
            ) : (
              <div className="card-grid">
                {stats.focusUsageStats.map((focus) => (
                  <article key={focus.focusName} className="card">
                    <h3 className="exercise-card-title">{focus.focusName}</h3>
                    <p className="exercise-meta">
                      Előfordulás: {focus.count}
                    </p>
                  </article>
                ))}
              </div>
            )}
          </section>
        </>
      )}
    </AppLayout>
  );
}

export default StatisticsPage;