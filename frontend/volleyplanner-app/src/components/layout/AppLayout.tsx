import {
  useEffect,
  useState,
  type ReactNode,
} from "react";

import {
  NavLink,
  useLocation,
} from "react-router-dom";

import "../../styles/app.css";

interface AppLayoutProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
}

function AppLayout({
  title,
  subtitle,
  children,
}: AppLayoutProps) {
  const location = useLocation();

  const [
    isMoreMenuOpen,
    setIsMoreMenuOpen,
  ] = useState(false);

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userEmail");

    window.location.href = "/";
  };

  const closeMoreMenu = () => {
    setIsMoreMenuOpen(false);
  };

  useEffect(() => {
    const handleKeyDown = (
      event: KeyboardEvent
    ) => {
      if (event.key === "Escape") {
        setIsMoreMenuOpen(false);
      }
    };

    document.addEventListener(
      "keydown",
      handleKeyDown
    );

    return () => {
      document.removeEventListener(
        "keydown",
        handleKeyDown
      );
    };
  }, []);

  const isMoreSectionActive =
    location.pathname.startsWith("/plans") ||
    location.pathname.startsWith("/statistics") ||
    location.pathname.startsWith("/profile");

  return (
    <div className="app-shell">
      <nav className="app-navbar">
        <div className="app-navbar-inner">
          <NavLink
            to="/exercises"
            className="app-brand"
          >
            VolleyMind
          </NavLink>

          <div className="app-nav-links desktop-nav">
            <NavLink
              to="/exercises"
              className="app-nav-link"
            >
              Gyakorlatok
            </NavLink>

            <NavLink
              to="/trainings"
              className="app-nav-link"
            >
              Edzések
            </NavLink>

            <NavLink
              to="/generate"
              className="app-nav-link"
            >
              Generálás
            </NavLink>

            <NavLink
              to="/plans"
              className="app-nav-link"
            >
              Tervek
            </NavLink>

            <NavLink
              to="/planner"
              className="app-nav-link"
            >
              Tervező
            </NavLink>

            <NavLink
              to="/statistics"
              className="app-nav-link"
            >
              Statisztikák
            </NavLink>

            <NavLink
              to="/profile"
              className="app-nav-link"
            >
              Profil
            </NavLink>

            <button
              type="button"
              className="danger-button nav-logout"
              onClick={handleLogout}
            >
              Kilépés
            </button>
          </div>
        </div>
      </nav>

      <main className="app-page">
        <div className="page-header">
          <div>
            <h1 className="page-title">
              {title}
            </h1>

            {subtitle && (
              <p className="page-subtitle">
                {subtitle}
              </p>
            )}
          </div>
        </div>

        {children}
      </main>

      {isMoreMenuOpen && (
        <div
          className="mobile-more-overlay"
          onClick={closeMoreMenu}
        >
          <section
            className="mobile-more-sheet"
            role="dialog"
            aria-modal="true"
            aria-label="További menüpontok"
            onClick={(event) =>
              event.stopPropagation()
            }
          >
            <div className="mobile-more-handle" />

            <div className="mobile-more-header">
              <div>
                <p>VOLLEYMIND</p>
                <h2>Továbbiak</h2>
              </div>

              <button
                type="button"
                className="mobile-more-close"
                onClick={closeMoreMenu}
                aria-label="Menü bezárása"
              >
                ×
              </button>
            </div>

            <div className="mobile-more-menu">
              <NavLink
                to="/plans"
                className="mobile-more-item"
                onClick={closeMoreMenu}
              >
                <span className="mobile-more-icon">
                  📋
                </span>

                <div>
                  <strong>
                    Mentett tervek
                  </strong>

                  <small>
                    Korábban mentett edzésterveid
                  </small>
                </div>

                <span className="mobile-more-arrow">
                  ›
                </span>
              </NavLink>

              <NavLink
                to="/trainings/mine"
                className="mobile-more-item"
                onClick={closeMoreMenu}
              >
                <span className="mobile-more-icon">
                  🗓️
                </span>

                <div>
                  <strong>
                    Saját edzéseim
                  </strong>

                  <small>
                    Az általad meghirdetett edzések
                  </small>
                </div>

                <span className="mobile-more-arrow">
                  ›
                </span>
              </NavLink>

              <NavLink
                to="/statistics"
                className="mobile-more-item"
                onClick={closeMoreMenu}
              >
                <span className="mobile-more-icon">
                  📊
                </span>

                <div>
                  <strong>
                    Statisztikák
                  </strong>

                  <small>
                    Edzésadataid és aktivitásod
                  </small>
                </div>

                <span className="mobile-more-arrow">
                  ›
                </span>
              </NavLink>

              <NavLink
                to="/profile"
                className="mobile-more-item"
                onClick={closeMoreMenu}
              >
                <span className="mobile-more-icon">
                  👤
                </span>

                <div>
                  <strong>
                    Profil
                  </strong>

                  <small>
                    Fiók és sportbeli szerepkör
                  </small>
                </div>

                <span className="mobile-more-arrow">
                  ›
                </span>
              </NavLink>

              <div className="mobile-more-divider" />

              <button
                type="button"
                className="mobile-more-item mobile-more-item-disabled"
                disabled
              >
                <span className="mobile-more-icon">
                  🏆
                </span>

                <div>
                  <strong>
                    Versenyek
                  </strong>

                  <small>
                    Versenyszervezés és nevezések
                  </small>
                </div>

                <span className="mobile-coming-soon">
                  Hamarosan
                </span>
              </button>

              <button
                type="button"
                className="mobile-more-item mobile-more-item-disabled"
                disabled
              >
                <span className="mobile-more-icon">
                  ⭐
                </span>

                <div>
                  <strong>
                    Premium
                  </strong>

                  <small>
                    Premium funkciók és előfizetés
                  </small>
                </div>

                <span className="mobile-coming-soon">
                  Hamarosan
                </span>
              </button>
            </div>

            <button
              type="button"
              className="mobile-more-logout"
              onClick={handleLogout}
            >
              <span>↪</span>
              Kijelentkezés
            </button>
          </section>
        </div>
      )}

      <nav className="mobile-bottom-nav">
        <NavLink
          to="/exercises"
          className="mobile-bottom-link"
        >
          <span>🏐</span>
          <small>Gyakorlatok</small>
        </NavLink>

        <NavLink
          to="/generate"
          className="mobile-bottom-link"
        >
          <span>✨</span>
          <small>Generálás</small>
        </NavLink>

        <NavLink
          to="/planner"
          className="mobile-bottom-link"
        >
          <span>📅</span>
          <small>Tervező</small>
        </NavLink>

        <NavLink
          to="/trainings"
          className="mobile-bottom-link"
        >
          <span>👥</span>
          <small>Edzések</small>
        </NavLink>

        <button
          type="button"
          className={`mobile-bottom-link mobile-more-trigger${
            isMoreMenuOpen || isMoreSectionActive
              ? " active"
              : ""
          }`}
          onClick={() =>
            setIsMoreMenuOpen(
              (previous) => !previous
            )
          }
          aria-expanded={isMoreMenuOpen}
          aria-label="További menüpontok"
        >
          <span>☰</span>
          <small>Továbbiak</small>
        </button>
      </nav>
    </div>
  );
}

export default AppLayout;