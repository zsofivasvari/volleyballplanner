import type { ReactNode } from "react";
import { Link } from "react-router-dom";
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
        <Link to="/exercises" className="app-brand">
          VolleyMind
        </Link>

        <div className="app-nav-links">
          <Link to="/exercises" className="app-nav-link">
            Gyakorlatok
          </Link>
          <Link to="/generate" className="app-nav-link">
            Generálás
          </Link>
          <Link to="/plans" className="app-nav-link">
            Mentett tervek
          </Link>
          <Link to="/planner" className="app-nav-link">
            Heti tervező
          </Link>
          <Link to="/statistics" className="app-nav-link">
            Statisztikák
          </Link>
          <button className="danger-button" onClick={handleLogout}>
            Kijelentkezés
          </button>
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
    </div>
  );
}

export default AppLayout;