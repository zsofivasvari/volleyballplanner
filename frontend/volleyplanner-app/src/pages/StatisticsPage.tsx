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
    if (!stats || stats.focusUsageStats.length === 0) return null;
    return stats.focusUsageStats[0];
  }, [stats]);

  return (
    <AppLayout
      title="Statisztikák"
      subtitle="Kövesd nyomon az edzéseid mennyiségét és fókuszterületeit."
    >
      <section className="card statistics-filter-card">
        <div className="form-field">
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
          <section className="mobile-stat-summary-grid">
            <article className="mobile-stat-card">
              <span>Heti edzés</span>
              <strong>{stats.weeklyCount}</strong>
            </article>

            <article className="mobile-stat-card">
              <span>Havi edzés</span>
              <strong>{stats.monthlyCount}</strong>
            </article>

            <article className="mobile-stat-card">
              <span>Összes idő</span>
              <strong>{totalHours} óra</strong>
            </article>

            <article className="mobile-stat-card">
              <span>Legfőbb fókusz</span>
              <strong>{topFocus ? topFocus.focusName : "Nincs adat"}</strong>
            </article>
          </section>

          <section className="card" style={{ marginBottom: "1.5rem" }}>
            <h2 style={{ marginTop: 0 }}>Heti edzéseloszlás</h2>

            <div className="weekly-chart">
              {stats.weeklyDailyStats.map((item) => {
                const height = `${(item.count / maxDailyCount) * 150}px`;

                return (
                  <div key={item.dayLabel} className="weekly-chart-item">
                    <span className="weekly-chart-count">
                      {item.count} edz.
                    </span>

                    <div
                      className="weekly-chart-bar"
                      style={{
                        height,
                        minHeight: item.count > 0 ? "18px" : "5px",
                      }}
                    />

                    <strong className="weekly-chart-day">
                      {item.dayLabel}
                    </strong>

                    <span className="weekly-chart-minutes">
                      {item.durationMinutes} p
                    </span>
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
              <div className="stat-list">
                {stats.topPlans.map((plan, index) => (
                  <article key={plan.trainingPlanId} className="stat-list-item">
                    <div className="stat-rank">{index + 1}</div>
                    <div>
                      <h3>{plan.title}</h3>
                      <p>Felhasználás: {plan.count} alkalom</p>
                    </div>
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
              <div className="stat-list">
                {stats.focusUsageStats.map((focus, index) => (
                  <article key={focus.focusName} className="stat-list-item">
                    <div className="stat-rank">{index + 1}</div>
                    <div>
                      <h3>{focus.focusName}</h3>
                      <p>Előfordulás: {focus.count}</p>
                    </div>
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