export interface TournamentListItem {
  id: number;
  name: string;
  location: string;
  startDate: string;
  endDate: string | null;
  registrationDeadline: string;
  maxTeams: number;
  registeredTeams: number;
  status: string;
  organizerName: string;
}

export interface TournamentDetails {
  id: number;
  organizerUserId: number;
  organizerName: string;
  name: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string | null;
  registrationDeadline: string;
  maxTeams: number;
  registeredTeams: number;
  format: string;
  status: string;
  createdAt: string;
}

export interface TournamentEntryListItem {
  entryId: number;
  teamId: number;
  teamName: string;
  player1UserId: number;
  player1Name: string;
  player2UserId: number;
  player2Name: string;
  registeredAt: string;
}

export interface TournamentMatchSet {
  setNumber: number;
  team1Points: number;
  team2Points: number;
}

export interface TournamentMatch {
  id: number;
  matchNumber: number;
  stage: string;

  poolId: number | null;
  poolName: string | null;

  team1EntryId: number | null;
  team1Name: string | null;

  team2EntryId: number | null;
  team2Name: string | null;

  team1SourceLabel: string;
  team2SourceLabel: string;

  team1SetsWon: number | null;
  team2SetsWon: number | null;

  winnerEntryId: number | null;
  winnerTeamName: string | null;

  isAutomaticResult: boolean;
  status: string;

  sets: TournamentMatchSet[];

  team1SourceMatchId: number | null;
  team2SourceMatchId: number | null;
}

export interface TournamentBracket {
  tournamentId: number;
  tournamentName: string;

  poolMatches: TournamentMatch[];
  roundOf12Matches: TournamentMatch[];
  quarterfinalMatches: TournamentMatch[];
  semifinalMatches: TournamentMatch[];

  bronzeMatch: TournamentMatch | null;
  finalMatch: TournamentMatch | null;
}

export interface TournamentFinalResult {
  tournamentId: number;
  tournamentName: string;

  firstPlaceEntryId: number;
  firstPlaceTeamName: string;

  secondPlaceEntryId: number;
  secondPlaceTeamName: string;

  thirdPlaceEntryId: number;
  thirdPlaceTeamName: string;

  fourthPlaceEntryId: number;
  fourthPlaceTeamName: string;
}

export interface SubmitTournamentMatchSetResult {
  team1Points: number;
  team2Points: number;
}

export interface SubmitTournamentMatchResultRequest {
  sets: SubmitTournamentMatchSetResult[];
}

export interface TournamentMatchResultResponse {
  matchId: number;
  matchNumber: number;
  stage: string;

  team1EntryId: number;
  team1Name: string;

  team2EntryId: number;
  team2Name: string;

  team1SetsWon: number;
  team2SetsWon: number;

  winnerEntryId: number;
  winnerTeamName: string;

  loserEntryId: number;
  loserTeamName: string;

  status: string;
}

export interface CreateTournamentRequest {
  name: string;
  description: string;
  location: string;
  startDate: string;
  endDate: string | null;
  registrationDeadline: string;
  maxTeams: number;
}

export interface CreateTeamInvitationRequest {
  tournamentId: number;
  playerCode: string;
}

export interface TeamInvitation {
  id: number;
  tournamentId: number;
  tournamentName: string;

  inviterUserId: number;
  inviterName: string;
  inviterPlayerCode: string;

  invitedUserId: number;
  invitedName: string;
  invitedPlayerCode: string;

  status: string;

  createdAt: string;
  respondedAt: string | null;
}

export interface TournamentEntryResponse {
  entryId: number;
  tournamentId: number;
  tournamentName: string;

  teamId: number;
  teamName: string;

  player1UserId: number;
  player1Name: string;

  player2UserId: number;
  player2Name: string;

  registeredAt: string;
}

export interface TournamentPoolSlot {
  slotNumber: number;
  entryId: number | null;
  teamId: number | null;
  teamName: string | null;
  isBye: boolean;
  finalRank: number | null;
}

export interface TournamentPool {
  poolId: number;
  name: string;
  poolNumber: number;
  slots: TournamentPoolSlot[];
}

export interface TournamentDrawResponse {
  tournamentId: number;
  tournamentName: string;
  pools: TournamentPool[];
  createdMatches: number;
}