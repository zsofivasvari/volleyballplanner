import { useEffect, useState } from "react";
import axios from "axios";
import AppLayout from "../components/layout/AppLayout";
import { authService } from "../services/authService";
import { sportRoleService } from "../services/sportRoleService";
import { subscriptionService } from "../services/subscriptionService";
import {
  teamInvitationService,
} from "../services/teamInvitationService";
import type {
  UpdateUserProfileRequest,
  UserProfileResponse,
} from "../types/auth";
import type { SportRolesResponse } from "../types/sportRole";
import type { SubscriptionStatus } from "../types/subscription";
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

  const [roleError, setRoleError] = useState("");
  const [roleSuccessMessage, setRoleSuccessMessage] = useState("");

  const [subscriptionStatus, setSubscriptionStatus] =
    useState<SubscriptionStatus | null>(null);

  const [subscriptionLoading, setSubscriptionLoading] =
    useState(false);

  const [subscriptionError, setSubscriptionError] =
    useState("");

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

        const searchParams =
          new URLSearchParams(
            window.location.search
          );

        const subscriptionResult =
          searchParams.get("subscription");

        const checkoutSessionId =
          searchParams.get("session_id");

        const [
          profileData,
          roleData,
          invitationData,
        ] = await Promise.all([
          authService.getMe(),
          sportRoleService.getMySportRoles(),
          teamInvitationService.getReceived(),
        ]);

        let subscriptionData: SubscriptionStatus;

        if (
          subscriptionResult === "success" &&
          checkoutSessionId
        ) {
          subscriptionData =
            await subscriptionService.syncSubscription(
              checkoutSessionId
            );

          window.history.replaceState(
            {},
            "",
            "/profile"
          );
        } else {
          try {
            subscriptionData =
              await subscriptionService
                .syncSubscription();
          } catch (syncError) {
            console.error(
              "Stripe subscription sync failed:",
              syncError
            );

            subscriptionData =
              await subscriptionService
                .getMyStatus();
          }
        }

        setProfile(profileData);

        setReceivedInvitations(
          invitationData
        );

        setSubscriptionStatus(
          subscriptionData
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
    setRoleSuccessMessage("");
    setRoleError("");

    setRoles({
      isPlayer: true,
      isOrganizerCoach: false,
    });
  };

  const selectOrganizerCoach = () => {
    setRoleSuccessMessage("");
    setRoleError("");

    setRoles({
      isPlayer: false,
      isOrganizerCoach: true,
    });
  };

  const handleRoleSave = async () => {
    if (roles.isPlayer === roles.isOrganizerCoach) {
      setRoleError(
        "Pontosan egy sportbeli szerepkört kell kiválasztani."
      );
      setRoleSuccessMessage("");
      return;
    }

    try {
      setSavingRole(true);
      setRoleError("");
      setRoleSuccessMessage("");

      const updatedRoles =
        await sportRoleService.updateMySportRoles(roles);

      setRoles(updatedRoles);

      setRoleSuccessMessage(
        "A sportbeli szerepkör sikeresen mentve."
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setRoleError(
          err.response?.data?.message ??
            "Nem sikerült menteni a szerepkört."
        );
      } else {
        setRoleError(
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

  const handleStartSubscription = async () => {
    try {
      setSubscriptionLoading(true);
      setSubscriptionError("");

      const result =
        await subscriptionService.createCheckoutSession();

      if (!result.checkoutUrl) {
        setSubscriptionError(
          "Nem sikerült megnyitni a Stripe fizetési oldalt."
        );
        return;
      }

      window.location.href =
        result.checkoutUrl;
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setSubscriptionError(
          err.response?.data?.message ??
            "Nem sikerült elindítani az előfizetést."
        );
      } else {
        setSubscriptionError(
          "Nem sikerült elindítani az előfizetést."
        );
      }
    } finally {
      setSubscriptionLoading(false);
    }
  };

  const handleCancelSubscription = async () => {
    try {
      setSubscriptionLoading(true);
      setSubscriptionError("");

      await subscriptionService.cancelSubscription();

      const updatedStatus =
        await subscriptionService.getMyStatus();

      setSubscriptionStatus(updatedStatus);
      setSuccessMessage(
        "Az előfizetésed lemondása sikeres. A Premium hozzáférés a jelenlegi időszak végéig aktív marad."
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setSubscriptionError(
          err.response?.data?.message ??
            "Nem sikerült lemondani az előfizetést."
        );
      } else {
        setSubscriptionError(
          "Nem sikerült lemondani az előfizetést."
        );
      }
    } finally {
      setSubscriptionLoading(false);
    }
  };

  const handleReactivateSubscription = async () => {
    try {
      setSubscriptionLoading(true);
      setSubscriptionError("");

      await subscriptionService.reactivateSubscription();

      const updatedStatus =
        await subscriptionService.getMyStatus();

      setSubscriptionStatus(updatedStatus);

      setSuccessMessage(
        "Az előfizetés automatikus megújítása ismét aktív."
      );
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setSubscriptionError(
          err.response?.data?.message ??
            "Nem sikerült visszakapcsolni az automatikus megújítást."
        );
      } else {
        setSubscriptionError(
          "Nem sikerült visszakapcsolni az automatikus megújítást."
        );
      }
    } finally {
      setSubscriptionLoading(false);
    }
  };

  const handleOpenPortal = async () => {
    try {
      setSubscriptionLoading(true);
      setSubscriptionError("");

      const result =
        await subscriptionService.createPortalSession();

      if (!result.portalUrl) {
        setSubscriptionError(
          "Nem sikerült megnyitni az előfizetés-kezelő oldalt."
        );
        return;
      }

      window.location.href =
        result.portalUrl;
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err)) {
        setSubscriptionError(
          err.response?.data?.message ??
            "Nem sikerült megnyitni az előfizetés-kezelő oldalt."
        );
      } else {
        setSubscriptionError(
          "Nem sikerült megnyitni az előfizetés-kezelő oldalt."
        );
      }
    } finally {
      setSubscriptionLoading(false);
    }
  };

  const formatSubscriptionDate = (
    value: string | null
  ) => {
    if (!value) {
      return null;
    }

    return new Intl.DateTimeFormat(
      "hu-HU",
      {
        year: "numeric",
        month: "long",
        day: "numeric",
      }
    ).format(new Date(value));
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
      <div className="profile-column">
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
      </div>

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

            <div className="profile-role-feedback">
              {roleSuccessMessage && (
                <p className="success-text profile-role-message">
                  {roleSuccessMessage}
                </p>
              )}

              {roleError && (
                <p className="error-text profile-role-message">
                  {roleError}
                </p>
              )}
            </div>
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
          <section className="card profile-message-card">
            <p className="error-text">
              {error}
            </p>
          </section>
        )}

        {successMessage && (
          <section className="card profile-message-card">
            <p className="success-text">
              {successMessage}
            </p>
          </section>
        )}

        {subscriptionStatus && (
          <section className="card profile-subscription-card">
            <div className="profile-subscription-content">
              <div>
                <p className="profile-subscription-kicker">
                  Előfizetés
                </p>

                <h2>
                  {subscriptionStatus.isAdmin
                    ? "Admin"
                    : subscriptionStatus.isPremium
                      ? "VolleyMind Premium"
                      : subscriptionStatus.subscriptionStatus === "PastDue"
                        ? "VolleyMind Premium"
                        : "VolleyMind Free"}
                </h2>

                {subscriptionStatus.isAdmin ? (
                  <p className="info-text">
                    Adminisztrátorként korlátlan automatikus
                    edzésterv-generálás áll rendelkezésedre.
                  </p>
                ) : subscriptionStatus.isPremium ? (
                  <>
                    <p className="info-text">
                      Korlátlan automatikus edzésterv-generálás.
                    </p>

                    {subscriptionStatus.cancelAtPeriodEnd && (
                      <p className="profile-subscription-meta">
                        Az előfizetésed le van mondva, de a jelenlegi
                        időszak végéig aktív marad.
                      </p>
                    )}

                    {subscriptionStatus.currentPeriodEnd && (
                      <p className="profile-subscription-meta">
                        {subscriptionStatus.cancelAtPeriodEnd
                          ? "Az előfizetés eddig aktív:"
                          : "Következő megújulás:"}{" "}
                        <strong>
                          {formatSubscriptionDate(
                            subscriptionStatus.currentPeriodEnd
                          )}
                        </strong>
                      </p>
                    )}
                  </>
                ) : subscriptionStatus.subscriptionStatus === "PastDue" ? (
                  <>
                    <p className="error-text">
                      A Premium előfizetésed megújítása sikertelen volt.
                    </p>

                    <p className="profile-subscription-meta">
                      A Premium funkciók jelenleg nem érhetők el.
                      Ellenőrizd a fizetési módodat.
                    </p>

                    <button
                      type="button"
                      className="primary-button"
                      onClick={() =>
                        void handleOpenPortal()
                      }
                      disabled={subscriptionLoading}
                      style={{ marginTop: "1rem" }}
                    >
                      {subscriptionLoading
                        ? "Megnyitás..."
                        : "Fizetési mód kezelése"}
                    </button>
                  </>
                ) : (
                  <>
                    <p className="info-text">
                      5 ingyenes automatikus edzésterv-generálás,
                      utána Premium előfizetéssel korlátlan használat.
                    </p>

                    <p className="profile-subscription-meta">
                      Felhasznált generálások:{" "}
                      <strong>
                        {subscriptionStatus.freeGenerationsUsed} /{" "}
                        {subscriptionStatus.freeGenerationLimit}
                      </strong>
                    </p>

                    <p className="profile-subscription-meta">
                      Hátralévő ingyenes generálások:{" "}
                      <strong>
                        {subscriptionStatus.remainingFreeGenerations}
                      </strong>
                    </p>
                  </>
                )}

                {subscriptionError && (
                  <p className="error-text profile-subscription-error">
                    {subscriptionError}
                  </p>
                )}
              </div>

              {!subscriptionStatus.isAdmin &&
                !subscriptionStatus.isPremium &&
                subscriptionStatus.subscriptionStatus !== "PastDue" && (
                  <div className="profile-subscription-action">
                    <strong>1 999 Ft / hó</strong>

                    <button
                      type="button"
                      className="primary-button"
                      onClick={() =>
                        void handleStartSubscription()
                      }
                      disabled={subscriptionLoading}
                    >
                      {subscriptionLoading
                        ? "Átirányítás..."
                        : "Premium előfizetés"}
                    </button>
                  </div>
                )}

              {subscriptionStatus.isPremium && (
                <div className="profile-subscription-action">
                  <div className="profile-subscription-badge">
                    Premium
                  </div>

                  <button
                    type="button"
                    className="secondary-button"
                    onClick={() =>
                      void handleOpenPortal()
                    }
                    disabled={subscriptionLoading}
                  >
                    {subscriptionLoading
                      ? "Megnyitás..."
                      : "Előfizetés kezelése"}
                  </button>

                  {subscriptionStatus.cancelAtPeriodEnd ? (
                    <button
                      type="button"
                      className="secondary-button"
                      onClick={() =>
                        void handleReactivateSubscription()
                      }
                      disabled={subscriptionLoading}
                    >
                      {subscriptionLoading
                        ? "Visszakapcsolás..."
                        : "Megújítás visszakapcsolása"}
                    </button>
                  ) : (
                    <button
                      type="button"
                      className="secondary-button"
                      onClick={() =>
                        void handleCancelSubscription()
                      }
                      disabled={subscriptionLoading}
                    >
                      {subscriptionLoading
                        ? "Lemondás..."
                        : "Előfizetés lemondása"}
                    </button>
                  )}
                </div>
              )}
            </div>
          </section>
        )}

        <section className="card profile-account-card">
          <div>
            <h2>Fiók</h2>

            <p className="info-text">
              Itt tudsz kijelentkezni a VolleyMind alkalmazásból.
            </p>
          </div>

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