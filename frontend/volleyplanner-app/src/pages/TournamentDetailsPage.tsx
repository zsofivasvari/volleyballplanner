import {
  useEffect,
  useMemo,
  useState,
} from "react";

import axios from "axios";

import {
  Link,
  useParams,
} from "react-router-dom";

import AppLayout from "../components/layout/AppLayout";

import {
  tournamentService,
} from "../services/tournamentService";

import type {
  TournamentBracket,
  TournamentDetails,
  TournamentEntryListItem,
  TournamentFinalResult,
  TournamentMatch,
} from "../types/tournament";

type TournamentTab =
  | "overview"
  | "pools"
  | "knockout"
  | "results";

function TournamentDetailsPage() {
  const { id } = useParams();

  const tournamentId = Number(id);

  const [
    tournament,
    setTournament,
  ] = useState<TournamentDetails | null>(
    null
  );

  const [
    entries,
    setEntries,
  ] = useState<TournamentEntryListItem[]>(
    []
  );

  const [
    bracket,
    setBracket,
  ] = useState<TournamentBracket | null>(
    null
  );

  const [
    finalResult,
    setFinalResult,
  ] = useState<TournamentFinalResult | null>(
    null
  );

  const [
    activeTab,
    setActiveTab,
  ] = useState<TournamentTab>("overview");

  const [
    loading,
    setLoading,
  ] = useState(true);

  const [
    error,
    setError,
  ] = useState("");

  useEffect(() => {
    if (
      !Number.isInteger(tournamentId) ||
      tournamentId <= 0
    ) {
      setError(
        "Érvénytelen versenyazonosító."
      );

      setLoading(false);

      return;
    }

    const loadData = async () => {
      try {
        setLoading(true);
        setError("");

        const [
          details,
          tournamentEntries,
        ] = await Promise.all([
          tournamentService.getById(
            tournamentId
          ),
          tournamentService.getEntries(
            tournamentId
          ),
        ]);

        setTournament(details);
        setEntries(tournamentEntries);

        try {
          const bracketData =
            await tournamentService.getBracket(
              tournamentId
            );

          setBracket(bracketData);
        } catch (bracketError) {
          console.log(
            "A bracket még nem érhető el.",
            bracketError
          );

          setBracket(null);
        }

        if (
          details.status === "Completed"
        ) {
          try {
            const result =
              await tournamentService.getFinalResult(
                tournamentId
              );

            setFinalResult(result);
          } catch (resultError) {
            console.error(resultError);
          }
        }
      } catch (err) {
        console.error(err);

        if (axios.isAxiosError(err)) {
          setError(
            err.response?.data?.message ??
              "Nem sikerült betölteni a versenyt."
          );
        } else {
          setError(
            "Nem sikerült betölteni a versenyt."
          );
        }
      } finally {
        setLoading(false);
      }
    };

    void loadData();
  }, [tournamentId]);

  const poolGroups = useMemo(() => {
    const groups: Record<
      string,
      TournamentMatch[]
    > = {};

    bracket?.poolMatches.forEach(
      match => {
        const poolName =
          match.poolName ?? "?";

        if (!groups[poolName]) {
          groups[poolName] = [];
        }

        groups[poolName].push(match);
      }
    );

    return groups;
  }, [bracket]);

  const knockoutTree = useMemo(() => {
    if (!bracket) {
      return null;
    }

    const roundOf12ById = new Map(
      bracket.roundOf12Matches.map(match => [
        match.id,
        match,
      ])
    );

    const quarterfinalById = new Map(
      bracket.quarterfinalMatches.map(match => [
        match.id,
        match,
      ])
    );

    const semifinalBranches =
      bracket.semifinalMatches.map(semifinal => {
        const quarterfinalIds = [
          semifinal.team1SourceMatchId,
          semifinal.team2SourceMatchId,
        ].filter(
          (id): id is number => id !== null
        );

        const quarterfinals =
          quarterfinalIds
            .map(id => quarterfinalById.get(id))
            .filter(
              (match): match is TournamentMatch =>
                match !== undefined
            );

        const branches = quarterfinals.map(
          quarterfinal => {
            const roundOf12Id =
              quarterfinal.team1SourceMatchId ??
              quarterfinal.team2SourceMatchId;

            const roundOf12Match =
              roundOf12Id !== null
                ? roundOf12ById.get(
                    roundOf12Id
                  ) ?? null
                : null;

            return {
              roundOf12Match,
              quarterfinal,
            };
          }
        );

        return {
          semifinal,
          branches,
        };
      });

    return {
      semifinalBranches,
      finalMatch: bracket.finalMatch,
      bronzeMatch: bracket.bronzeMatch,
    };
  }, [bracket]);

  const getPoolStandings = (
    matches: TournamentMatch[]
  ) => {
    const winnersMatch =
      matches.find(
        match =>
          match.stage === "PoolWinners"
      );

    const losersMatch =
      matches.find(
        match =>
          match.stage === "PoolLosers"
      );

    const standings: {
      rank: number;
      entryId: number;
      teamName: string;
    }[] = [];

    if (
      winnersMatch?.status === "Completed" &&
      winnersMatch.winnerEntryId &&
      winnersMatch.winnerTeamName
    ) {
      standings.push({
        rank: 1,
        entryId:
          winnersMatch.winnerEntryId,
        teamName:
          winnersMatch.winnerTeamName,
      });

      const secondEntryId =
        winnersMatch.team1EntryId ===
        winnersMatch.winnerEntryId
          ? winnersMatch.team2EntryId
          : winnersMatch.team1EntryId;

      const secondTeamName =
        winnersMatch.team1EntryId ===
        winnersMatch.winnerEntryId
          ? winnersMatch.team2Name
          : winnersMatch.team1Name;

      if (
        secondEntryId &&
        secondTeamName
      ) {
        standings.push({
          rank: 2,
          entryId: secondEntryId,
          teamName: secondTeamName,
        });
      }
    }

    if (
      losersMatch?.status === "Completed" &&
      losersMatch.winnerEntryId &&
      losersMatch.winnerTeamName
    ) {
      standings.push({
        rank: 3,
        entryId:
          losersMatch.winnerEntryId,
        teamName:
          losersMatch.winnerTeamName,
      });

      const fourthEntryId =
        losersMatch.team1EntryId ===
        losersMatch.winnerEntryId
          ? losersMatch.team2EntryId
          : losersMatch.team1EntryId;

      const fourthTeamName =
        losersMatch.team1EntryId ===
        losersMatch.winnerEntryId
          ? losersMatch.team2Name
          : losersMatch.team1Name;

      if (
        fourthEntryId &&
        fourthTeamName
      ) {
        standings.push({
          rank: 4,
          entryId: fourthEntryId,
          teamName: fourthTeamName,
        });
      }
    }

    return standings.sort(
      (a, b) => a.rank - b.rank
    );
  };

  const formatDateTime = (
    value: string
  ) => {
    return new Date(
      value
    ).toLocaleString(
      "hu-HU",
      {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      }
    );
  };

  const getStatusLabel = (
    status: string
  ) => {
    switch (status) {
      case "RegistrationOpen":
        return "Nevezés nyitva";

      case "RegistrationClosed":
        return "Nevezés lezárva";

      case "PoolStage":
        return "Csoportkör";

      case "KnockoutStage":
        return "Kieséses szakasz";

      case "Completed":
        return "Befejezett";

      case "Cancelled":
        return "Törölve";

      case "Ready":
        return "Lejátszható";

      case "WaitingForTeams":
        return "Résztvevőkre vár";

      default:
        return status;
    }
  };

  const renderMatch = (
    match: TournamentMatch
  ) => {
    const getDisplayTeamName = (
      teamName: string | null,
      sourceLabel: string,
      entryId: number | null,
      isAutomaticResult: boolean
    ) => {
      if (teamName) {
        return teamName;
      }

      if (
        isAutomaticResult &&
        entryId === null
      ) {
        return "BYE";
      }

      if (sourceLabel) {
        return sourceLabel;
      }

      return "Még nem ismert";
    };

    const team1 = getDisplayTeamName(
      match.team1Name,
      match.team1SourceLabel,
      match.team1EntryId,
      match.isAutomaticResult
    );

    const team2 = getDisplayTeamName(
      match.team2Name,
      match.team2SourceLabel,
      match.team2EntryId,
      match.isAutomaticResult
    );

    return (
      <article
        key={match.id}
        className="tournament-match-card"
      >
        <div className="tournament-match-header">
          <strong>
            #{match.matchNumber}
          </strong>

          <span>
            {getStatusLabel(
              match.status
            )}
          </span>
        </div>

        <div className="tournament-match-team">
          <span>
            {team1 || "Még nem ismert"}
          </span>

          <strong>
            {match.team1SetsWon ?? "-"}
          </strong>
        </div>

        <div className="tournament-match-team">
          <span>
            {team2 || "Még nem ismert"}
          </span>

          <strong>
            {match.team2SetsWon ?? "-"}
          </strong>
        </div>

        {match.sets.length > 0 && (
          <div className="tournament-set-list">
            {match.sets.map(set => (
              <span key={set.setNumber}>
                {set.team1Points}
                {" : "}
                {set.team2Points}
              </span>
            ))}
          </div>
        )}

        {match.isAutomaticResult && (
          <small className="tournament-auto-result">
            Automatikus eredmény
          </small>
        )}
      </article>
    );
  };

  if (loading) {
    return (
      <AppLayout title="Verseny">
        <div className="card">
          <p className="info-text">
            Verseny betöltése...
          </p>
        </div>
      </AppLayout>
    );
  }

  if (
    error ||
    !tournament
  ) {
    return (
      <AppLayout title="Verseny">
        <div className="card">
          <p className="error-text">
            {error ||
              "A verseny nem található."}
          </p>

          <Link
            to="/tournaments"
            className="secondary-button tournament-back-link"
          >
            Vissza a versenyekhez
          </Link>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout
      title={tournament.name}
      subtitle={`${tournament.location} • ${getStatusLabel(
        tournament.status
      )}`}
    >
      <div className="tournament-details-actions">
        <Link
          to="/tournaments"
          className="secondary-button tournament-back-link"
        >
          ← Vissza
        </Link>
      </div>

      <div className="tournament-tabs">
        <button
          type="button"
          className={
            activeTab === "overview"
              ? "active"
              : ""
          }
          onClick={() =>
            setActiveTab("overview")
          }
        >
          Áttekintés
        </button>

        <button
          type="button"
          className={
            activeTab === "pools"
              ? "active"
              : ""
          }
          onClick={() =>
            setActiveTab("pools")
          }
        >
          Csoportok
        </button>

        <button
          type="button"
          className={
            activeTab === "knockout"
              ? "active"
              : ""
          }
          onClick={() =>
            setActiveTab("knockout")
          }
        >
          Kieséses szakasz
        </button>

        <button
          type="button"
          className={
            activeTab === "results"
              ? "active"
              : ""
          }
          onClick={() =>
            setActiveTab("results")
          }
        >
          Végeredmény
        </button>
      </div>

      {activeTab === "overview" && (
        <div className="tournament-overview-grid">
          <section className="card tournament-info-card">
            <div className="tournament-section-heading">
              <span>
                VERSENYADATOK
              </span>

              <h2>
                {tournament.name}
              </h2>
            </div>

            <p className="tournament-description">
              {tournament.description ||
                "Nincs megadott leírás."}
            </p>

            <div className="tournament-info-grid">
              <div>
                <span>
                  Helyszín
                </span>

                <strong>
                  {tournament.location}
                </strong>
              </div>

              <div>
                <span>
                  Kezdés
                </span>

                <strong>
                  {formatDateTime(
                    tournament.startDate
                  )}
                </strong>
              </div>

              <div>
                <span>
                  Befejezés
                </span>

                <strong>
                  {tournament.endDate
                    ? formatDateTime(
                        tournament.endDate
                      )
                    : "Nincs megadva"}
                </strong>
              </div>

              <div>
                <span>
                  Nevezési határidő
                </span>

                <strong>
                  {formatDateTime(
                    tournament.registrationDeadline
                  )}
                </strong>
              </div>

              <div>
                <span>
                  Szervező
                </span>

                <strong>
                  {tournament.organizerName}
                </strong>
              </div>

              <div>
                <span>
                  Csapatok
                </span>

                <strong>
                  {tournament.registeredTeams}
                  {" / "}
                  {tournament.maxTeams}
                </strong>
              </div>

              <div>
                <span>
                  Formátum
                </span>

                <strong>
                  Modified Pool Play
                </strong>
              </div>

              <div>
                <span>
                  Állapot
                </span>

                <strong>
                  {getStatusLabel(
                    tournament.status
                  )}
                </strong>
              </div>
            </div>
          </section>

          <section className="card tournament-entry-card">
            <div className="tournament-section-heading">
              <span>
                NEVEZÉSEK
              </span>

              <h2>
                Nevezett csapatok
              </h2>
            </div>

            {entries.length === 0 ? (
              <p className="info-text">
                Még nincs nevezett csapat.
              </p>
            ) : (
              <div className="tournament-entry-list">
                {entries.map(
                  (entry, index) => (
                    <div
                      key={entry.entryId}
                      className="tournament-entry-item"
                    >
                      <span className="tournament-entry-number">
                        {index + 1}
                      </span>

                      <div>
                        <strong>
                          {entry.teamName}
                        </strong>

                        <p>
                          {entry.player1Name}
                          {" / "}
                          {entry.player2Name}
                        </p>
                      </div>
                    </div>
                  )
                )}
              </div>
            )}
          </section>
        </div>
      )}

      {activeTab === "pools" && (
        <>
          {!bracket ||
          bracket.poolMatches.length ===
            0 ? (
            <div className="card">
              <p className="info-text">
                A csoportkör még nem
                került kisorsolásra.
              </p>
            </div>
          ) : (
            <div className="tournament-pool-grid">
              {Object.entries(
                poolGroups
              ).map(
                ([
                  poolName,
                  matches,
                ]) => {
                  const standings =
                    getPoolStandings(matches);

                  return (
                    <section
                      key={poolName}
                      className="card tournament-pool-card"
                    >
                      <div className="tournament-pool-title">
                        <span>
                          Csoport
                        </span>

                        <strong>
                          {poolName}
                        </strong>
                      </div>

                      <div className="tournament-pool-section">
                        <h3>
                          Csoportállás
                        </h3>

                        {standings.length === 0 ? (
                          <p className="info-text">
                            A végleges sorrend a
                            csoportmeccsek lezárása
                            után jelenik meg.
                          </p>
                        ) : (
                          <div className="tournament-standing-list">
                            {standings.map(
                              standing => (
                                <div
                                  key={
                                    standing.entryId
                                  }
                                  className="tournament-standing-row"
                                >
                                  <span
                                    className={`tournament-standing-rank tournament-standing-rank-${standing.rank}`}
                                  >
                                    {standing.rank}.
                                  </span>

                                  <strong>
                                    {
                                      standing.teamName
                                    }
                                  </strong>

                                  {standing.rank <=
                                    3 && (
                                    <small>
                                      {standing.rank ===
                                      1
                                        ? "Negyeddöntő"
                                        : "Round of 12"}
                                    </small>
                                  )}
                                </div>
                              )
                            )}
                          </div>
                        )}
                      </div>

                      <div className="tournament-pool-section">
                        <h3>
                          Mérkőzések
                        </h3>

                        <div className="tournament-match-list">
                          {matches.map(
                            renderMatch
                          )}
                        </div>
                      </div>
                    </section>
                  );
                }
              )}
            </div>
          )}
        </>
      )}

      {activeTab === "knockout" && (
        <>
          {!bracket ||
          !knockoutTree ||
          bracket.roundOf12Matches.length === 0 ? (
            <div className="card">
              <p className="info-text">
                A kieséses szakasz még nem kezdődött el.
              </p>
            </div>
          ) : (
            <div className="knockout-tree-scroll">
              <div className="knockout-tree">
                <div className="knockout-tree-headings">
                  <span>Legjobb 12</span>
                  <span>Negyeddöntő</span>
                  <span>Elődöntő</span>
                  <span>Döntő</span>
                </div>

                <div className="knockout-tree-board">
                  {knockoutTree.semifinalBranches.map(
                    (semifinalBranch, sfIndex) => (
                      <div
                        key={
                          semifinalBranch.semifinal.id
                        }
                        className={`knockout-sf-group knockout-sf-group-${
                          sfIndex + 1
                        }`}
                      >
                        {semifinalBranch.branches.map(
                          (branch, branchIndex) => (
                            <div
                              key={
                                branch.quarterfinal.id
                              }
                              className={`knockout-qf-branch knockout-qf-branch-${
                                branchIndex + 1
                              }`}
                            >
                              <div className="knockout-node knockout-node-r12">
                                {branch.roundOf12Match
                                  ? renderMatch(
                                      branch.roundOf12Match
                                    )
                                  : (
                                    <div className="tournament-match-card">
                                      <p className="info-text">
                                        Nincs R12
                                        forrásmeccs.
                                      </p>
                                    </div>
                                  )}
                              </div>

                              <div className="knockout-connector-r12-qf" />

                              <div className="knockout-node knockout-node-qf">
                                {renderMatch(
                                  branch.quarterfinal
                                )}
                              </div>
                            </div>
                          )
                        )}

                        <div className="knockout-qf-merge">
                          <span className="knockout-qf-vertical" />
                          <span className="knockout-qf-horizontal" />
                        </div>

                        <div className="knockout-node knockout-node-sf">
                          {renderMatch(
                            semifinalBranch.semifinal
                          )}
                        </div>
                      </div>
                    )
                  )}

                  <div className="knockout-sf-merge">
                    <span className="knockout-sf-vertical" />
                    <span className="knockout-sf-horizontal" />
                  </div>

                  <div className="knockout-final-area">
                    <div className="knockout-final-main">
                      <h3>Döntő</h3>

                      {knockoutTree.finalMatch &&
                        renderMatch(
                          knockoutTree.finalMatch
                        )}
                    </div>

                    <div className="knockout-bronze-main">
                      <h3>Bronzmérkőzés</h3>

                      <small>
                        Az elődöntők vesztesei
                      </small>

                      {knockoutTree.bronzeMatch &&
                        renderMatch(
                          knockoutTree.bronzeMatch
                        )}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </>
      )}

      {activeTab === "results" && (
        <>
          {!finalResult ? (
            <div className="card">
              <p className="info-text">
                A végeredmény a verseny
                befejezése után jelenik
                meg.
              </p>
            </div>
          ) : (
            <div className="tournament-result-grid">
              <article className="card tournament-result-card tournament-result-first">
                <span>
                  🥇
                </span>

                <small>
                  1. hely
                </small>

                <strong>
                  {
                    finalResult.firstPlaceTeamName
                  }
                </strong>
              </article>

              <article className="card tournament-result-card">
                <span>
                  🥈
                </span>

                <small>
                  2. hely
                </small>

                <strong>
                  {
                    finalResult.secondPlaceTeamName
                  }
                </strong>
              </article>

              <article className="card tournament-result-card">
                <span>
                  🥉
                </span>

                <small>
                  3. hely
                </small>

                <strong>
                  {
                    finalResult.thirdPlaceTeamName
                  }
                </strong>
              </article>

              <article className="card tournament-result-card">
                <span>
                  4.
                </span>

                <small>
                  4. hely
                </small>

                <strong>
                  {
                    finalResult.fourthPlaceTeamName
                  }
                </strong>
              </article>
            </div>
          )}
        </>
      )}
    </AppLayout>
  );
}

export default TournamentDetailsPage;