import type { ReactNode } from "react";
import { NavLink } from "react-router-dom";
import "../../styles/app.css";

interface AppLayoutProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
}

function AppLayout({ title, subtitle, children }: AppLayoutProps) {
  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userEmail");
    window.location.href = "/";
  };

  return (
    <div className="app-shell">
      <nav className="app-navbar">
        <div className="app-navbar-inner">
          <NavLink to="/exercises" className="app-brand">
            VolleyMind
          </NavLink>

          <div className="app-nav-links desktop-nav">
            <NavLink to="/exercises" className="app-nav-link">
              Gyakorlatok
            </NavLink>
            <NavLink to="/generate" className="app-nav-link">
              Generálás
            </NavLink>
            <NavLink to="/plans" className="app-nav-link">
              Tervek
            </NavLink>
            <NavLink to="/planner" className="app-nav-link">
              Tervező
            </NavLink>
            <NavLink to="/statistics" className="app-nav-link">
              Statisztikák
            </NavLink>
            <button className="danger-button nav-logout" onClick={handleLogout}>
              Kilépés
            </button>
          </div>
        </div>
      </nav>

      <main className="app-page">
        <div className="page-header">
          <div>
            <h1 className="page-title">{title}</h1>
            {subtitle && <p className="page-subtitle">{subtitle}</p>}
          </div>
        </div>

        {children}
      </main>

      <nav className="mobile-bottom-nav">
        <NavLink to="/exercises" className="mobile-bottom-link">
          <span>🏐</span>
          <small>Gyakorlatok</small>
        </NavLink>

        <NavLink to="/generate" className="mobile-bottom-link">
          <span>✨</span>
          <small>Generálás</small>
        </NavLink>

        <NavLink to="/planner" className="mobile-bottom-link">
          <span>📅</span>
          <small>Tervező</small>
        </NavLink>

        <NavLink to="/statistics" className="mobile-bottom-link">
          <span>📊</span>
          <small>Statisztikák</small>
        </NavLink>

        <button className="mobile-bottom-link mobile-logout" onClick={handleLogout}>
          <span>↪</span>
          <small>Kilépés</small>
        </button>
      </nav>
    </div>
  );
}

export default AppLayout;