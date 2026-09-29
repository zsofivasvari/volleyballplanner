import { useEffect, useState } from "react";
import axios from "axios";
import AppLayout from "../components/layout/AppLayout";
import { authService } from "../services/authService";
import { sportRoleService } from "../services/sportRoleService";
import type { UserProfileResponse } from "../types/auth";
import type { SportRolesResponse } from "../types/sportRole";

function ProfilePage() {
  const [profile, setProfile] =
    useState<UserProfileResponse | null>(null);

  const [roles, setRoles] = useState<SportRolesResponse>({
    isPlayer: false,
    isOrganizerCoach: false,
  });

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  useEffect(() => {
    const loadProfile = async () => {
      try {
        setLoading(true);
        setError("");

        const [profileData, roleData] = await Promise.all([
          authService.getMe(),
          sportRoleService.getMySportRoles(),
        ]);

        setProfile(profileData);

        // Ha régi adat miatt még mindkét szerep igaz lenne,
        // a frontend nem tartja meg mindkettőt.
        if (roleData.isOrganizerCoach) {
          setRoles({
            isPlayer: false,
            isOrganizerCoach: true,
          });
        } else if (roleData.isPlayer) {
          setRoles({
            isPlayer: true,
            isOrganizerCoach: false,
          });
        } else {
          setRoles({
            isPlayer: false,
            isOrganizerCoach: false,
          });
        }
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni a profiladatokat."
          );
        } else {
          setError(
            "Nem sikerült betölteni a profiladatokat."
          );
        }
      } finally {
        setLoading(false);
      }
    };

    void loadProfile();
  }, []);

  const selectPlayer = () => {
    setSuccessMessage("");
    setError("");

    setRoles({
      isPlayer: true,
      isOrganizerCoach: false,
    });
  };

  const selectOrganizerCoach = () => {
    setSuccessMessage("");
    setError("");

    setRoles({
      isPlayer: false,
      isOrganizerCoach: true,
    });
  };

  const handleSave = async () => {
    if (roles.isPlayer === roles.isOrganizerCoach) {
      setError(
        "Pontosan egy sportbeli szerepkört kell kiválasztani."
      );
      setSuccessMessage("");
      return;
    }

    try {
      setSaving(true);
      setError("");
      setSuccessMessage("");

      const updatedRoles =
        await sportRoleService.updateMySportRoles(roles);

      setRoles(updatedRoles);

      setSuccessMessage(
        "A sportbeli szerepkör sikeresen mentve."
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setError(
          err.response?.data?.message ??
            "Nem sikerült menteni a szerepkört."
        );
      } else {
        setError(
          "Nem sikerült menteni a szerepkört."
        );
      }
    } finally {
      setSaving(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userEmail");

    window.location.href = "/";
  };

  if (loading) {
    return (
      <AppLayout
        title="Profil"
        subtitle="Felhasználói adatok és sportbeli szerepkör."
      >
        <div className="card">
          <p className="info-text">
            Profil betöltése...
          </p>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title="Profil"
      subtitle="Felhasználói adatok és sportbeli szerepkör kezelése."
    >
      <div className="profile-grid">
        <section className="card profile-info-card">
          <div className="profile-avatar">
            {profile?.name?.charAt(0).toUpperCase() ?? "U"}
          </div>

          <div>
            <p className="profile-label">
              Név
            </p>

            <h2 className="profile-name">
              {profile?.name ?? "Nincs megadva"}
            </h2>

            <p className="profile-label">
              Email
            </p>

            <p className="profile-value">
              {profile?.email ?? "Nincs megadva"}
            </p>
          </div>
        </section>

        <section className="card profile-role-card">
          <div className="profile-section-heading">
            <h2>Sportbeli szerepkör</h2>

            <p>
              Válaszd ki, milyen szerepkörben szeretnéd
              használni a VolleyMind alkalmazást. Egyszerre
              egy szerepkör lehet aktív.
            </p>
          </div>

          <div className="profile-role-options">
            <button
              type="button"
              className={`profile-role-option ${
                roles.isPlayer ? "active" : ""
              }`}
              onClick={selectPlayer}
            >
              <div className="profile-role-checkbox">
                {roles.isPlayer ? "✓" : ""}
              </div>

              <div>
                <strong>Játékos</strong>

                <span>
                  Edzések böngészése, jelentkezés és
                  versenyeken való részvétel.
                </span>
              </div>
            </button>

            <button
              type="button"
              className={`profile-role-option ${
                roles.isOrganizerCoach ? "active" : ""
              }`}
              onClick={selectOrganizerCoach}
            >
              <div className="profile-role-checkbox">
                {roles.isOrganizerCoach ? "✓" : ""}
              </div>

              <div>
                <strong>Edző / Szervező</strong>

                <span>
                  Edzések és versenyek létrehozása és
                  kezelése.
                </span>
              </div>
            </button>
          </div>

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

          <div className="profile-actions">
            <button
              type="button"
              className="primary-button"
              onClick={handleSave}
              disabled={saving}
            >
              {saving
                ? "Mentés..."
                : "Szerepkör mentése"}
            </button>
          </div>
        </section>

        {roles.isPlayer && (
          <section className="card profile-details-card">
            <h2>Játékosprofil</h2>

            <div className="profile-detail-grid">
              <div>
                <span>Szint</span>

                <strong>
                  {profile?.level ?? "Nincs megadva"}
                </strong>
              </div>

              <div>
                <span>Cél</span>

                <strong>
                  {profile?.goal ?? "Nincs megadva"}
                </strong>
              </div>

              <div>
                <span>Életkor</span>

                <strong>
                  {profile?.age != null
                    ? `${profile.age} év`
                    : "Nincs megadva"}
                </strong>
              </div>

              <div>
                <span>Magasság</span>

                <strong>
                  {profile?.height != null
                    ? `${profile.height} cm`
                    : "Nincs megadva"}
                </strong>
              </div>

              <div>
                <span>Testsúly</span>

                <strong>
                  {profile?.weight != null
                    ? `${profile.weight} kg`
                    : "Nincs megadva"}
                </strong>
              </div>
            </div>
          </section>
        )}

        <section className="card profile-account-card">
          <h2>Fiók</h2>

          <p className="info-text">
            Itt tudsz kijelentkezni a VolleyMind
            alkalmazásból.
          </p>

          <button
            type="button"
            className="danger-button"
            onClick={handleLogout}
          >
            Kijelentkezés
          </button>
        </section>
      </div>
    </AppLayout>
  );
}

export default ProfilePage;