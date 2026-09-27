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
        setRoles(roleData);
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni a profiladatokat."
          );
        } else {
          setError("Nem sikerült betölteni a profiladatokat.");
        }
      } finally {
        setLoading(false);
      }
    };

    void loadProfile();
  }, []);

  const togglePlayer = () => {
    setSuccessMessage("");
    setError("");

    setRoles((current) => ({
      ...current,
      isPlayer: !current.isPlayer,
    }));
  };

  const toggleOrganizerCoach = () => {
    setSuccessMessage("");
    setError("");

    setRoles((current) => ({
      ...current,
      isOrganizerCoach: !current.isOrganizerCoach,
    }));
  };

  const handleSave = async () => {
    if (!roles.isPlayer && !roles.isOrganizerCoach) {
      setError(
        "Legalább egy sportbeli szerepkört ki kell választani."
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
        "A sportbeli szerepkörök sikeresen mentve."
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setError(
          err.response?.data?.message ??
            "Nem sikerült menteni a szerepköröket."
        );
      } else {
        setError("Nem sikerült menteni a szerepköröket.");
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
        subtitle="Felhasználói adatok és sportbeli szerepkörök."
      >
        <div className="card">
          <p className="info-text">Profil betöltése...</p>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title="Profil"
      subtitle="Felhasználói adatok és sportbeli szerepkörök kezelése."
    >
      <div className="profile-grid">
        <section className="card profile-info-card">
          <div className="profile-avatar">
            {profile?.name?.charAt(0).toUpperCase() ?? "U"}
          </div>

          <div>
            <p className="profile-label">Név</p>
            <h2 className="profile-name">
              {profile?.name ?? "Nincs megadva"}
            </h2>

            <p className="profile-label">Email</p>
            <p className="profile-value">
              {profile?.email ?? "Nincs megadva"}
            </p>
          </div>
        </section>

        <section className="card profile-role-card">
          <div className="profile-section-heading">
            <h2>Sportbeli szerepkörök</h2>

            <p>
              Válaszd ki, milyen szerepekben szeretnéd használni
              a VolleyMind alkalmazást.
            </p>
          </div>

          <div className="profile-role-options">
            <button
              type="button"
              className={`profile-role-option ${
                roles.isPlayer ? "active" : ""
              }`}
              onClick={togglePlayer}
            >
              <div className="profile-role-checkbox">
                {roles.isPlayer ? "✓" : ""}
              </div>

              <div>
                <strong>Játékos</strong>

                <span>
                  Edzések böngészése, jelentkezés és versenyeken
                  való részvétel.
                </span>
              </div>
            </button>

            <button
              type="button"
              className={`profile-role-option ${
                roles.isOrganizerCoach ? "active" : ""
              }`}
              onClick={toggleOrganizerCoach}
            >
              <div className="profile-role-checkbox">
                {roles.isOrganizerCoach ? "✓" : ""}
              </div>

              <div>
                <strong>Edző / Szervező</strong>

                <span>
                  Edzések és versenyek létrehozása és kezelése.
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
                : "Szerepkörök mentése"}
            </button>
          </div>
        </section>

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

        <section className="card profile-account-card">
          <h2>Fiók</h2>

          <p className="info-text">
            Itt tudsz kijelentkezni a VolleyMind alkalmazásból.
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