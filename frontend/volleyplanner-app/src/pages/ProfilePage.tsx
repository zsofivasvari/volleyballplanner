import { useEffect, useState } from "react";
import axios from "axios";
import AppLayout from "../components/layout/AppLayout";
import { authService } from "../services/authService";
import { sportRoleService } from "../services/sportRoleService";
import {
  teamInvitationService,
} from "../services/teamInvitationService";
import type {
  UpdateUserProfileRequest,
  UserProfileResponse,
} from "../types/auth";
import type { SportRolesResponse } from "../types/sportRole";
import type {
  TeamInvitation,
} from "../types/tournament";

function ProfilePage() {
  const [profile, setProfile] =
    useState<UserProfileResponse | null>(null);

  const [roles, setRoles] = useState<SportRolesResponse>({
    isPlayer: false,
    isOrganizerCoach: false,
  });

  const [playerProfileForm, setPlayerProfileForm] =
    useState<UpdateUserProfileRequest>({
      level: "",
      goal: "",
      age: null,
      height: null,
      weight: null,
    });

  const [loading, setLoading] = useState(true);
  const [savingRole, setSavingRole] = useState(false);
  const [savingProfile, setSavingProfile] = useState(false);

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

    const [
      receivedInvitations,
      setReceivedInvitations,
    ] = useState<TeamInvitation[]>([]);

    const [
      respondingInvitationId,
      setRespondingInvitationId,
    ] = useState<number | null>(null);

  useEffect(() => {
    const loadProfile = async () => {
      try {
        setLoading(true);
        setError("");

        const [
          profileData,
          roleData,
          invitationData,
        ] = await Promise.all([
          authService.getMe(),
          sportRoleService.getMySportRoles(),
          teamInvitationService.getReceived(),
        ]);

        setProfile(profileData);

        setReceivedInvitations(
          invitationData
        );

        setPlayerProfileForm({
          level: profileData.level ?? "",
          goal: profileData.goal ?? "",
          age: profileData.age ?? null,
          height: profileData.height ?? null,
          weight: profileData.weight ?? null,
        });

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

  const pendingInvitations =
    receivedInvitations.filter(
      invitation =>
        invitation.status === "Pending"
    );

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

  const handleRoleSave = async () => {
    if (roles.isPlayer === roles.isOrganizerCoach) {
      setError(
        "Pontosan egy sportbeli szerepkört kell kiválasztani."
      );
      setSuccessMessage("");
      return;
    }

    try {
      setSavingRole(true);
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
      setSavingRole(false);
    }
  };

  const handlePlayerProfileChange = (
    field: keyof UpdateUserProfileRequest,
    value: string
  ) => {
    setSuccessMessage("");
    setError("");

    if (
      field === "age" ||
      field === "height" ||
      field === "weight"
    ) {
      setPlayerProfileForm((previous) => ({
        ...previous,
        [field]:
          value.trim() === ""
            ? null
            : Number(value),
      }));

      return;
    }

    setPlayerProfileForm((previous) => ({
      ...previous,
      [field]: value,
    }));
  };

  const validatePlayerProfile = () => {
    if (
      playerProfileForm.age != null &&
      (
        playerProfileForm.age < 10 ||
        playerProfileForm.age > 100
      )
    ) {
      setError(
        "Az életkor 10 és 100 év között lehet."
      );
      return false;
    }

    if (
      playerProfileForm.height != null &&
      (
        playerProfileForm.height < 100 ||
        playerProfileForm.height > 250
      )
    ) {
      setError(
        "A magasság 100 és 250 cm között lehet."
      );
      return false;
    }

    if (
      playerProfileForm.weight != null &&
      (
        playerProfileForm.weight < 30 ||
        playerProfileForm.weight > 250
      )
    ) {
      setError(
        "A testsúly 30 és 250 kg között lehet."
      );
      return false;
    }

    return true;
  };

  const handlePlayerProfileSave = async () => {
    if (!validatePlayerProfile()) {
      setSuccessMessage("");
      return;
    }

    try {
      setSavingProfile(true);
      setError("");
      setSuccessMessage("");

      const updatedProfile =
        await authService.updateMyProfile({
          level:
            playerProfileForm.level?.trim() || null,
          goal:
            playerProfileForm.goal?.trim() || null,
          age: playerProfileForm.age ?? null,
          height: playerProfileForm.height ?? null,
          weight: playerProfileForm.weight ?? null,
        });

      setProfile(updatedProfile);

      setPlayerProfileForm({
        level: updatedProfile.level ?? "",
        goal: updatedProfile.goal ?? "",
        age: updatedProfile.age ?? null,
        height: updatedProfile.height ?? null,
        weight: updatedProfile.weight ?? null,
      });

      setSuccessMessage(
        "A játékosprofil sikeresen mentve."
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setError(
          err.response?.data?.message ??
            "Nem sikerült menteni a játékosprofilt."
        );
      } else {
        setError(
          "Nem sikerült menteni a játékosprofilt."
        );
      }
    } finally {
      setSavingProfile(false);
    }
  };

  const handleCopyPlayerCode = async () => {
    if (!profile?.playerCode) {
      return;
    }

    try {
      await navigator.clipboard.writeText(
        profile.playerCode
      );

      setError("");

      setSuccessMessage(
        "A játékoskód a vágólapra másolva."
      );
    } catch (err) {
      console.error(err);

      setSuccessMessage("");

      setError(
        "A játékoskód másolása nem sikerült."
      );
    }
  };

  const handleAcceptInvitation =
    async (invitationId: number) => {
      try {
        setRespondingInvitationId(
          invitationId
        );

        setError("");
        setSuccessMessage("");

        const entry =
          await teamInvitationService.accept(
            invitationId
          );

        setReceivedInvitations(current =>
          current.map(invitation =>
            invitation.id === invitationId
              ? {
                  ...invitation,
                  status: "Accepted",
                  respondedAt:
                    new Date().toISOString(),
                }
              : invitation
          )
        );

        setSuccessMessage(
          `Sikeresen beneveztetek a(z) ${entry.tournamentName} versenyre ${entry.teamName} csapatként.`
        );
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült elfogadni a meghívást."
          );
        } else {
          setError(
            "Nem sikerült elfogadni a meghívást."
          );
        }
      } finally {
        setRespondingInvitationId(null);
      }
    };

  const handleRejectInvitation =
    async (invitationId: number) => {
      try {
        setRespondingInvitationId(
          invitationId
        );

        setError("");
        setSuccessMessage("");

        await teamInvitationService.reject(
          invitationId
        );

        setReceivedInvitations(current =>
          current.map(invitation =>
            invitation.id === invitationId
              ? {
                  ...invitation,
                  status: "Rejected",
                  respondedAt:
                    new Date().toISOString(),
                }
              : invitation
          )
        );

        setSuccessMessage(
          "A meghívást elutasítottad."
        );
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült elutasítani a meghívást."
          );
        } else {
          setError(
            "Nem sikerült elutasítani a meghívást."
          );
        }
      } finally {
        setRespondingInvitationId(null);
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

            {roles.isPlayer && (
              <>
                <p className="profile-label">
                  Játékoskód
                </p>

                <div className="profile-player-code-row">
                  <strong className="profile-player-code">
                    {profile?.playerCode ??
                      "Nincs játékoskód"}
                  </strong>

                  {profile?.playerCode && (
                    <button
                      type="button"
                      className="secondary-button profile-copy-code-button"
                      onClick={() =>
                        void handleCopyPlayerCode()
                      }
                    >
                      Másolás
                    </button>
                  )}
                </div>

                <p className="profile-player-code-help">
                  Ezt a kódot add meg a
                  csapattársadnak, ha együtt
                  szeretnétek nevezni egy
                  versenyre.
                </p>
              </>
            )}
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

          <div className="profile-actions">
            <button
              type="button"
              className="primary-button"
              onClick={handleRoleSave}
              disabled={savingRole}
            >
              {savingRole
                ? "Mentés..."
                : "Szerepkör mentése"}
            </button>
          </div>
        </section>

        {roles.isPlayer && (
          <section className="card profile-details-card">
            <div className="profile-section-heading">
              <h2>Játékosprofil</h2>

              <p>
                Add meg a sportolói adataidat. Ezek később
                az edzések és versenyek kezelésénél is
                használhatók lesznek.
              </p>
            </div>

            <div className="profile-edit-grid">
              <div className="profile-field">
                <label htmlFor="profile-level">
                  Szint
                </label>

                <select
                  id="profile-level"
                  value={playerProfileForm.level ?? ""}
                  onChange={(event) =>
                    handlePlayerProfileChange(
                      "level",
                      event.target.value
                    )
                  }
                >
                  <option value="">
                    Válassz szintet
                  </option>

                  <option value="Kezdő">
                    Kezdő
                  </option>

                  <option value="Középhaladó">
                    Középhaladó
                  </option>

                  <option value="Haladó">
                    Haladó
                  </option>

                  <option value="Versenyző">
                    Versenyző
                  </option>
                </select>
              </div>

              <div className="profile-field">
                <label htmlFor="profile-goal">
                  Cél
                </label>

                <input
                  id="profile-goal"
                  type="text"
                  value={playerProfileForm.goal ?? ""}
                  onChange={(event) =>
                    handlePlayerProfileChange(
                      "goal",
                      event.target.value
                    )
                  }
                  placeholder="Pl. fejlődés, versenyzés"
                  maxLength={100}
                />
              </div>

              <div className="profile-field">
                <label htmlFor="profile-age">
                  Életkor
                </label>

                <div className="profile-input-with-unit">
                  <input
                    id="profile-age"
                    type="number"
                    min={10}
                    max={100}
                    value={playerProfileForm.age ?? ""}
                    onChange={(event) =>
                      handlePlayerProfileChange(
                        "age",
                        event.target.value
                      )
                    }
                    placeholder="Pl. 25"
                  />

                  <span>év</span>
                </div>
              </div>

              <div className="profile-field">
                <label htmlFor="profile-height">
                  Magasság
                </label>

                <div className="profile-input-with-unit">
                  <input
                    id="profile-height"
                    type="number"
                    min={100}
                    max={250}
                    step="0.1"
                    value={playerProfileForm.height ?? ""}
                    onChange={(event) =>
                      handlePlayerProfileChange(
                        "height",
                        event.target.value
                      )
                    }
                    placeholder="Pl. 178"
                  />

                  <span>cm</span>
                </div>
              </div>

              <div className="profile-field">
                <label htmlFor="profile-weight">
                  Testsúly
                </label>

                <div className="profile-input-with-unit">
                  <input
                    id="profile-weight"
                    type="number"
                    min={30}
                    max={250}
                    step="0.1"
                    value={playerProfileForm.weight ?? ""}
                    onChange={(event) =>
                      handlePlayerProfileChange(
                        "weight",
                        event.target.value
                      )
                    }
                    placeholder="Pl. 70"
                  />

                  <span>kg</span>
                </div>
              </div>
            </div>

            <div className="profile-actions">
              <button
                type="button"
                className="primary-button profile-save-button"
                onClick={handlePlayerProfileSave}
                disabled={savingProfile}
              >
                {savingProfile
                  ? "Mentés..."
                  : "Játékosprofil mentése"}
              </button>
            </div>
          </section>
        )}

        {roles.isPlayer && (
          <section className="card profile-invitations-card">
            <div className="profile-section-heading">
              <h2>
                Versenymeghívások
              </h2>

              <p>
                Itt láthatod a csapattársnak
                érkezett versenymeghívásaidat.
              </p>
            </div>

            {pendingInvitations.length === 0 ? (
              <p className="info-text">
                Nincs függőben lévő
                versenymeghívásod.
              </p>
            ) : (
              <div className="profile-invitation-list">
                {pendingInvitations.map(
                  invitation => (
                    <article
                      key={invitation.id}
                      className="profile-invitation-item"
                    >
                      <div className="profile-invitation-info">
                        <span>
                          VERSENY
                        </span>

                        <h3>
                          {
                            invitation.tournamentName
                          }
                        </h3>

                        <p>
                          Meghívó:
                          {" "}
                          <strong>
                            {
                              invitation.inviterName
                            }
                          </strong>
                        </p>

                        <p>
                          Játékoskód:
                          {" "}
                          <strong>
                            {
                              invitation.inviterPlayerCode
                            }
                          </strong>
                        </p>
                      </div>

                      <div className="profile-invitation-actions">
                        <button
                          type="button"
                          className="primary-button"
                          onClick={() =>
                            void handleAcceptInvitation(
                              invitation.id
                            )
                          }
                          disabled={
                            respondingInvitationId !==
                            null
                          }
                        >
                          {respondingInvitationId ===
                          invitation.id
                            ? "Feldolgozás..."
                            : "Elfogadás"}
                        </button>

                        <button
                          type="button"
                          className="secondary-button"
                          onClick={() =>
                            void handleRejectInvitation(
                              invitation.id
                            )
                          }
                          disabled={
                            respondingInvitationId !==
                            null
                          }
                        >
                          Elutasítás
                        </button>
                      </div>
                    </article>
                  )
                )}
              </div>
            )}
          </section>
        )}

        {error && (
          <section className="card">
            <p className="error-text">
              {error}
            </p>
          </section>
        )}

        {successMessage && (
          <section className="card">
            <p className="success-text">
              {successMessage}
            </p>
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